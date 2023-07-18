using Sonar.Indexes;
using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Trackers
{
    public sealed partial class RelayTrackerData<T> where T : Relay
    {
        private static ParallelOptions GetParallelOptions(bool parallel = false)
        {
            var options = new ParallelOptions();
            if (!parallel) options.MaxDegreeOfParallelism = 1;
            return options;
        }

        /// <summary>Perform a consistency check of all states data</summary>
        /// <returns>Consistency results. Empty means its fully consistent.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<string> DebugIndexConsistencyCheck(bool parallel = false)
        {
            List<string> output = new();
            var typeName = typeof(T).Name;
            var options = GetParallelOptions(parallel);

            Parallel.ForEach(this._states, options, kvp =>
            {
                var (key, state) = kvp;
                // Make sure state's relay key is consistent with the key stored into
                if (key != state.RelayKey)
                {
                    lock (output) output.Add($"{typeName} State key is inconsistent: {key} != {state.RelayKey}");
                    if (Debugger.IsAttached) Debugger.Break();
                }

                // Make sure state can be found on all indexes it belongs into
                foreach (var indexKey in state.IndexKeys)
                {
                    if (!this.GetIndexEntries(indexKey).Contains(state))
                    {
                        lock (output) output.Add($"{typeName} State not found at index {indexKey}: {state.RelayKey} (Expected indexes: {string.Join(", ", state.IndexKeys)})");
                        if (Debugger.IsAttached) Debugger.Break();
                    }
                }
            });

            Parallel.ForEach(this.Index, options, kvp =>
            {
                var (indexKey, index) = kvp;

                // Make sure the index key is valid
                if (!IndexInfo.TryParse(indexKey, out _))
                {
                    lock (output) output.Add($"Index key not valid: {indexKey}");
                    if (Debugger.IsAttached) Debugger.Break();
                }

                foreach (var state in index)
                {
                    var key = state.RelayKey;
                    var state2 = this._states.GetValueOrDefault(key);

                    // Make sure state exist
                    if (state2 is null)
                    {
                        lock (output) output.Add($"{typeName} State at index {indexKey} not found: {state.RelayKey}");
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    // Make sure its the correct state
                    else if (state != state2)
                    {
                        lock (output) output.Add($"{typeName} State at index {indexKey} not found: {state.RelayKey}, found {state2.RelayKey} instead");
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    // Make sure it does belong to the index
                    if (!state.IndexKeys.Contains(indexKey))
                    {
                        lock (output) output.Add($"{typeName} does not belong to index {indexKey}: {state.RelayKey}");
                        if (Debugger.IsAttached) Debugger.Break();
                    }
                }
            });

            return output;
        }

        /// <summary>Rebuild indexes</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DebugRebuildIndex(bool parallel = false)
        {
            this._index.Clear(this._index.Capacity);
            Parallel.ForEach(this._states.Values, GetParallelOptions(parallel), this.AddIndexEntries);
        }

        /// <summary>Cleanup empty index entries</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DebugCleanupIndex()
        {
            foreach (var (indexKey, index) in this.Index)
            {
                if (index.Count == 0)
                {
                    this._index.TryRemove(indexKey, out _);
                }
            }
        }

        /// <summary>Rebuild states from indexes (the reverse of <see cref="DebugRebuildIndex"/></summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DebugRebuildStatesFromIndex()
        {
            // This is mostly a benchmark and should result in the same set of states (untested)
            var addedCache = new HashSet<RelayState<T>>();

            this._states.Clear();
            foreach (var index in this.Index.Values)
            {
                foreach (var state in index.Where(addedCache.Add))
                {
                    this._states[state.RelayKey] = state;
                }
            }
        }
    }
}
