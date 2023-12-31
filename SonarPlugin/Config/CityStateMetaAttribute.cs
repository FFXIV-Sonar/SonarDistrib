using Dalamud.Utility;
using Sonar.Data;
using Sonar.Data.Rows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Config
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CityStateMetaAttribute : Attribute
    {
        public uint AetheryteId { get; init; }
        public uint ZoneId { get; init; }

        public CityStateMetaAttribute(uint aetheryteId, uint zoneId)
        {
            this.AetheryteId = aetheryteId;
            this.ZoneId = zoneId;
        }

        public AetheryteRow? GetAetheryte() => Database.Aetherytes.GetValueOrDefault(this.AetheryteId);
        public ZoneRow? GetZone() => Database.Zones.GetValueOrDefault(this.ZoneId);
    }

    public static class CityStateExtensions
    {
        public static CityStateMetaAttribute? GetMeta(this CityState cityState) => cityState.GetAttribute<CityStateMetaAttribute>();
    }
}
