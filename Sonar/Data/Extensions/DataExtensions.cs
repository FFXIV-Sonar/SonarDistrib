using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Models;
using System.Collections.Generic;
using System.Linq;
using Sonar.Numerics;
using Sonar.Trackers;
using static Sonar.Data.MapFlagUtils;
using Sonar.Relays;

namespace Sonar.Data.Extensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Intentional")]
    public static class SonarDataExtensions
    {
        #region Enumerables
        public static IEnumerable<WorldRow> GetWorlds(this DatacenterRow dc) => Database.Worlds.Values.Where(w => w.DatacenterId == dc.Id);
        public static IEnumerable<FateRow> GetFates(this ZoneRow z) => Database.Fates.Values.Where(f => f.ZoneId == z.Id);
        public static IEnumerable<FateRow> GetFates(this ExpansionPack e) => Database.Fates.Values.Where(f => f.Expansion == e);
        public static IEnumerable<ZoneRow> GetZones(this ExpansionPack e) => Database.Zones.Values.Where(z => z.Expansion == e);
        public static IEnumerable<HuntRow> GetHunts(this ExpansionPack e) => Database.Hunts.Values.Where(h => h.Expansion == e);
        #endregion

        #region GetWorld
        public static WorldRow? GetWorld(this GamePlace p) => Database.Worlds.GetValueOrDefault(p.WorldId);
        public static WorldRow? GetWorld(this PlayerInfo p) => Database.Worlds.GetValueOrDefault(p.HomeWorldId);
        public static WorldRow? GetWorld(this RelayState s) => s.Relay.GetWorld();
        public static WorldRow? GetWorld<T>(this RelayConfirmationBase<T> c) where T : Relay => Database.Worlds.GetValueOrDefault(c.WorldId);
        public static WorldRow? GetHomeWorld(this PlayerInfo i) => Database.Worlds.GetValueOrDefault(i.HomeWorldId); // duplicate...
        #endregion

        #region GetDatacenter
        public static DatacenterRow? GetDatacenter(this GamePlace p) => p.GetWorld()?.GetDatacenter();
        public static DatacenterRow? GetDatacenter(this WorldRow w) => Database.Datacenters.GetValueOrDefault(w.DatacenterId);
        public static DatacenterRow? GetDatacenter(this RelayState s) => s.Relay.GetDatacenter();
        public static DatacenterRow? GetDatacenter<T>(this RelayConfirmationBase<T> c) where T : Relay => c.GetWorld()?.GetDatacenter();
        #endregion

        #region Zone Extensions
        public static IEnumerable<ZoneRow> GetSpawnZones(this HuntRow h) => h.ZoneIds.Select(z => Database.Zones[z]);
        public static ZoneRow? GetZone(this FateRow f) => Database.Zones.GetValueOrDefault(f.ZoneId);
        public static ZoneRow? GetZone(this GamePlace p) => Database.Zones.GetValueOrDefault(p.ZoneId);
        public static ZoneRow? GetZone<T>(this RelayConfirmationBase<T> c) where T : Relay => Database.Zones.GetValueOrDefault(c.ZoneId);
        public static IEnumerable<uint> GetGroupZoneIds(this FateRow fate) => fate.GetGroupFates().Select(f => f.ZoneId).Distinct();
        public static IEnumerable<ZoneRow> GetGroupZones(this FateRow fate) => fate.GetGroupZoneIds().Select(i => Database.Zones[i]);
        #endregion

        #region GetExpansion
        public static ExpansionPack GetExpansion(this GamePlace p) => p.GetZone()?.Expansion ?? ExpansionPack.Unknown;
        public static ExpansionPack GetExpansion(this HuntRelay r) => r.GetHunt()?.Expansion ?? ExpansionPack.Unknown;
        public static ExpansionPack GetExpansion(this FateRelay r) => r.GetFate()?.Expansion ?? ExpansionPack.Unknown;
        #endregion

        #region GetFlag
        private static float GetScaleHelper(ZoneRow? zone) => zone?.Scale ?? 1;
        private static SonarVector3 GetOffsetHelper(ZoneRow? zone) => zone?.Offset ?? new SonarVector3(0, 0, 0);
        public static SonarVector3 GetFlag(this FateRow f) => RawToFlag(GetScaleHelper(f.GetZone()), GetOffsetHelper(f.GetZone()), f.Coordinates);
        public static SonarVector3 GetFlag(this GamePosition p) => RawToFlag(GetScaleHelper(p.GetZone()), GetOffsetHelper(p.GetZone()), p.Coords);
        #endregion

        #region GetFlagString
        /// <summary>
        /// Get a flag string for this vector (assumes vector is a flag)
        /// </summary>
        public static string ToFlagString(this SonarVector3 flag, MapFlagFormatFlags format) => MapFlagUtils.FlagToString(flag, format);

        public static string GetFlagString(this FateRow f, MapFlagFormatFlags format = MapFlagFormatFlags.SonarPreset) => f.GetFlag().ToFlagString(format);
        public static string GetFlagString(this GamePosition p, MapFlagFormatFlags format = MapFlagFormatFlags.SonarPreset) => p.GetFlag().ToFlagString(format);
        #endregion

        #region Hunt extensions
        public static HuntRow? GetHunt(this HuntRelay r) => Database.Hunts.GetValueOrDefault(r.Id);
        public static HuntRow? GetHunt(this RelayState<HuntRelay> s) => s.Relay.GetHunt();
        public static HuntRow? GetHunt(this RelayConfirmationBase<HuntRelay> c) => Database.Hunts.GetValueOrDefault(c.RelayId);
        public static HuntRank GetRank(this HuntRelay r) => r.GetHunt()?.Rank ?? HuntRank.None;
        public static HuntRank GetRank(this RelayState<HuntRelay> s) => s.Relay.GetRank();
        public static HuntRank GetRank(this RelayConfirmationBase<HuntRelay> c) => c.GetHunt()?.Rank ?? HuntRank.None;
        public static IEnumerable<HuntRow> GetZoneHunts(this ZoneRow z) => z.HuntIds.Select(i => Database.Hunts[i]);
        #endregion

        #region Fate extensions
        public static FateRow? GetFate(this FateRelay r) => Database.Fates.GetValueOrDefault(r.Id);
        public static FateRow? GetFate(this RelayState<FateRelay> s) => s.Relay.GetFate();
        public static FateRow? GetFate(this RelayConfirmationBase<FateRelay> c) => Database.Fates.GetValueOrDefault(c.RelayId);
        public static IEnumerable<FateRow> GetGroupFates(this FateRow f) => f.GroupFateIds.Select(i => Database.Fates[i]);

        public static bool IsFateInField(this FateRow f) => f.GetZone()?.IsField == true;
        public static IEnumerable<FateRow> GetZoneFates(this ZoneRow z) => z.FateIds.Select(i => Database.Fates[i]);
        #endregion
    }
}
