using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.IO;

namespace SonarResources.Maps
{
    public static class MapUtils
    {
        public static readonly IReadOnlyDictionary<MapSize, string> SizeSuffix = new Dictionary<MapSize, string>()
        {
            {MapSize.Tiny, "t" },
            {MapSize.Small, "s" },
            {MapSize.Medium, "m" },
            {MapSize.Large, "l" },
        };

        public static string GetZoneImageAssetPath(uint id, MapSize size, string extension) => Path.Join(Program.Config.AssetsPath, "images", $"zone-{id}-{SizeSuffix[size]}.{extension}");
        public static string GetZoneImageAssetPath(this TerritoryType territoryType, MapSize size, string extension) => GetZoneImageAssetPath(territoryType.RowId, size, extension);
        public static string GetMapImageAssetPath(uint id, MapSize size, string extension) => Path.Join(Program.Config.AssetsPath, "images", $"map-{id}-{SizeSuffix[size]}.{extension}");
        public static string GetMapImageAssetPath(this Map map, MapSize size, string extension) => GetMapImageAssetPath(map.RowId, size, extension);
        public static string GetMapImageAssetPath(this TerritoryType territoryType, MapSize size, string extension) => GetMapImageAssetPath(territoryType.Map.Value, size, extension);

    }
}
