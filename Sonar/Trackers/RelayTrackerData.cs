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

namespace Sonar.Trackers
{
    public sealed partial class RelayTrackerData<T> where T : Relay
    {
        private readonly ConcurrentDictionarySlim<string, RelayState<T>> _states = new(comparer: FarmHashStringComparer.Instance);
        private readonly ConcurrentDictionarySlim<string, ConcurrentHashSetSlim<RelayState<T>>> _index = new(comparer: FarmHashStringComparer.Instance);
        private readonly IReadOnlyDictionary<string, IReadOnlySet<RelayState<T>>> _indexTransform;

        public IReadOnlyDictionary<string, RelayState<T>> States => this._states;
        public IReadOnlyDictionary<string, IReadOnlySet<RelayState<T>>> Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                this.ThrowIfNotIndexing();
                return this._indexTransform;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfNotIndexing()
        {
            if (!this.Indexing) ThrowNotIndexing();
        }

        [DoesNotReturn]
        private static void ThrowNotIndexing() => throw new InvalidOperationException($"Indexing is disabled on this {nameof(RelayTrackerData<T>)}");

        public bool Indexing { get; set; }

        internal RelayTrackerData(bool indexing = true)
        {
            this.Indexing = indexing;
            this._indexTransform = TransformReadOnlyDictionary.Create(this._index, static entries => (IReadOnlySet<RelayState<T>>)entries);
        }

        internal RelayTrackerData(IEnumerable<RelayState<T>> states, bool indexing = true) : this(indexing)
        {
            this.TryAddStates(states);
        }

        internal bool TryAddState(RelayState<T> state)
        {
            if (this._states.TryAdd(state.RelayKey, state))
            {
                if (this.Indexing) this.AddIndexEntries(state);
                this.Added?.SafeInvoke(this, state);
                return true;
            }
            return false;
        }

        internal void TryAddStates(IEnumerable<RelayState<T>> states)
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
            this.Removed?.SafeInvoke(this, state);
            return true;
        }

        /// <summary>Remove a state</summary>
        public bool Remove(RelayState<T> state) => this.Remove(state.RelayKey);

        /// <summary>Remove a state associated with a specific relay</summary>
        public bool Remove(T relay) => this.Remove(relay.RelayKey);

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
            return this.Index.GetValueOrDefault(indexKey) ?? ImmutableHashSet<RelayState<T>>.Empty;
        }

        /// <summary>Clear all data</summary>
        public void Clear()
        {
            this._states.Clear();
            this._index.Clear();
            this.Cleared?.SafeInvoke(this);
        }

        public event Action<RelayTrackerData<T>, RelayState<T>>? Added;
        public event Action<RelayTrackerData<T>, RelayState<T>>? Removed;
        public event Action<RelayTrackerData<T>>? Cleared;

        [SuppressMessage("Major Code Smell", "S1121", Justification = "Used immediately")]
        private void AddIndexEntries(RelayState<T> state)
        {
            if (!this.Indexing) return;
            foreach (var indexKey in Unsafe.As<string[]>(state.IndexKeys))
            {
                ConcurrentHashSetSlim<RelayState<T>> entries;
                while (true)
                {
                    if (this._index.TryGetValue(indexKey, out entries!)) break;
                    if (this._index.TryAdd(indexKey, entries = new())) break;
                }
                entries.Add(state);
            }
        }

        private void RemoveIndexEntries(RelayState<T> state)
        {
            if (!this.Indexing) return;
            foreach (var indexKey in state.IndexKeys)
            {
                if (!this._index.TryGetValue(indexKey, out var entries)) continue;
                entries.Remove(state);
            }
        }
    }
}
