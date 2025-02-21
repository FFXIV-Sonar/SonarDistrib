using Lumina;
using Lumina.Data.Files;
using Lumina.Excel.Sheets;
using Lumina.Models.Materials;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SonarResources.Maps
{
    // Paths here are assumed
    public static class MapGeneratorExtensions
    {
        public const string MapFileFormat = "ui/map/{0}/{1}{2}_m.tex";
        public static readonly IReadOnlyDictionary<MapSize, string> SizeSuffix = new Dictionary<MapSize, string>()
        {
            {MapSize.Tiny, "t" },
            {MapSize.Small, "s" },
            {MapSize.Medium, "m" },
            {MapSize.Large, "l" },
        };

        public static MapPaths GetMapGameDataPaths(string mapId)
        {
            var fileName = mapId.Replace("/", "");
            return new MapPaths()
            {
                Image = string.Format(MapFileFormat, mapId, fileName, string.Empty),
                Mask = string.Format(MapFileFormat, mapId, fileName, "m")
            };
        }
        public static MapPaths GetMapGameDataPaths(this Map map) => GetMapGameDataPaths(map.Id.ExtractText());

        public static string GetZoneImageAssetPath(uint id, MapSize size, string extension) => $"../../../Assets/images/zone-{id}-{SizeSuffix[size]}.{extension}";
        public static string GetZoneImageAssetPath(this TerritoryType territoryType, MapSize size, string extension) => GetZoneImageAssetPath(territoryType.RowId, size, extension);

        public static string GetMapImageAssetPath(uint id, MapSize size, string extension) => $"../../../Assets/images/map-{id}-{SizeSuffix[size]}.{extension}";
        public static string GetMapImageAssetPath(this Map map, MapSize size, string extension) => GetMapImageAssetPath(map.RowId, size, extension);
        public static string GetMapImageAssetPath(this TerritoryType territoryType, MapSize size, string extension) => GetMapImageAssetPath(territoryType.Map.Value, size, extension);

        public static Image<Bgra32>? ToMapImage(this Map map, GameData data)
        {
            var textures = map.GetMapTextures(data);

            if (textures.Image is null) return null;
            var result = textures.Image.ToImage();

            if (textures.Mask is not null)
            {
                using var maskImage = textures.Mask.ToImage();
                result.Mutate(ctx => ctx.DrawImage(maskImage, PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.SrcOver, 1.0f));
            }
            return result;
        }

        public static Image<Rgba32>? ToMapImageRgba(this Map map, GameData data)
        {
            using var image = map.ToMapImage(data);
            return image?.CloneAs<Rgba32>();
        }

        public static Image<Bgra32>? ToMapImage(this TerritoryType zone, GameData data)
        {
            var map = zone.Map.ValueNullable;
            if (map is null) return null;
            return map.Value.ToMapImage(data);
        }

        public static MapTextures GetMapTextures(this MapPaths paths, GameData data)
        {
            var imageExists = data.FileExists(paths.Image);
            var maskExists = data.FileExists(paths.Mask);

            return new()
            {
                Image = imageExists ? data.GetFile<TexFile>(paths.Image) : null,
                Mask = imageExists && maskExists ? data.GetFile<TexFile>(paths.Mask) : null
            };
        }

        public static MapTextures GetMapTextures(this Map map, GameData data) => map.GetMapGameDataPaths().GetMapTextures(data);
    }
}
