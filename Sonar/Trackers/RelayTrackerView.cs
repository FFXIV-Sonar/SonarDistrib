using Sonar.Extensions;
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
        private readonly object _processLock = new();
        private readonly ConcurrentQueue<RelayState<T>> _statesQueue = new();
        private IEnumerator<RelayState<T>>? _trackerEnumerator;
        private int _trackerCount;
        private int _addedCount;

        private readonly Predicate<RelayState<T>> _predicate;
        private readonly string _indexKey;

        public SonarClient Client => this.Tracker.Client;
        private IRelayTracker<T> Tracker { get; }

        public RelayTrackerData<T> Data { get; }
        IRelayTrackerData IRelayTracker.Data => this.Data;

        private static int GetCountToProcess(int count) => (int)Math.Max(Math.Min(Math.Max(1, count), 16), Math.Sqrt(count));

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

        private void Data_Added(IRelayTrackerData<T> arg1, RelayState<T> arg2) => Interlocked.Increment(ref this._addedCount);

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
            if (!Monitor.TryEnter(this._processLock)) return;
            try
            {
                var viewSize = this.Data.Count;
                var queueSize = this._statesQueue.Count;
                var trackerSize = this._trackerCount;
                var realTrackerSize = this.Tracker.Data.Count;

                var multiplier = Interlocked.Exchange(ref this._addedCount, 0);
                if (multiplier < 1) multiplier = 1;

                //var count = GetCountToProcess(this.Data.Count) + GetCountToProcess(trackerSize) + GetCountToProcess(this.Data.Count + trackerSize) + GetCountToProcess(this._statesQueue.Count);

                var count = GetCountToProcess(viewSize + queueSize + trackerSize + realTrackerSize);
                var queueIncrement = Math.Min(count, queueSize);
                var trackerIncrement = Math.Max(Math.Min(count, trackerSize), 1); // Special cased as it needs at least 1 to refresh the enumerator
                var queueCount = Math.Min(queueIncrement * multiplier, queueSize);
                var trackerCount = Math.Max(Math.Min(trackerIncrement * multiplier, trackerSize), 1);

                trackerSize = Math.Max(trackerSize, realTrackerSize); // For loop purposes

                var entries = this.Tracker.Data.GetIndexStates(this._indexKey);

                // Part 1: Current states - Slowly remove states that no longer pass the predicate
                for (var i = 0; i < queueCount * multiplier && i < queueSize; i++)
                {
                    if (!this._statesQueue.TryDequeue(out var state)) break;

                    // Check if the state exists in the tracker.
                    // "all" index key is a special case as its a Values enumerable and should be treated as such
                    var exists = this._indexKey.Equals("all") ? this.Tracker.Data.States.ContainsKey(state.RelayKey) : entries.Contains(state);

                    if (exists && this._predicate(state)) this._statesQueue.Enqueue(state);
                    else if (this.Data.Remove(state)) multiplier++;
                }

                // Part 2: Tracker states - Silently add states that were added via Relay Data Request
                for (var i = 0; i < trackerCount * multiplier && i < trackerSize + 1; i++)
                {
                    var state = this.GetNextFromTracker(); // NOTE: Only states with the index key will appear
                    if (state is null) break;
                    if (this._predicate(state))
                    {
                        if (this.Data.TryAddState(state))
                        {
                            this._statesQueue.Enqueue(state);
                            multiplier++;
                        }
                    }
                    else if (this.Data.Remove(state))
                    {
                        multiplier++;
                    }
                }
            }
            finally
            {
                Monitor.Exit(this._processLock);
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
            if (this._indexKey != "all" && !state.IndexKeys.Contains(this._indexKey)) return;

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
