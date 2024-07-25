using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Messages;
using Sonar.Models;
using SonarUtils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sonar.Utilities.UnixTimeHelper;
using System.Threading;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Config;
using System.ComponentModel;
using System.Diagnostics;
using Sonar.Threading;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Sonar.Extensions;
using Sonar.Connections;
using Sonar.Sockets;
using Sonar.Relays;
using SonarUtils.Collections;
using Sonar.Utilities;
using SonarUtils.Text;

namespace Sonar.Trackers
{
    /// <summary>Handles, receives and relay hunt tracking information</summary>
    public sealed class RelayTracker<T> : RelayTrackerBase<T>, IRelayTracker<T>, IDisposable where T : Relay
    {
        private static readonly RelayType s_type = RelayUtils.GetRelayType<T>();

        private RelayConfig Config { get; }
        private SonarContributeConfig Contribute { get; }
        public SonarClient Client { get; }
        public RelayTrackers Trackers { get; }
        public RelayTrackerData<T> Data { get; } = new();
        IRelayTrackerData IRelayTracker.Data => this.Data;

        private readonly ConcurrentQueue<T> _relayUpdateQueue = new();
        private readonly ConcurrentHashSetSlim<string> _confirmationRequests = new(comparer: FarmHashStringComparer.Instance);
        private readonly ConcurrentHashSetSlim<string> _lockOn = new(comparer: FarmHashStringComparer.Instance);
        private readonly ConcurrentDictionarySlim<string, double> _lastSeen = new(comparer: FarmHashStringComparer.Instance);

        /// <summary>Dispatch events regardless of jurisdiction settings</summary>
        public bool AlwaysDispatchEvents { get; set; }

        internal RelayTracker(RelayTrackers trackers, RelayConfig config)
        {
            this.Trackers = trackers;
            this.Config = config;
            this.Contribute = trackers.Config.Contribute;
            this.Client = trackers.Client;

            this.Client.Meta.PlayerPlaceChanged += this.Meta_PlayerPlaceChanged;
            this.Client.Tick += this.Client_Tick;
            this.Client.Connection.MessageReceived += this.Client_MessageReceived;
            this.Client.Connection.DisconnectedInternal += this.Client_Disconnected;
            this.Data.Cleared += this.Data_Clear;
        }

        private void Data_Clear(IRelayTrackerData<T> obj)
        {
            this._relayUpdateQueue.Clear();
            this._confirmationRequests.Clear();
            this._lockOn.Clear();
            this._lastSeen.Clear();
        }

        private void Meta_PlayerPlaceChanged(PlayerPosition obj)
        {
            this._confirmationRequests.Clear();
            this._lockOn.Clear();
        }

        private void Client_Disconnected(SonarConnectionManager arg1, ISonarSocket arg2) => this._confirmationRequests.Clear();

        private void Client_MessageReceived(SonarConnectionManager arg1, ISonarMessage message)
        {
            switch (message)
            {
                case RelayState<T> state:
                    this.UpdateState(state, true);
                    break;
                case T relay:
                    this.FeedRelay(relay);
                    break;
                case LockOn<T> lockOn:
                    this._lockOn.Add(lockOn.RelayKey);
                    break;
                case RelayConfirmationSlim<T> confirmation:
                    this._confirmationRequests.Add(confirmation.RelayKey);
                    break;
            }
        }

        private void Client_Tick(SonarClient source)
        {
            if (!this._relayUpdateQueue.IsEmpty)
            {
                if (this.Client.Connection.IsConnected && this.Contribute.Compute(s_type) && this.Client.Modifiers.CanContribute(this.RelayType))
                {
                    var relaysDict = new Dictionary<string, T>();
                    while (this._relayUpdateQueue.TryDequeue(out var item)) relaysDict[item.RelayKey] = item;
                    var relays = new MessageList(relaysDict.Values);
                    this.Client.Connection.SendIfConnected(relays);
                }
                else this._relayUpdateQueue.Clear();
            }
        }

        private bool IsWithinJurisdiction(IReadOnlyDictionary<uint, WorldRow>? worlds, T relay)
        {
            var jurisdiction = this.Config.GetReportJurisdiction(relay.Id);
            return jurisdiction == SonarJurisdiction.All || (this.Client.Meta.PlayerPosition?.IsWithinJurisdiction(relay, jurisdiction, worlds) ?? false);
        }

        private bool IsWithinJurisdiction(IReadOnlyDictionary<uint, WorldRow>? worlds, RelayState<T> state) => this.IsWithinJurisdiction(worlds, state.Relay);

        public bool IsTrackable(T relay)
        {
            return this.Config.TrackAll || this.IsWithinJurisdiction(null, relay);
        }
        public bool IsTrackable(RelayState<T> state) => this.IsTrackable(state.Relay);

        /// <inheritdoc/>
        public bool FeedRelay(T relay)
        {
            // Avoid invalid relays
            if (!relay.IsValid()) return false;
            var localOnly = relay.GetZone()!.LocalOnly;

            // Confirm relay existence if requested
            if (this._confirmationRequests.Count > 0 && this._confirmationRequests.Remove(relay.RelayKey))
            {
                this.Client.Connection.SendIfConnected(new RelayConfirmationSlim<T>() { RelayKey = relay.RelayKey });
            }

            // Local only relay, disconnected or not contributing?
            if (localOnly || !this.Client.Connection.IsConnected || !this.Contribute.Compute(s_type) || !this.Client.Modifiers.AllowContribute!.GetValueOrDefault(this.RelayType, true))
            {
                if (this.IsTrackable(relay) && !((IRelayTracker<T>)this).FeedRelayInternal(relay)) return false;
                if (this._lockOn.Count > 0 && this._lockOn.Contains(relay.RelayKey)) this._relayUpdateQueue.Enqueue(relay);
            }
            else
            {
                this._relayUpdateQueue.Enqueue(relay);
            }
            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FeedRelay(Relay relay) => relay is T trelay && this.FeedRelay(trelay);

        /// <inheritdoc/>
        public void FeedRelays(IEnumerable<T> relays)
        {
            foreach (var relay in relays)
            {
                this.FeedRelay(relay);
            }
        }

        /// <inheritdoc/>
        public void FeedRelays(IEnumerable<Relay> relays)
        {
            foreach (var relay in relays)
            {
                this.FeedRelay(relay);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IRelayTracker<T>.FeedRelayInternal(T relay) => this.UpdateState(relay, null, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IRelayTracker.FeedRelayInternal(Relay relay) => relay is T trelay && this.UpdateState(trelay, null, false);

        [SuppressMessage("Code Smell", "S3241", Justification = "Internal API consistency")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool UpdateState(RelayState<T> newState, bool isFromServer) => this.UpdateState(null, newState, isFromServer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool UpdateState(T? newRelay, RelayState<T>? newState, bool isFromServer)
        {
            // This method is intended to be called only with a newRelay or newState
            Debug.Assert(newRelay is not null ^ newState is not null, "Only a new relay or state must be provided"); // This should not happen

            if (newState is not null)
            {
                newRelay = newState.Relay;
                if (this._lastSeen.TryGetValue(newRelay.RelayKey, out var lastSeen) && newState.LastSeen <= lastSeen) return false;
                this._lastSeen[newState.RelayKey] = newState.LastSeen;
            }
            Debug.Assert(newRelay is not null); // newRelay will never be null from this point forward

            if (isFromServer)
            {
                if (!this.Contribute.Compute(s_type)) this.Client.Connection.SendIfConnected(this.Contribute);
                // Avoid storing non-receivable states
                if (!this.IsTrackable(newRelay!)) return false;
            }

            // Local scope variable declarations
            var isNew = false;

            // Get current time
            var now = SyncedUnixNow;

            // Get and work on the state
            RelayState<T> state;
            do // Expected number of iterations: 2
            {
                // Under normal circumstances this loop will execute at most twice, whicn means
                // that another thread made the value between the TryGet and TryAdd, therefore
                // TryGet will succeed on the second loop. (TryGet = TryGetValue).
                if (this.Data.States.TryGetValue(newRelay.RelayKey, out state!)) break;
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions (Justification: Immediately used after the loop)
                if (this.Data.TryAddState(state = newState ?? new(newRelay, now)))
#pragma warning restore S1121 // Assignments should not be made from within sub-expressions
                {
                    isNew = true;
                    break;
                }
                // Forward-progressing: No Spinwait needed
            } while (true);

            // Process state changes if its not new
            if (!isNew)
            {
                state._lock.Enter(ref isNew); // Reusing isNew as its always false at this point and not modified until the state.IsSameEntity check. No exceptions are expected here.
                try
                {
                    if (newState is not null && newState.LastSeen < state.LastSeen) return false;
                    isNew = !state.IsSameEntity(newRelay); // DispatchEvents uses isNew as a way to tell if its a new entity, which forces it to dispatch Found events
                    if (!isNew && (state.IsDeadInternal() || state.IsSimilarData(newRelay, now))) return false;

                    if (newState is null) state.UpdateWithRelay(newRelay, now, isNew);
                    else state.UpdateWithState(newState);
                }
                finally
                {
                    state._lock.Exit();
                }
            }

            // Successful
            if (this.Client.Modifiers.EnableRelayEventHandlers!.Value && (this.AlwaysDispatchEvents || this.IsWithinJurisdiction(null, state)))
            {
                this.DispatchEvents(state, isNew);
            }
            return true;
        }

        public IRelayTrackerView<T> CreateView(Predicate<RelayState<T>>? predicate = null, string index = "all", bool indexing = false)
        {
            return new RelayTrackerView<T>(this, predicate, index, indexing);
        }

        public IRelayTrackerView CreateView(Predicate<RelayState>? predicate = null, string index = "all", bool indexing = false)
        {
            return new RelayTrackerView<T>(this, predicate, index, indexing);
        }

        #region IDisposable Pattern
        private int disposed; //Interlocked
        public bool IsDisposed => this.disposed == 1;
        private void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1) return;
            if (disposing)
            {
                this.Data.Cleared -= this.Data_Clear;
                this.Client.Tick -= this.Client_Tick;
                this.Client.Connection.MessageReceived -= this.Client_MessageReceived;
                this.Client.Connection.DisconnectedInternal -= this.Client_Disconnected;
                this.Data.Clear();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
