using Sonar.Indexes;
using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SonarUtils;

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
            this.ThrowIfNotIndexing();

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
                foreach (var indexKey in state.IndexKeysCore.Items.AsSpan())
                {
                    // Check that the index entries exist
                    if (!this._index.TryGetValue(indexKey, out var entries))
                    {
                        lock (output) output.Add($"{typeName} State index not found for {state.RelayKey}: {indexKey}");
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    // Check that the state is in the index
                    else if (!entries.TryGetValue(state, out var state2))
                    {
                        lock (output) output.Add($"{typeName} State not found at index {indexKey}: {state.RelayKey} (Expected indexes: {string.Join(", ", state.IndexKeys)})");
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    // Make sure its the same state
                    else if (!ReferenceEquals(state, state2))
                    {
                        lock (output) output.Add($"{typeName} State does not match between states and index entries contents: {state.RelayKey}");
                        if (Debugger.IsAttached) Debugger.Break();
                    }
                }
            });

            Parallel.ForEach(this._index, options, kvp =>
            {
                var (indexKey, entries) = kvp;

                // Make sure the index key is valid
                if (!IndexInfo.TryParse(indexKey, out _))
                {
                    lock (output) output.Add($"Index key not valid: {indexKey}");
                    if (Debugger.IsAttached) Debugger.Break();
                }

                foreach (var state in entries)
                {
                    var key = state.RelayKey;
                    
                    // Make sure state exist
                    if (!this._states.TryGetValue(key, out var state2))
                    {
                        lock (output) output.Add($"{typeName} State at index {indexKey} not found: {key}");
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    // Make sure its the same state
                    else if (!ReferenceEquals(state, state2))
                    {
                        lock (output) output.Add($"{typeName} State does not match between states and index entries contents: {state.RelayKey}");
                        if (Debugger.IsAttached) Debugger.Break();
                    }

                    // Make sure it does belong to the index
                    else if (!state.IndexKeysCore.Contains(indexKey))
                    {
                        lock (output) output.Add($"{typeName} state does not belong to index {indexKey}: {state.RelayKey} (Expected indexes: {string.Join(", ", state.IndexKeys)})");
                        if (Debugger.IsAttached) Debugger.Break();
                    }
                }
            });

            return output;
        }

        /// <summary>Rebuild indexes</summary>
        public void RebuildIndex(bool parallel = false)
        {
            this.ThrowIfNotIndexing();
            foreach (var set in this._index.Values) set.Clear();
            Parallel.ForEach(this._states.Values, GetParallelOptions(parallel), this.AddIndexEntries);
        }

        /// <summary>Cleanup empty index entries</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DebugCleanupIndex()
        {
            this.ThrowIfNotIndexing();
            foreach (var (indexKey, entries) in this.Index)
            {
                if (entries.Count == 0)
                {
                    this._index.Remove(indexKey, out _);
                }
            }
        }
    }
}
