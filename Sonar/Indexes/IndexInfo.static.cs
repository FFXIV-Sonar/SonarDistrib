using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sonar.Indexes
{
    public sealed partial class IndexInfo
    {
        private static IReadOnlyDictionary<IndexType, Regex>? s_regexComparers;
        public static IReadOnlyDictionary<IndexType, Regex> GetRegexComparers()
        {
            // I'm assuming entries are enumerated in insertion order. If this changes well...
            return s_regexComparers ??= new Dictionary<IndexType, Regex>()
            {
                { IndexType.None, IndexUtils.NoneRegex },
                { IndexType.All, IndexUtils.AllRegex },

                { IndexType.World, IndexUtils.WorldRegex },
                { IndexType.WorldZone, IndexUtils.WorldZoneRegex },
                { IndexType.WorldZoneInstance, IndexUtils.WorldZoneInstanceRegex },
                { IndexType.WorldInstance, IndexUtils.WorldInstanceRegex },
                { IndexType.Zone, IndexUtils.ZoneRegex },
                { IndexType.ZoneInstance, IndexUtils.ZoneInstanceRegex },
                { IndexType.Instance, IndexUtils.InstanceRegex },

                { IndexType.Datacenter, IndexUtils.DatacenterRegex },
                { IndexType.DatacenterZone, IndexUtils.DatacenterZoneRegex },
                { IndexType.DatacenterZoneInstance, IndexUtils.DatacenterZoneInstanceRegex },
                { IndexType.DatacenterInstance, IndexUtils.DatacenterInstanceRegex },

                { IndexType.Region, IndexUtils.RegionRegex },
                { IndexType.RegionZone, IndexUtils.RegionZoneRegex },
                { IndexType.RegionZoneInstance, IndexUtils.RegionZoneInstanceRegex },
                { IndexType.RegionInstance, IndexUtils.RegionInstanceRegex },

                { IndexType.Audience, IndexUtils.AudienceRegex },
                { IndexType.AudienceZone, IndexUtils.AudienceZoneRegex },
                { IndexType.AudienceZoneInstance, IndexUtils.AudienceZoneInstanceRegex },
                { IndexType.AudienceInstance, IndexUtils.AudienceInstanceRegex },
            };
        }
    }
}
