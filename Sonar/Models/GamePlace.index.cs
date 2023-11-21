using MessagePack;
using Newtonsoft.Json;
using Sonar.Data;
using Sonar.Indexes;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using SonarUtils.Collections;
using SonarUtils.Text;

namespace Sonar.Models
{
    public partial class GamePlace : ITrackerIndexable
    {
        private static readonly ConcurrentDictionarySlim<string, string[]> s_indexKeysCache = new(comparer: FarmHashStringComparer.Instance);
        private IEnumerable<string>? _indexKeys;

        /// <summary>
        /// Index Keys
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public IEnumerable<string> IndexKeys => this._indexKeys ??= this.GetIndexKeysCore();

        [SuppressMessage("Stinky message", "S1121", Justification = "Immediately used")]
        private IEnumerable<string> GetIndexKeysCore()
        {
            while (true) // Expected max number of iterations: 2
            {
                if (s_indexKeysCache.TryGetValue(this.PlaceKey, out var result)) return result;
                if (s_indexKeysCache.TryAdd(this.PlaceKey, result = this.GetIndexKeysCore_Factory())) return result;
            }
        }

        private string[] GetIndexKeysCore_Factory()
        {
            var world = Database.Worlds.GetValueOrDefault(this.WorldId);
            var info = new IndexInfo()
            {
                WorldId = this.WorldId,
                ZoneId = this.ZoneId,
                InstanceId = this.InstanceId,

                DatacenterId = world?.DatacenterId,
                RegionId = world?.RegionId,
                AudienceId = world?.AudienceId,
            };
            return info.GetIndexKeys().ToArray();
        }

        /// <summary>
        /// Get an index key of a specific <see cref="IndexType"/>
        /// </summary>
        public string GetIndexKey(IndexType type)
        {
            var world = Database.Worlds.GetValueOrDefault(this.WorldId);
            var info = new IndexInfo()
            {
                WorldId = this.WorldId,
                ZoneId = this.ZoneId,
                InstanceId = this.InstanceId,

                DatacenterId = world?.DatacenterId,
                RegionId = world?.RegionId,
                AudienceId = world?.AudienceId,
            };
            return info.GetIndexKey(type);
        }

        internal static void ResetIndexCache() => s_indexKeysCache.Clear();
    }
}
