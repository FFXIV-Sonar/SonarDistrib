using Sonar.Enums;

namespace Sonar.Indexes
{
    public static class IndexExtensions
    {
        /// <summary>Gets the index <see cref="SonarJurisdiction"/></summary>
        /// <param name="indexType">Index type</param>
        /// <param name="partial">Whether to return partial jurisdictions</param>
        /// <returns><see cref="SonarJurisdiction"/></returns>
        public static SonarJurisdiction GetIndexJurisdiction(this IndexType indexType, bool partial = true)
        {
            return indexType switch
            {
                IndexType.None => SonarJurisdiction.None,
                IndexType.WorldZoneInstance => SonarJurisdiction.Instance,
                IndexType.WorldZone => SonarJurisdiction.Zone,
                IndexType.World => SonarJurisdiction.World,
                IndexType.Datacenter => SonarJurisdiction.Datacenter,
                IndexType.Region => SonarJurisdiction.Region,
                IndexType.Audience => SonarJurisdiction.Audience,
                IndexType.All => SonarJurisdiction.All,

                IndexType.WorldInstance => partial ? SonarJurisdiction.World : SonarJurisdiction.Default,
                
                IndexType.Zone => partial ? SonarJurisdiction.All : SonarJurisdiction.Default,
                IndexType.ZoneInstance => partial ? SonarJurisdiction.All : SonarJurisdiction.Default,
                IndexType.Instance => partial ? SonarJurisdiction.All : SonarJurisdiction.Default,
                
                IndexType.DatacenterZone => partial ? SonarJurisdiction.Datacenter : SonarJurisdiction.Default,
                IndexType.DatacenterZoneInstance => partial ? SonarJurisdiction.Datacenter : SonarJurisdiction.Default,
                IndexType.DatacenterInstance => partial ? SonarJurisdiction.Datacenter : SonarJurisdiction.Default,
                
                IndexType.RegionZone => partial ? SonarJurisdiction.Region : SonarJurisdiction.Default,
                IndexType.RegionZoneInstance => partial ? SonarJurisdiction.Region : SonarJurisdiction.Default,
                IndexType.RegionInstance => partial ? SonarJurisdiction.Region : SonarJurisdiction.Default,
                
                IndexType.AudienceZone => partial ? SonarJurisdiction.Audience: SonarJurisdiction.Default,
                IndexType.AudienceZoneInstance => partial ? SonarJurisdiction.Audience : SonarJurisdiction.Default,
                IndexType.AudienceInstance => partial ? SonarJurisdiction.Audience : SonarJurisdiction.Default,

                _ => SonarJurisdiction.Default,
            };
        }
    }
}
