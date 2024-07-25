using DryIocAttributes;
using Humanizer;
using Lumina;
using Lumina.Excel.GeneratedSheets2;
using Sonar.Data.Details;
using Sonar.Enums;
using SonarResources.Lumina;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class ZoneReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }

        public ZoneReader(LuminaManager luminas, SonarDb db, MapReader _)
        {
            this.Luminas = luminas;
            this.Db = db;

            Console.WriteLine("Reading all zones");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.Read(entry) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this.Db.Zones.Count})");

            Console.WriteLine("Setting up zones");
            this.SetupZones();
        }

        public bool Read(LuminaEntry lumina)
        {
            var territorySheet = lumina.Data.GetExcelSheet<TerritoryType>();
            if (territorySheet is null) return false;

            var transientSheet = lumina.Data.GetExcelSheet<TerritoryTypeTransient>();
            if (transientSheet is null) return false;

            var placeNames = lumina.Data.GetExcelSheet<PlaceName>(lumina.LuminaLanguage);
            if (placeNames is null) return false;

            var result = false;
            foreach (var territory in territorySheet)
            {
                var id = territory.RowId;

                // OffsetZ
                float offsetZ;
                try { offsetZ = transientSheet.GetRow(id)?.OffsetZ ?? -10000; } catch { offsetZ = -10000; }
                var hasOffsetZ = offsetZ != -10000;
                if (!hasOffsetZ) offsetZ = 0;

                if (!this.Db.Zones.TryGetValue(id, out var zone))
                {
                    this.Db.Zones[id] = zone = new()
                    {
                        Id = id,
                        MapId = territory.Map.Row,
                        Scale = territory.Map.Value!.SizeFactor / 100f,
                        Offset = new(territory.Map.Value.OffsetX, territory.Map.Value.OffsetY, offsetZ),
                        HasOffsetZ = hasOffsetZ,
                        MapResourcePath = territory.Map.Value.Id,
                        Expansion = GetZoneExpansion(territory.Bg),
                        IsField = territory.TerritoryIntendedUse.Value?.RowId == 1 || territory.TerritoryIntendedUse.Value?.RowId == 41 || territory.TerritoryIntendedUse.Value?.RowId == 48, // && t.Stealth && t.Mount && t.Aetheryte.Row != 0 && !t.IsPvpZone,
                        LocalOnly = territory.TerritoryIntendedUse.Value?.RowId == 41 || territory.TerritoryIntendedUse.Value?.RowId == 48,
                    };
                }

                if (!zone.Name.ContainsKey(lumina.SonarLanguage))
                {
                    var name = placeNames.GetRow(territory.PlaceName.Row)?.Name?.ToTextString()?.Transform(To.TitleCase);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        zone.Name[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }

                if (!zone.Region.ContainsKey(lumina.SonarLanguage))
                {
                    var name = placeNames.GetRow(territory.PlaceNameRegion.Row)?.Name?.ToTextString()?.Transform(To.TitleCase);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        zone.Region[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }
            }
            return result;
        }

        private void SetupZones()
        {
            this.Db.Zones[630].IsField = false; // The Diadem
            this.Db.Zones[886].IsField = true; // The Firmament (for Fetes)
            this.Db.Zones[967].IsField = false; // Castrum Marinum Drydocks
        }

        public static ExpansionPack GetZoneExpansion(string bg)
        {
            if (bg.StartsWith("ffxiv", StringComparison.InvariantCultureIgnoreCase)) return ExpansionPack.ARealmReborn;
            if (bg.StartsWith("ex1", StringComparison.InvariantCultureIgnoreCase)) return ExpansionPack.Heavensward;
            if (bg.StartsWith("ex2", StringComparison.InvariantCultureIgnoreCase)) return ExpansionPack.Stormblood;
            if (bg.StartsWith("ex3", StringComparison.InvariantCultureIgnoreCase)) return ExpansionPack.Shadowbringers;
            if (bg.StartsWith("ex4", StringComparison.InvariantCultureIgnoreCase)) return ExpansionPack.Endwalker;
            if (bg.StartsWith("ex5", StringComparison.InvariantCultureIgnoreCase)) return ExpansionPack.Dawntrail;
            return ExpansionPack.Unknown;
        }
    }
}
