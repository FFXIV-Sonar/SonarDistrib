using MessagePack;
using Sonar.Data;
using Sonar.Indexes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace Sonar.Models
{
    public sealed partial class PlayerInfo : ITrackerIndexable
    {
        private static readonly NonBlocking.NonBlockingDictionary<uint, string[]> s_indexKeysCache = new();

        private IEnumerable<string>? _indexKeys;

        /// <summary>
        /// Index Keys
        /// </summary>
        [IgnoreMember]
        [JsonIgnore]
        public IEnumerable<string> IndexKeys => this._indexKeys ??= this.GetIndexKeysCore();

        [SuppressMessage("Stinky message", "S1121", Justification = "Immediately used")]
        private IEnumerable<string> GetIndexKeysCore()
        {
            while (true) // Expected max number of iterations: 2
            {
                if (s_indexKeysCache.TryGetValue(this.HomeWorldId, out var result)) return result;
                if (s_indexKeysCache.TryAdd(this.HomeWorldId, result = this.GetIndexKeysCore_Factory())) return result;
            }
        }

        private string[] GetIndexKeysCore_Factory()
        {
            var world = Database.Worlds.GetValueOrDefault(this.HomeWorldId);
            var info = new IndexInfo()
            {
                WorldId = this.HomeWorldId,
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
            var world = Database.Worlds.GetValueOrDefault(this.HomeWorldId);
            var info = new IndexInfo()
            {
                WorldId = this.HomeWorldId,
                DatacenterId = world?.DatacenterId,
                RegionId = world?.RegionId,
                AudienceId = world?.AudienceId,
            };
            return info.GetIndexKey(type);
        }

        internal static void ResetIndexCache() => s_indexKeysCache.Clear();
    }
}
