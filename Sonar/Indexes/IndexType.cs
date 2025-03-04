namespace Sonar.Indexes
{
    /// <summary>Index Types</summary>
    public enum IndexType
    {
        /// <summary>None index key in the form of <c>"none"</c></summary>
        None,

        /// <summary>World index key in the form of <c>"{worldId}"</c></summary>
        /// <example><c>"62"</c></example>
        World,

        /// <summary>WorldZone index key in the form of <c>"{worldId}_{zoneId}"</c></summary>
        /// <example><c>"62_818"</c></example>
        WorldZone,

        /// <summary>WorldZoneInstance index key in the form of <c>"{worldId}_{zoneId}_{instanceId}"</c></summary>
        /// <example><c>"62_818_0"</c></example>
        WorldZoneInstance,

        /// <summary>WorldInstance index key in the form of <c>"wi{worldId}_{instanceId}"</c></summary>
        /// <example><c>"wi62_0"</c></example>
        WorldInstance,

        /// <summary>Zone index key in the form of <c>"z{zoneId}"</c></summary>
        /// <example><c>"z818"</c></example>
        Zone,

        /// <summary>ZoneInstance index key in the form of <c>"z{zoneId}_{instanceId}"</c></summary>
        /// <example><c>"z818_0"</c></example>
        ZoneInstance,

        /// <summary>Instance index key in the form of <c>"i{instanceId}"</c></summary>
        /// <example>(ex: <c>"i0"</c>)</example>
        Instance,

        /// <summary>Datacenter index key in the form of <c>"d{datacenterId}"</c></summary>
        /// <example><c>"d8"</c></example>
        Datacenter,

        /// <summary>DatacenterZone index key in the form of <c>"d{datacenterId}_{zoneId}"</c></summary>
        /// <example><c>"d8_818"</c></example>
        DatacenterZone,

        /// <summary>DatacenterZoneInstance index key in the form of <c>"d{datacenterId}_{zoneId}_{instanceId}"</c></summary>
        /// <example><c>"d8_818_0"</c></example>
        DatacenterZoneInstance,

        /// <summary>DatacenterInstance index key in the form of <c>"di{datacenterId}_{instanceId}"</c></summary>
        /// <example><c>"d8_0"</c></example>
        DatacenterInstance,

        /// <summary>Region index key in the form of <c>"r{regionId}"</c></summary>
        /// <example><c>"r2"</c></example>
        Region,

        /// <summary>RegionZone index key in the form of <c>"r{regionId}_{zoneId}"</c></summary>
        /// <example><c>"r2_818"</c></example>
        RegionZone,

        /// <summary>RegionZoneInstance index key in the form of <c>"r{regionId}_{zoneId}_{instanceId}"</c></summary>
        /// <example><c>"r2_818_0"</c></example>
        RegionZoneInstance,

        /// <summary>RegionInstance index key in the form of <c>"ri{regionId}_{instanceId}"</c></summary>
        /// <example><c>"ri2_0"</c></example>
        RegionInstance,

        /// <summary>Audience index key in the form of <c>"a{audienceId}"</c></summary>
        /// <example><c>"a1"</c></example>
        Audience,

        /// <summary>AudienceZone index key in the form of <c>"a{audienceId}_{zoneId}"</c></summary>
        /// <example><c>"a1_818"</c></example>
        AudienceZone,

        /// <summary>AudienceZoneInstance index key in the form of <c>"a{audienceId}_{zoneId}_{instanceId}"</c></summary>
        /// <example><c>"a1_818_0"</c></example>
        AudienceZoneInstance,

        /// <summary>AudienceInstance index key in the form of <c>"ai{audienceId}_{instanceId}"</c></summary>
        /// <example><c>"ai1_0"</c></example>
        AudienceInstance,

        /// <summary>All index key in the form of <c>"all"</c></summary>
        All,
    }
}
