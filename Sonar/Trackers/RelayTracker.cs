﻿using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Messages;
using Sonar.Models;
using Sonar.Utilities;
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
using Sonar.Collections;
using System.Diagnostics.CodeAnalysis;
using Sonar.Extensions;
using Sonar.Connections;
using Sonar.Sockets;
using Sonar.Relays;

namespace Sonar.Trackers
{
    /// <summary>Handles, receives and relay hunt tracking information</summary>
    public abstract partial class RelayTracker<T> : IRelayTracker<T>, IDisposable where T : Relay
    {
        /// <summary>Relay Tracker Configuration. Please access it from SonarClient.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract RelayConfig Config { get; }
        protected SonarClient Client { get; }
        public RelayTrackerData<T> Data { get; } = new();

        private readonly ConcurrentQueue<T> _relayUpdateQueue = new();
        private readonly NonBlocking.NonBlockingHashSet<string> _confirmationRequests = new(comparer: FarmHashStringComparer.Instance);

        /// <summary>Dispatch events regardless of jurisdiction settings</summary>
        public bool AlwaysDispatchEvents { get; set; }

        private protected RelayTracker(SonarClient sonar)
        {
            this.Client = sonar;
            this.Client.Tick += this.Client_Tick;
            this.Client.Connection.MessageReceived += this.Client_MessageReceived;
            this.Client.Connection.DisconnectedInternal += this.Client_Disconnected;
        }

        private void Client_Disconnected(SonarConnectionManager arg1, ISonarSocket arg2) => this._confirmationRequests.Clear();

        private void Client_MessageReceived(SonarConnectionManager arg1, ISonarMessage message)
        {
            switch (message)
            {
                case T relay:
                    this.FeedRelay(relay);
                    break;
                case RelayState<T> state:
                    this.UpdateState(state, true);
                    break;
                case RelayConfirmationSlim<T> confirmation:
                    this._confirmationRequests.Add(confirmation.RelayKey);
                    break;
                case RelayDataIndexCapacities indexCounts:
                    {
                        var counts =
                            typeof(T) == typeof(HuntRelay) ? indexCounts.Hunts :
                            typeof(T) == typeof(FateRelay) ? indexCounts.Fates :
                            null;
                        if (counts is null) break;
                        this.Data.SetCapacitiesCache(counts);
                    }
                    break;
            }
        }

        private void Client_Tick(SonarClient source)
        {
            if (!this._relayUpdateQueue.IsEmpty)
            {
                if (this.Client.Connection.IsConnected && this.Config.Contribute && this.Client.Modifiers.AllowContribute!.Value)
                {
                    var relaysDict = new Dictionary<string, T>();
                    while (this._relayUpdateQueue.TryDequeue(out var item)) relaysDict[item.RelayKey] = item;
                    var relays = new MessageList(relaysDict.Values);
                    this.Client.Connection.SendIfConnected(relays);
                }
                else this._relayUpdateQueue.Clear();
            }
        }

        private bool IsWithinJurisdiction(IReadOnlyDictionary<uint, WorldRow>? worlds, T relay, bool ignoreContrib = false)
        {
            var jurisdiction = this.Config.GetReportJurisdiction(relay.Id, ignoreContrib);
            return this.Client.PlayerPlace.IsWithinJurisdiction(relay, jurisdiction, worlds);
        }

        private bool IsWithinJurisdiction(IReadOnlyDictionary<uint, WorldRow>? worlds, RelayState<T> state, bool ignoreContrib = false) => this.IsWithinJurisdiction(worlds, state.Relay, ignoreContrib);

        protected bool IsWithinJurisdiction(IDictionary<uint, SonarJurisdiction> cache, IReadOnlyDictionary<uint, WorldRow>? worlds, T relay, bool ignoreContrib = false)
        {
            if (!cache.TryGetValue(relay.Id, out var jurisdiction))
            {
                cache[relay.Id] = jurisdiction = this.Config.GetReportJurisdiction(relay.Id, ignoreContrib);
            }
            return this.Client.PlayerPlace.IsWithinJurisdiction(relay, jurisdiction, worlds);
        }

        public bool IsTrackable(T relay)
        {
            return this.Config.TrackAll || this.IsWithinJurisdiction(null, relay, true);
        }
        public bool IsTrackable(RelayState<T> state) => this.IsTrackable(state.Relay);

        /// <inheritdoc/>
        public bool FeedRelay(T relay)
        {
            // Avoid invalid relays
            if (!relay.IsValid()) return false;
            var localOnly = relay.GetZone()!.LocalOnly;

            // Confirm relay existence if requested
            if (this._confirmationRequests.EstimatedCount > 0 && this._confirmationRequests.Remove(relay.RelayKey))
            {
                this.Client.Connection.SendIfConnected(new RelayConfirmationSlim<T>() { RelayKey = relay.RelayKey });
            }

            // Local only relay, disconnected or not contributing?
            if (localOnly || !this.Client.Connection.IsConnected || !this.Config.Contribute || !this.Client.Modifiers.AllowContribute!.Value)
            {
                if (this.IsTrackable(relay) && !this.FeedRelayInternal(relay)) return false;
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
        internal bool FeedRelayInternal(T newRelay) => this.UpdateState(newRelay, null, false);

        [SuppressMessage("Code Smell", "S3241", Justification = "Internal API consistency")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool UpdateState(RelayState<T> newState, bool isFromServer) => this.UpdateState(null, newState, isFromServer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool UpdateState(T? newRelay, RelayState<T>? newState, bool isFromServer)
        {
            // This method is intended to be called only with a newRelay or newState
            // Debug.Assert(newRelay is not null ^ newState is not null); // This should not happen

            if (newState is not null) newRelay = newState.Relay;
            // Debug.Assert(newRelay is not null); // newRelay will never be null from this point forward

            if (isFromServer)
            {
                if (!this.Config.Contribute) this.Client.Connection.SendIfConnected(this.Client.Configuration);
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
                if (this.Data.States.TryGetValue(newRelay!.RelayKey, out state!)) break;
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions (Justification: Immediately used after the loop)
                if (this.Data.TryAddState(state = newState ?? new(newRelay, now)))
#pragma warning restore S1121 // Assignments should not be made from within sub-expressions
                {
                    isNew = true;
                    break;
                }
                // Forward-progressing: No Spinwait needed
            } while (true);
            // Debug.Assert(state is not null); // This should not happen

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
            this.DispatchEvents(state, isNew);
            return true;
        }

        [SuppressMessage("Minor Code Smell", "S1199", Justification = "Attempt at organization")]
        [Obsolete("Will be removed, use index instead and filter with .Where as needed")]
        public IEnumerable<KeyValuePair<string, RelayState<T>>> GetRelaysQuery(bool withinJurisdiction = true, uint? regionId = null, uint? dcId = null, uint? worldId = null, uint? zoneId = null, uint? instanceId = null, uint? id = null, IReadOnlyCollection<uint>? regionIds = null, IReadOnlyCollection<uint>? dcIds = null, IReadOnlyCollection<uint>? worldIds = null, IReadOnlyCollection<uint>? zoneIds = null, IReadOnlyCollection<uint>? instanceIds = null, IReadOnlyCollection<uint>? ids = null, double? lastUpdatedAgo = null)
        {
            var cache = new Dictionary<uint, SonarJurisdiction>();
            var query = this.Data.States.AsEnumerable();
            var worlds = Database.Worlds;

            // Single entries into HashSets? Blame ASP.NET Core
            {
                if (regionIds?.Count == 1 && !regionId.HasValue) { regionId = regionIds.First(); regionIds = null; }
                if (dcIds?.Count == 1 && !dcId.HasValue) { dcId = dcIds.First(); dcIds = null; }
                if (worldIds?.Count == 1 && !worldId.HasValue) { worldId = worldIds.First(); worldIds = null; }
                if (zoneIds?.Count == 1 && !zoneId.HasValue) { zoneId = zoneIds.First(); zoneIds = null; }
                if (instanceIds?.Count == 1 && !instanceId.HasValue) { instanceId = instanceIds.First(); instanceIds = null; }
                if (ids?.Count == 1 && !id.HasValue) { id = ids.First(); ids = null; }
            }

            // Light
            {
                if (lastUpdatedAgo.HasValue) query = query.Where(kv => kv.Value.LastSeenAgo <= lastUpdatedAgo.Value);

                if (id.HasValue) query = query.Where(kv => kv.Value.Relay.Id == id.Value);
                if (instanceId.HasValue) query = query.Where(kv => kv.Value.Relay.InstanceId == instanceId.Value);
                if (zoneId.HasValue) query = query.Where(kv => kv.Value.Relay.ZoneId == zoneId.Value);
                if (worldId.HasValue) query = query.Where(kv => kv.Value.Relay.WorldId == worldId.Value);

                if (ids?.Count > 0) query = query.Where(kv => ids.Contains(kv.Value.Relay.Id));
                if (instanceIds?.Count > 0) query = query.Where(kv => instanceIds.Contains(kv.Value.Relay.InstanceId));
                if (zoneIds?.Count > 0) query = query.Where(kv => zoneIds.Contains(kv.Value.Relay.ZoneId));
                if (worldIds?.Count > 0) query = query.Where(kv => worldIds.Contains(kv.Value.Relay.WorldId));
            }

            // Heavy
            {
                if (dcId.HasValue) query = query.Where(kv => kv.Value.Relay.GetDatacenter()?.Id == dcId.Value);
                if (regionId.HasValue) query = query.Where(kv => kv.Value.Relay.GetDatacenter()?.RegionId == regionId.Value);
            }

            // Complicated and inefficient
            {
                if (dcIds?.Count > 0) query = query.Where(kv =>
                {
                    var dc = kv.Value.GetDatacenter();
                    if (dc is null) return false;
                    return dcIds.Contains(dc.Id);
                });

                if (regionIds?.Count > 0) query = query.Where(kv =>
                {
                    var dc = kv.Value.GetDatacenter();
                    if (dc is null) return false;
                    return regionIds.Contains(dc.RegionId);
                });

            }

            // Extreme
            {
                if (withinJurisdiction) query = query.Where(kv => this.IsWithinJurisdiction(cache, worlds, kv.Value.Relay, true));
            }

            return query;
        }

        [Obsolete("Will be removed, use index instead and filter with .Where as needed")]
        public IEnumerable<KeyValuePair<string, RelayState<T>>> GetRelays(bool withinJurisdiction = true, uint? regionId = null, uint? dcId = null, uint? worldId = null, uint? zoneId = null, uint? instanceId = null, uint? id = null, IReadOnlyCollection<uint>? regionIds = null, IReadOnlyCollection<uint>? dcIds = null, IReadOnlyCollection<uint>? worldIds = null, IReadOnlyCollection<uint>? zoneIds = null, IReadOnlyCollection<uint>? instanceIds = null, IReadOnlyCollection<uint>? ids = null, double? lastUpdatedAgo = null)
        {
            return this.GetRelaysQuery(withinJurisdiction, regionId, dcId, worldId, zoneId, instanceId, id, regionIds, dcIds, worldIds, zoneIds, instanceIds, ids, lastUpdatedAgo);
        }

        [Obsolete("Will be removed, use index instead and filter with .Where as needed")]
        public IEnumerable<KeyValuePair<string, RelayState<T>>> GetRelays(int count = 100, bool withinJurisdiction = true,  uint? regionId = null, uint? dcId = null, uint? worldId = null, uint? zoneId = null, uint? instanceId = null, uint? id = null, IReadOnlyCollection<uint>? regionIds = null, IReadOnlyCollection<uint>? dcIds = null, IReadOnlyCollection<uint>? worldIds = null, IReadOnlyCollection<uint>? zoneIds = null, IReadOnlyCollection<uint>? instanceIds = null, IReadOnlyCollection<uint>? ids = null, double? lastUpdatedAgo = null)
        {
            // Not going to reinvent Math.Clamp
            Math.Clamp(count, 0, int.MaxValue);
            if (count == 0) return Enumerable.Empty<KeyValuePair<string, RelayState<T>>>();

            return this.GetRelaysQuery(withinJurisdiction, regionId, dcId, worldId, zoneId, instanceId, id, regionIds, dcIds, worldIds, zoneIds, instanceIds, ids, lastUpdatedAgo)
                .OrderByDescending(s => s.Value.LastSeen)
                .Take(count);
        }

        #region Event Handlers
        private void DispatchEvents(RelayState<T> state, bool isNew)
        {
            if (this.Found is null && this.Updated is null && this.Dead is null && this.All is null) return;
            if (!this.Client.Modifiers.EnableRelayEventHandlers!.Value) return;
            if (this.AlwaysDispatchEvents || this.IsWithinJurisdiction(null, state, true))
            {
                var exceptions = Enumerable.Empty<Exception>();
                if (state.IsAliveInternal())
                {
                    if (isNew) this.Found?.SafeInvoke(state, out exceptions);
                    else this.Updated?.SafeInvoke(state, out exceptions);
                }
                else
                {
                    this.Dead?.SafeInvoke(state, out exceptions);
                }
                if (exceptions.Any() && this.Client.LogErrorEnabled) foreach (var exception in exceptions) this.Client.LogError($"{exception}");
                this.All?.SafeInvoke(state, out exceptions);
                if (exceptions.Any() && this.Client.LogErrorEnabled) foreach (var exception in exceptions) this.Client.LogError($"{exception}");
            }
        }

        /// <inheritdoc/>
        public event Action<RelayState<T>>? Found;

        /// <inheritdoc/>
        public event Action<RelayState<T>>? Updated;

        /// <inheritdoc/>
        public event Action<RelayState<T>>? Dead;

        /// <inheritdoc/>
        public event Action<RelayState<T>>? All;
        #endregion

        #region IDisposable Pattern
        private int disposed; //Interlocked
        public bool IsDisposed => this.disposed == 1;
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1) return;
            if (disposing)
            {
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