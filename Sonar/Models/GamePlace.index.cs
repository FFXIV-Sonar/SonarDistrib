using MessagePack;
using Sonar.Data;
using Sonar.Indexes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace Sonar.Models
{
    public partial class GamePlace : ITrackerIndexable
    {
        internal static readonly ConcurrentDictionary<string, FrozenSet<string>> s_indexKeysCache = new();
        private FrozenSet<string>? _indexKeys;

        /// <summary>Index Keys</summary>
        [JsonIgnore]
        [IgnoreMember]
        public IEnumerable<string> IndexKeys => this.IndexKeysCore;
        internal FrozenSet<string> IndexKeysCore => this._indexKeys ??= this.GetIndexKeysCore();

        [SuppressMessage("Stinky message", "S1121", Justification = "Immediately used")]
        private FrozenSet<string> GetIndexKeysCore() => s_indexKeysCache.GetOrAdd(this.PlaceKey, static (key, self) => self.GetIndexKeysCore_Factory(), this);
        private FrozenSet<string> GetIndexKeysCore_Factory()
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
            return info.GetIndexKeys().ToFrozenSet();
        }

        /// <summary>Get an index key of a specific <see cref="IndexType"/></summary>
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
