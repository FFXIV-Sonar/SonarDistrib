using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumina.Excel.Sheets;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.IO;
using SonarUtils;
using Sonar.Data.Details;
using SonarResources.Lumina;
using Lumina;
using Lumina.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using SixLabors.ImageSharp.Formats.Jpeg;
using Lumina.Data.Files;

namespace SonarResources.Maps
{
    public struct MapPaths
    {
        public string Image { get; set; }
        public string Mask { get; set; }

    }

    public struct MapTextures
    {
        public TexFile? Image { get; set; }
        public TexFile? Mask { get; set; }

    }

    public enum MapSize
    {
        Tiny, // 256
        Small, // 512
        Medium, // 1024
        Large, // 2048
    }

    public sealed class MapsGenerator
    {
        private readonly HashSet<uint> _processedMaps = new();
        private readonly HashSet<uint> _processedZones = new();
        private volatile int _count; // Interlocked increment

        public bool GenerateMapImages(GameData data, bool parallel)
        {
            Console.WriteLine($"Generating Map Images from {data.DataPath}");
            var maps = data.GetExcelSheet<Map>()!;

            this._count = 0;
            maps
                .Where(m => !string.IsNullOrWhiteSpace(m.Id.ExtractText()))
                .Where(map => this._processedMaps.Add(map.RowId))
                .AsParallel().WithDegreeOfParallelism(parallel ? Environment.ProcessorCount : 1)
                .ForAll(map => this.GenerateMapImages(data, map));

            Console.WriteLine($"Generated {this._count} map images");
            return true;
        }

        /// <summary>WARNING: Assumes <see cref="GenerateMapImages(GameData, bool)"/> has been run</summary>
        public bool GenerateZoneImages(GameData data)
        {
            Console.WriteLine($"Generating Zone Images from {data.DataPath}");
            var zones = data.GetExcelSheet<TerritoryType>()!;

            this._count = 0;
            zones
                .Where(t => t.Map.IsValid && !string.IsNullOrWhiteSpace(t.Map.Value.Id.ExtractText()))
                .Where(territory => this._processedZones.Add(territory.RowId))
                .ForEach(territory => this.GenerateZoneImages(data, territory));

            Console.WriteLine($"Generated {this._count} zone images");
            return true;
        }

        private void GenerateMapImages(GameData data, Map map)
        {
            var placeNames = data.GetExcelSheet<PlaceName>()!;
            Console.WriteLine($"Processing {placeNames.GetRowOrDefault(map.PlaceName.RowId)?.Name.ExtractText() ?? "UNKNOWN MAP"} ({map.RowId})");

            var textures = map.GetMapTextures(data);
            if (textures.Image is null) return;
            using var mapImage = textures.Image.ToImage();
            if (textures.Mask is not null)
            {
                using var maskImage = textures.Mask.ToImage();
                using var resultImage = mapImage.MultiplyWith(maskImage);
                SaveMap(resultImage, map);
            }
            else
            {
                SaveMap(mapImage, map);
            }
            Interlocked.Increment(ref this._count);
        }

        private void GenerateZoneImages(GameData data, TerritoryType territoryType)
        {
            var placeNames = data.GetExcelSheet<PlaceName>()!;
            Console.WriteLine($"Processing {placeNames?.GetRowOrDefault(territoryType.PlaceName.RowId)?.Name.ExtractText() ?? "UNKNOWN ZONE"} ({territoryType.RowId})");
            foreach (var size in Enum.GetValues<MapSize>())
            {
                foreach (var extension in new[] { "jpg", "png", "gif", "webp" })
                {
                    var src = territoryType.GetMapImageAssetPath(size, extension);
                    var dst = territoryType.GetZoneImageAssetPath(size, extension);
                    File.Copy(src, dst, true);
                }
            }
            Interlocked.Increment(ref this._count);
        }

        public static void SaveMap(Image<Bgra32> image, Map map)
        {
            if (image.Width != 2048 || image.Height != 2048)
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw new ArgumentException("Image is not 2048x2048", nameof(image));
            }

            // 2048x2048
            image.SaveAsJpeg($@"../../../Assets/images/map-{map.RowId}-l.jpg");
            image.SaveAsPng($@"../../../Assets/images/map-{map.RowId}-l.png");
            image.SaveAsGif($@"../../../Assets/images/map-{map.RowId}-l.gif");
            image.SaveAsWebp($@"../../../Assets/images/map-{map.RowId}-l.webp");

            // 1024x1024
            using (var resized = image.Clone(x => x.Resize(1024, 1024)))
            {
                resized.SaveAsJpeg($@"../../../Assets/images/map-{map.RowId}-m.jpg");
                resized.SaveAsPng($@"../../../Assets/images/map-{map.RowId}-m.png");
                resized.SaveAsGif($@"../../../Assets/images/map-{map.RowId}-m.gif");
                resized.SaveAsWebp($@"../../../Assets/images/map-{map.RowId}-m.webp");
            }

            // 512x512
            using (var resized = image.Clone(x => x.Resize(512, 512)))
            {
                resized.SaveAsJpeg($@"../../../Assets/images/map-{map.RowId}-s.jpg");
                resized.SaveAsPng($@"../../../Assets/images/map-{map.RowId}-s.png");
                resized.SaveAsGif($@"../../../Assets/images/map-{map.RowId}-s.gif");
                resized.SaveAsWebp($@"../../../Assets/images/map-{map.RowId}-s.webp");
            }

            // 256x256
            using (var resized = image.Clone(x => x.Resize(256, 256)))
            {
                resized.SaveAsJpeg($@"../../../Assets/images/map-{map.RowId}-t.jpg");
                resized.SaveAsPng($@"../../../Assets/images/map-{map.RowId}-t.png");
                resized.SaveAsGif($@"../../../Assets/images/map-{map.RowId}-t.gif");
                resized.SaveAsWebp($@"../../../Assets/images/map-{map.RowId}-t.webp");
            }
        }
    }
}
