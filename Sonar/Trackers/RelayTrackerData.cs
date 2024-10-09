using NonBlocking;
using Sonar.Data;
using Sonar.Extensions;
using Sonar.Indexes;
using Sonar.Relays;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using SonarUtils;
using SonarUtils.Collections;
using DryIoc;
using SonarUtils.Text;
using System.Collections.Frozen;
using AG.Collections.Concurrent;

namespace Sonar.Trackers
{
    public sealed partial class RelayTrackerData<T> : IRelayTrackerData<T> where T : Relay
    {
        private readonly ConcurrentDictionarySlim<string, RelayState<T>> _states = new(comparer: FarmHashStringComparer.Instance);
        private readonly ConcurrentDictionarySlim<string, ConcurrentTrieSet<RelayState<T>>> _index = new(comparer: FarmHashStringComparer.Instance);
        private readonly IReadOnlyDictionary<string, IReadOnlyCollection<RelayState<T>>> _indexTransform;

        private readonly IReadOnlyDictionary<string, RelayState> _statesGenericTransform;
        private readonly IReadOnlyDictionary<string, IReadOnlyCollection<RelayState>> _indexGenericTransform;

        public IReadOnlyDictionary<string, RelayState<T>> States => this._states;
        IReadOnlyDictionary<string, RelayState> IRelayTrackerData.States => this._statesGenericTransform;
        public IReadOnlyDictionary<string, IReadOnlyCollection<RelayState<T>>> Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                this.ThrowIfNotIndexing();
                return this._indexTransform;
            }
        }
        IReadOnlyDictionary<string, IReadOnlyCollection<RelayState>> IRelayTrackerData.Index => this._indexGenericTransform;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfNotIndexing()
        {
            if (!this.Indexing) ThrowNotIndexing();
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowNotIndexing() => throw new InvalidOperationException($"Indexing is disabled on this {nameof(RelayTrackerData<T>)}");

        public bool Indexing { get; set; }

        internal RelayTrackerData(bool indexing = true)
        {
            this.Indexing = indexing;
            this._indexTransform = TransformReadOnlyDictionary.Create(this._index, static entries => (IReadOnlyCollection<RelayState<T>>)entries);

            this._statesGenericTransform = TransformReadOnlyDictionary.Create(this._states, static state => (RelayState)state);
            this._indexGenericTransform = TransformReadOnlyDictionary.Create(this._index, static entries => (IReadOnlyCollection<RelayState>)entries);
        }

        internal RelayTrackerData(IEnumerable<RelayState<T>> states, bool indexing = true) : this(indexing)
        {
            this.TryAddStates(states);
        }

        public bool TryAddState(RelayState<T> state)
        {
            if (this._states.TryAdd(state.RelayKey, state))
            {
                if (this.Indexing) this.AddIndexEntries(state);
                this.DispatchEvent(this._addedHandlers, state);
                return true;
            }
            return false;
        }

        public bool TryAddState(RelayState state)
        {
            return state is RelayState<T> tState && this.TryAddState(tState);
        }

        public void TryAddStates(IEnumerable<RelayState<T>> states)
        {
            foreach (var state in states) this.TryAddState(state);
        }

        public void TryAddStates(IEnumerable<RelayState> states)
        {
            foreach (var state in states) this.TryAddState(state);
        }

        public int Count => this._states.Count;

        public int IndexCount => this._index.Count;

        /// <summary>Remove a state through its key</summary>
        public bool Remove(string key)
        {
            if (!this._states.Remove(key, out var state)) return false;
            if (this.Indexing) this.RemoveIndexEntries(state);
            this.DispatchEvent(this._removedHandlers, state);
            return true;
        }

        /// <summary>Remove a state</summary>
        public bool Remove(RelayState<T> state) => this.Remove(state.RelayKey);

        /// <summary>Remove a state</summary>
        public bool Remove(RelayState state) => state is RelayState<T> && this.Remove(state.RelayKey);

        /// <summary>Remove a state associated with a specific relay</summary>
        public bool Remove(T relay) => this.Remove(relay.RelayKey);

        /// <summary>Remove a state associated with a specific relay</summary>
        public bool Remove(Relay relay) => relay is T && this.Remove(relay.RelayKey);

        /// <summary>Get states from an index</summary>
        /// <param name="indexKey"><see cref="IndexType"/></param>
        /// <remarks>
        /// <para>If index doesn't exist, an empty collection is returned</para>
        /// <para>They're technically <see cref="IReadOnlySet{T}"/> except for <c>"all"</c>, please don't cast and assume its safe.</para>
        /// </remarks>
        public IReadOnlyCollection<RelayState<T>> GetIndexStates(string indexKey)
        {
            this.ThrowIfNotIndexing();
            if (indexKey == "all") return this.States.GetNonSnapshottingValues(); // NOTE: This is the cause why this method cannot return an IReadOnlySet
            return this.Index.GetValueOrDefault(indexKey) ?? [];
        }

        /// <summary>Get states from an index</summary>
        /// <param name="indexKey"><see cref="IndexType"/></param>
        /// <remarks>
        /// <para>If index doesn't exist, an empty collection is returned</para>
        /// <para>They're technically <see cref="IReadOnlySet{T}"/> except for <c>"all"</c>, please don't cast and assume its safe.</para>
        /// </remarks>
        IReadOnlyCollection<RelayState> IRelayTrackerData.GetIndexStates(string indexKey)
        {
            return this.GetIndexStates(indexKey);
        }

        /// <summary>Clear all data</summary>
        public void Clear()
        {
            this._states.Clear();
            this._index.Clear();
            this.DispatchCleredEvent();
        }

        [SuppressMessage("Major Code Smell", "S1121", Justification = "Used immediately")]
        private void AddIndexEntries(RelayState<T> state)
        {
            if (!this.Indexing) return;
            foreach (var indexKey in state.IndexKeysCore.Items.AsSpan())
            {
                ConcurrentTrieSet<RelayState<T>> entries;
                while (true)
                {
                    if (this._index.TryGetValue(indexKey, out entries!)) break;
                    if (this._index.TryAdd(indexKey, entries = [])) break;
                }
                entries.Add(state);
            }
        }

        private void RemoveIndexEntries(RelayState<T> state)
        {
            if (!this.Indexing) return;
            foreach (var indexKey in state.IndexKeysCore.Items.AsSpan())
            {
                if (!this._index.TryGetValue(indexKey, out var entries)) continue;
                entries.Remove(state);
            }
        }
    }
}
