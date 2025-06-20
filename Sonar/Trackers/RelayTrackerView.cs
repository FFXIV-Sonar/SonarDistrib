﻿using Sonar.Extensions;
using Sonar.Relays;
using Sonar.Services;
using Sonar.Utilities;
using SonarUtils;
using SonarUtils.Collections;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar.Trackers
{
    public sealed partial class RelayTrackerView<T> : RelayTrackerBase<T>, IRelayTrackerView<T> where T : Relay
    {
        private readonly Lock _processLock = new();
        private readonly ConcurrentQueue<RelayState<T>> _statesQueue = new();
        private IEnumerator<RelayState<T>>? _trackerEnumerator;
        private int _trackerCount;
        private int _nextMultiplier;

        private readonly Predicate<RelayState<T>> _predicate;
        private readonly string _indexKey;

        public SonarClient Client => this.Tracker.Client;
        private IRelayTracker<T> Tracker { get; }

        public ViewScanRate ScanRate { get; set; } = ViewScanRate.Normal;
        public ViewScanAcceleration ScanAcceleration { get; set; } = ViewScanAcceleration.Triangular;

        public RelayTrackerData<T> Data { get; }
        IRelayTrackerData IRelayTracker.Data => this.Data;
        private static int GetCountToProcess(int count, ViewScanRate scanRate)
        {
            var result = Math.Sqrt(count);
            if (scanRate is ViewScanRate.Fast) result *= Math.Pow(count, 1.0 / 8.0);
            else if (scanRate is ViewScanRate.Slow) result /= Math.Pow(count, 1.0 / 8.0);
            else if (scanRate is ViewScanRate.Disabled) return 0;

            var minimum = 16;
            if (scanRate is ViewScanRate.Fast) minimum *= 2;
            else if (scanRate is ViewScanRate.Slow) minimum /= 2;

            return (int)Math.Max(Math.Min(Math.Max(1, count), minimum), result);
        }

        private static int GetMultiplier(int multiplier, ViewScanAcceleration acceleration)
        {
            return acceleration switch
            {
                ViewScanAcceleration.None => 1,
                ViewScanAcceleration.Linear => multiplier,
                ViewScanAcceleration.Triangular => AG.MathUtils.Triangular(multiplier),
                ViewScanAcceleration.Exponential => multiplier * multiplier,
                _ => multiplier
            };
        }

        internal RelayTrackerView(IRelayTracker<T> tracker, Predicate<RelayState<T>>? predicate, string indexKey = "all", bool indexing = false)
        {
            this.Tracker = tracker;
            this.Data = new(indexing);
            this._indexKey = StringUtils.Intern(indexKey);

            this._predicate = predicate is not null ? state =>
            {
                try
                {
                    return predicate(state);
                }
                catch (Exception ex)
                {
                    this.Client.LogError(ex, $"Exception in {nameof(RelayTrackerView<T>)} predicate");
                    return false;
                }
            }
            : state => true;

            this.Client.Tick += this.PerformTick;
            this.Tracker.Found += this.Tracker_Found;
            this.Tracker.Updated += this.Tracker_Updated;
            this.Tracker.Dead += this.Tracker_Updated; // NOTE: This is not a typo
            this.Tracker.Data.Added += this.Data_Added; // NOTE: Do not add "Added" states as they'll cause the "Found" event not to be dispatched properly.
            this.Tracker.Data.Removed += this.Data_Removed;
            this.Tracker.Data.Cleared += this.Data_Cleared;

            this.ProcessAllStates();
        }

        private void Data_Cleared(IRelayTrackerData<T> obj)
        {
            lock (this._processLock)
            {
                this._statesQueue.Clear();
                this._trackerCount = 0;
                this._trackerEnumerator = null;
                this.Data.Clear();
            }
        }

        private void Data_Added(IRelayTrackerData<T> arg1, RelayState<T> arg2) => Interlocked.Increment(ref this._nextMultiplier);

        private void Data_Removed(IRelayTrackerData<T> arg1, RelayState<T> arg2) => this.Data.Remove(arg2);

        private void Tracker_Found(RelayState<T> obj) => this.ProcessState(obj, true);

        private void Tracker_Updated(RelayState<T> obj) => this.ProcessState(obj, false);

        /// <summary>Precondition: this._processLock is acquired</summary>
        private RelayState<T>? GetNextFromTracker()
        {
            if (this._trackerEnumerator is null || !this._trackerEnumerator.MoveNext())
            {
                this._trackerEnumerator?.Dispose();
                var states = this.Tracker.Data.GetIndexStates(this._indexKey);
                this._trackerEnumerator = states.GetEnumerator();
                this._trackerCount = states.Count;
                if (!this._trackerEnumerator.MoveNext()) return null;
            }
            return this._trackerEnumerator.Current;
        }

        private void PerformTick(SonarClient client) => Task.Run(this.ProcessStates);

        private void ProcessStates()
        {
            if (!this._processLock.TryEnter()) return;
            try
            {
                var viewSize = this.Data.Count;
                var queueSize = this._statesQueue.Count;
                var trackerSize = this._trackerCount;
                var trackerData = this.Tracker.Data;
                var realTrackerSize = trackerData.Count;

                var multiplier = GetMultiplier(Interlocked.Exchange(ref this._nextMultiplier, 0) + 1, this.ScanAcceleration);

                var count = GetCountToProcess(viewSize + queueSize + trackerSize + realTrackerSize, this.ScanRate);
                var queueIncrement = Math.Min(count, queueSize);
                var trackerIncrement = Math.Max(Math.Min(count, trackerSize), 1); // Special cased as it needs at least 1 to refresh the enumerator

                var queueCount = Math.Min(queueIncrement * multiplier, queueSize);
                var trackerCount = Math.Max(Math.Min(trackerIncrement * multiplier, trackerSize), 1);

                trackerSize = Math.Max(trackerSize, realTrackerSize); // For loop purposes

                // Part 1: Current states - Slowly remove states that no longer pass the predicate
                for (var i = 0; i < queueCount && i < queueSize; i++)
                {
                    if (!this._statesQueue.TryDequeue(out var state)) break;

                    // Check if the state exists in the tracker.
                    // "all" index key is a special case as its a Values enumerable and should be treated as such
                    var exists = this._indexKey is "all" ? trackerData.States.ContainsKey(state.RelayKey) : trackerData.GetIndexStates(this._indexKey).Contains(state);
                    if (exists && this._predicate(state)) this._statesQueue.Enqueue(state);
                    else if (this.Data.Remove(state)) Interlocked.Increment(ref this._nextMultiplier);
                }

                // Part 2: Tracker states - Silently add states that were added via Relay Data Request
                for (var i = 0; i < trackerCount && i < trackerSize + 1; i++)
                {
                    var state = this.GetNextFromTracker(); // NOTE: Only states with the index key will appear
                    if (state is null) break;
                    if (trackerData.States.ContainsKey(state.RelayKey) && this._predicate(state))
                    {
                        if (this.Data.TryAddState(state))
                        {
                            this._statesQueue.Enqueue(state);
                            Interlocked.Increment(ref this._nextMultiplier);
                        }
                    }
                    else if (this.Data.Remove(state))
                    {
                        Interlocked.Increment(ref this._nextMultiplier);
                    }
                }
            }
            finally
            {
                this._processLock.Exit();
            }
        }

        private void ProcessAllStates()
        {
            this.Tracker.Data.GetIndexStates(this._indexKey)
                .Where(state => this._predicate(state))
                .Where(this.Data.TryAddState)
                .ForEach(this._statesQueue.Enqueue);
        }

        private void ProcessState(RelayState<T> state, bool isFound)
        {
            if (this._indexKey != "all" && !state.IndexKeysCore.Contains(this._indexKey)) return;

            if (!this._predicate(state))
            {
                this.Data.Remove(state);
                return;
            }

            if (this.Data.TryAddState(state))
            {
                this._statesQueue.Enqueue(state);
                isFound = true;
            }

            this.DispatchEvents(state, isFound);
        }

        public bool FeedRelay(T relay) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");
        public bool FeedRelay(Relay relay) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");
        public bool FeedRelayInternal(T relay) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");
        public bool FeedRelayInternal(Relay relay) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");
        public void FeedRelays(IEnumerable<T> relays) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");
        public void FeedRelays(IEnumerable<Relay> relays) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");
        public void FeedRelaysInternal(IEnumerable<T> relays) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");
        public void FeedRelaysInternal(IEnumerable<Relay> relays) => throw new NotSupportedException($"{nameof(RelayTrackerView<T>)} does not support this operation");

        public IRelayTrackerView<T> CreateView(Predicate<RelayState<T>>? predicate = null, string index = "all", bool indexing = false)
        {
            return new RelayTrackerView<T>(this, predicate, index, indexing);
        }

        public IRelayTrackerView CreateView(Predicate<RelayState>? predicate = null, string index = "all", bool indexing = false)
        {
            return new RelayTrackerView<T>(this, predicate, index, indexing);
        }

        public void Dispose()
        {
            this.Client.Tick -= this.PerformTick;
            this.Tracker.Found -= this.Tracker_Found;
            this.Tracker.Updated -= this.Tracker_Updated;
            this.Tracker.Dead -= this.Tracker_Updated; // not a typo
            this.Tracker.Data.Added -= this.Data_Added;
            this.Tracker.Data.Removed -= this.Data_Removed;
            this.Tracker.Data.Cleared -= this.Data_Cleared;
        }
    }
}
