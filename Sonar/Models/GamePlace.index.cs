using MessagePack;
using Newtonsoft.Json;
using Sonar.Data;
using Sonar.Indexes;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sonar.Models
{
    public partial class GamePlace : ITrackerIndexable
    {
        private static readonly NonBlocking.ConcurrentDictionary<string, WeakReference<IEnumerable<string>>> s_indexKeysCache = new(comparer: FarmHashStringComparer.Instance);
        private IEnumerable<string>? _indexKeys;

        /// <summary>
        /// Index Keys
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public IEnumerable<string> IndexKeys => this._indexKeys ??= this.GetIndexKeysCore();

        private IEnumerable<string> GetIndexKeysCore()
        {
            while (true) // Expected max number of iterations: 2
            {
                IEnumerable<string>? result;
                if (!s_indexKeysCache.TryGetValue(this.PlaceKey, out var weakRef))
                {
                    result = this.GetIndexKeysCore_Factory();
                    s_indexKeysCache.TryAdd(this.PlaceKey, new(result));
                }
                else
                {
                    weakRef.TryGetTarget(out result); // Result will never be null if not garbage collected
                }
                if (result is not null) return result;
                s_indexKeysCache.TryRemove(KeyValuePair.Create(this.PlaceKey, weakRef)); // Previously generated index keys garbage collected

                // Possible race condition may result in duplicate identical arrays being generated.
                // This is considered a non-issue as the strings contained are the same anyway, they just got twice the references now.
                // (This race condition, while existing, should never happen)
            }
        }

        private IEnumerable<string> GetIndexKeysCore_Factory()
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
    }
}
