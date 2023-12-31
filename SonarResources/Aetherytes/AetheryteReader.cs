using Lumina;
using Lumina.Excel.GeneratedSheets2;
using Sonar.Data.Details;
using SonarResources.Lgb;
using SonarResources.Lumina;
using SonarResources.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumina.Data;
using Sonar.Data;
using Sonar.Numerics;
using Lumina.Data.Parsing.Layer;
using static Lumina.Data.Parsing.Layer.LayerCommon;
using System.Diagnostics;
using DryIocAttributes;

namespace SonarResources.Aetherytes
{
    [ExportEx]
    [SingletonReuse]
    public sealed class AetheryteReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }
        private LgbInstancesReader Lgb { get; }

        public AetheryteReader(LuminaManager luminas, SonarDb db, LgbInstancesReader lgb, ZoneReader _)
        {
            this.Luminas = luminas;
            this.Db = db;
            this.Lgb = lgb;

            Console.WriteLine("Reading all aetherytes");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                try
                {
                    Program.WriteProgress(this.Read(entry) ? "+" : ".");
                }
                catch (Exception ex)
                {
                    Program.WriteProgress("e");
                }
            }
            Program.WriteProgressLine($" ({this.Db.Aetherytes.Count})");

            Console.WriteLine("Scanning aetheryte coordinates through map markers");
            foreach (var data in this.Luminas.GetAllDatas())
            {
                Program.WriteProgress(this.ScanMapMarkers(data) ? "+" : ".");
            }
            Program.WriteProgressLine(string.Empty);

            Console.WriteLine("Scanning aetheryte coordinates through LGB instances");
            Program.WriteProgress(this.ScanLgbData() ? "+" : ".");
            Program.WriteProgressLine(string.Empty);

            Console.WriteLine("Setting up Aetherytes");
            this.SetupAetherytes();
            
            Console.WriteLine("Sanity checking aetherytes");
            this.SanityCheck();

            Program.WriteProgressLine($"Teleportable aetherytes: {this.Db.Aetherytes.Values.Count(aetheryte => aetheryte.Teleportable)}");
        }

        private bool Read(LuminaEntry lumina)
        {
            var aetheryteSheet = lumina.Data.GetExcelSheet<Aetheryte>(lumina.LuminaLanguage);
            if (aetheryteSheet is null || !aetheryteSheet.Languages.Contains(lumina.LuminaLanguage)) return false;

            var result = false;
            foreach (var aetheryteRow in aetheryteSheet)
            {
                var id = aetheryteRow.RowId;
                if (!this.Db.Aetherytes.TryGetValue(id, out var aetheryte))
                {
                    this.Db.Aetherytes[id] = aetheryte = new()
                    {
                        Id = id,
                        ZoneId = aetheryteRow.Territory.Row,
                        AethernetGroup = aetheryteRow.AethernetGroup,
                        Teleportable = aetheryteRow.IsAetheryte,
                    };
                    result = true;
                }

                if (!aetheryte.Name.ContainsKey(lumina.SonarLanguage))
                {
                    var name = aetheryteRow.PlaceName.Value?.Name.ToTextString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        aetheryte.Name[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }
            }
            return result;
        }

        private bool ScanMapMarkers(GameData data)
        {
            var markerSheet = data.GetExcelSheet<MapMarker>(Language.None)?
                .Where(marker => marker.DataType is 3);
            if (markerSheet is null) return false;

            var result = false;
            foreach (var marker in markerSheet)
            {
                var id = marker.DataKey.Row;
                var aetheryte = this.Db.Aetherytes[id];
                if (aetheryte.Coords is { X: 0, Y: 0, Z: 0 })
                {
                    var zone = this.Db.Zones[aetheryte.ZoneId];
                    var map = this.Db.Maps[zone.MapId];

                    var pixelCoords = new SonarVector2((float)marker.X, marker.Y);
                    var flagCoords = MapFlagUtils.PixelToFlag(map.Scale, pixelCoords);
                    var rawCoords = MapFlagUtils.FlagToRaw(map.Scale, map.Offset, flagCoords);
                    aetheryte.Coords = rawCoords;
                    aetheryte.Teleportable = true;

                    result = true;
                }
            }
            return result;
        }

        private bool ScanLgbData()
        {
            var aetheryteInstances = this.Lgb.GetInstances()
                .Where(lgb => lgb.Type is LayerEntryType.Aetheryte);

            var result = false;
            foreach (var lgb in aetheryteInstances)
            {
                var aetheryteObj = (AetheryteInstanceObject)lgb.Object;
                var id = aetheryteObj.ParentData.BaseId;

                var aetheryte = this.Db.Aetherytes[id];
                if (aetheryte.Coords is { Z: 0 })
                {
                    Debug.Assert(lgb.ZoneId == aetheryte.ZoneId);
                    if (aetheryte.Coords is not { X: 0, Y: 0 })
                    {
                        Debug.Assert(Math.Abs(lgb.Coords.X - aetheryte.Coords.X) < 10);
                        Debug.Assert(Math.Abs(lgb.Coords.Y - aetheryte.Coords.Y) < 10);
                    }
                    aetheryte.Coords = lgb.Coords;
                    result = true;
                }
            }
            return result;
        }

        private void SetupAetherytes()
        {
            // Aethernet shards SE placeholder
            this.Db.Aetherytes[1].Teleportable = false;

            // 1 second = 20 units / yalms
            this.SetDistanceCost("Tamamizu", 100); // Underwater inside a giant bubble
            this.SetDistanceCost("Pla Enni", 50); // Minor inconvenience getting out of this cave
            this.SetDistanceCost("The Macarenses Angle", 500); // I wish we could fly up faster...
            this.SetDistanceCost("Tertium", 200); // Some inconvenience getting out of this subway station
            this.SetDistanceCost("Bestways Burrow", 100); // Minor inconveience getting out of their base

            /* Special cases / notes:
               - Missing Aetherytes:
                 - The Dravanian Hinterlands

               - Uncomfortable Aetherytes:
                 - Lower La Noscea: Bottom corner of the map
                 - Coerthas Western Highlands: Bottom right corner of the map

               - Potential shortcuts:
                 - Eastern Thanalan <= South Shroud
                 - Coerthas Western Highlands <= The Dravanian Forelands
                 - Coerthas Western Highlands <= The Sea of Clouds
                 - The Dravanian Hinterlands <= Idyllshire
                 - The Dravanian Hinterlands <= The Dravanian Forelands
                 - The Doman Enclave <= Yanxia

               - Additional considerations:
                 - The Ruby Sea skipper
            */
        }

        private void SanityCheck()
        {
            foreach (var aetheryte in this.Db.Aetherytes.Values.Where(aetheryte => aetheryte.Coords is { Z: 0 }))
            {
                var zoneId = aetheryte.ZoneId;
                if (zoneId is 0) continue;

                var zone = this.Db.Zones[zoneId];
                if (aetheryte.Coords is { X: 0, Y: 0 })
                {
                    Console.WriteLine($"WARNING: No coordinates found for Aetheryte {aetheryte.Name} ({aetheryte.Id}): {zone}");
                }
                else
                {
                    var flagCoords = MapFlagUtils.RawToFlag(zone.Scale, zone.Offset, aetheryte.Coords);
                    Console.WriteLine($"WARNING: No LGB coordinates found for Aetheryte {aetheryte.Name} ({aetheryte.Id}): {zone} ({flagCoords.X:F2}, {flagCoords.Y:F2})");
                }
            }
        }

        private void SetDistanceCost(string name, float cost)
        {
            var aetheryte =
                this.Db.Aetherytes.Values.FirstOrDefault(aetheryte => aetheryte.Name.ToString().Equals(name, StringComparison.InvariantCulture)) ??
                this.Db.Aetherytes.Values.FirstOrDefault(aetheryte => aetheryte.Name.ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"Aetheryte {name} does not exist", nameof(name));

            aetheryte.DistanceCostModifier = cost;
        }
    }
}
