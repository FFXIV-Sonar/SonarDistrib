using DryIocAttributes;
using Humanizer;
using Lumina.Excel.GeneratedSheets;
using Sonar.Data.Details;
using SonarResources.Lumina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class MapReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }

        public MapReader(LuminaManager luminas, SonarDb db)
        {
            this.Luminas = luminas;
            this.Db = db;

            Console.WriteLine("Reading all maps");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.Read(entry) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this.Db.Maps.Count})");
        }

        public bool Read(LuminaEntry lumina)
        {
            var mapSheet = lumina.Data.GetExcelSheet<Map>(lumina.LuminaLanguage);
            if (mapSheet is null) return false;

            var transientSheet = lumina.Data.GetExcelSheet<TerritoryTypeTransient>(lumina.LuminaLanguage);
            if (transientSheet is null) return false;

            var placeNames = lumina.Data.GetExcelSheet<PlaceName>(lumina.LuminaLanguage);
            if (placeNames is null) return false;

            var result = false;
            foreach (var luminaMap in mapSheet)
            {
                var id = luminaMap.RowId;
                var zoneId = luminaMap.TerritoryType.Row;

                // OffsetZ
                float offsetZ;
                try { offsetZ = transientSheet.GetRow(zoneId)?.OffsetZ ?? -10000; } catch { offsetZ = -10000; }
                var hasOffsetZ = offsetZ != -10000;
                if (!hasOffsetZ) offsetZ = 0;

                if (!this.Db.Maps.TryGetValue(id, out var map))
                {
                    this.Db.Maps[id] = map = new()
                    {
                        Id = id,
                        Scale = luminaMap.SizeFactor / 100f,
                        Offset = new(luminaMap.OffsetX, luminaMap.OffsetY, offsetZ),
                        HasOffsetZ = hasOffsetZ,
                        MapResourcePath = luminaMap.Id,
                        ZoneId = zoneId,
                    };
                }

                if (!map.Name.ContainsKey(lumina.SonarLanguage))
                {
                    var name = placeNames.GetRow(luminaMap.PlaceName.Row)?.Name?.ToTextString()?.Transform(To.TitleCase);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        map.Name[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }

                if (!map.SubName.ContainsKey(lumina.SonarLanguage))
                {
                    var name = placeNames.GetRow(luminaMap.PlaceNameSub.Row)?.Name?.ToTextString()?.Transform(To.TitleCase);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        map.SubName[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }

                if (!map.Region.ContainsKey(lumina.SonarLanguage))
                {
                    var name = placeNames.GetRow(luminaMap.PlaceNameRegion.Row)?.Name?.ToTextString()?.Transform(To.TitleCase);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        map.Region[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
