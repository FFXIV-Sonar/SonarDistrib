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
using Lumina.Data.Files;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using Spectre.Console;
using static System.Net.Mime.MediaTypeNames;
using FileSystemLinks;
using Blurhash.ImageSharp;
using Sonar.Data;

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
        private readonly HashSet<uint> _processedMaps = [];
        private readonly HashSet<uint> _processedZones = [];

        public async Task GenerateAllMapImagesAsync(GameData data, SonarDb db, int parallel, CancellationToken cancellationToken = default)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow bold]Generating map images from:[/] [white bold]{data.DataPath}[/]");
            var maps = data.GetExcelSheet<Map>()!
                .Where(map => this._processedMaps.Add(map.RowId) && !string.IsNullOrWhiteSpace(map.Id.ExtractText()))
                .ToList();

            await AnsiConsole.Progress()
                .HideCompleted(true)
                .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
                .StartAsync(async ctx =>
            {
                var progress = ctx.AddTask($"[yellow]Total Progress:[/] [white]0/{maps.Count}[/]", true, maps.Count);
                var options = new ParallelOptions()
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = parallel,
                };
                await Parallel.ForEachAsync(maps, options, async (map, ct) =>
                {
                    var blurHash = await this.GenerateMapAssetsAsync(data, map, ctx, ct);
                    var mapRow = db.Maps.GetValueOrDefault(map.RowId);
                    if (mapRow is not null) mapRow.BlurHash = blurHash;
                    progress.Increment(1);
                    progress.Description($"[yellow]Total Progress:[/] [white]{(int)progress.Value}/{maps.Count}[/]");
                });
                progress.StopTask();
            });
        }

        /// <summary>WARNING: Assumes <see cref="GenerateMapImagesAsync(GameData, bool)"/> has been run</summary>
        public async Task GenerateAllZoneImagesAsync(GameData data, CancellationToken cancellationToken = default)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow bold]Generatring zone images from:[/] [white bold]{data.DataPath}[/]");
            var zones = data.GetExcelSheet<TerritoryType>()!
                .Where(zone => this._processedZones.Add(zone.RowId) && !string.IsNullOrWhiteSpace(zone.Map.ValueNullable?.Id.ExtractText()))
                .ToList();

            await AnsiConsole.Progress()
                .HideCompleted(true)
                .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
                .StartAsync(async ctx =>
                {
                    var progress = ctx.AddTask("[yellow]Total Progress:[/] [white]0/{zones.Count}[/]", true, zones.Count);
                    foreach (var zone in zones)
                    {
                        await this.GenerateMapAssetsAsync(data, zone, ctx, cancellationToken);
                        progress.Increment(1);
                        progress.Description($"[yellow]Total Progress:[/] [white]{(int)progress.Value}/{zones.Count}[/]");
                    }
                });
        }

        private async Task<string?> GenerateMapAssetsAsync(GameData data, Map map, ProgressContext progressContext, CancellationToken cancellationToken = default)
        {
            var placeNames = data.GetExcelSheet<PlaceName>()!;
            var mapName = $"{placeNames?.GetRowOrDefault(map.PlaceName.RowId)?.Name.ExtractText() ?? "UNKNOWN MAP"} ({map.RowId})";

            var progress = progressContext.AddTask($"[green]{mapName.EscapeMarkup()}[/]", true, (64 + 16 + 4 + 1) * 4 + 1 + 1);

            using var image = map.ToMapImage(data);
            if (image is null)
            {
                progress.StopTask();
                return null;
            }
            progress.Increment(1);

            Debug.Assert(image.Width is 2048 && image.Height is 2048);

            // Encoders
            var pngEncoder = new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression };
            var webpEncoder = new WebpEncoder() { Method = WebpEncodingMethod.BestQuality, EntropyPasses = 10 };

            // 2048x2048
            await image.SaveAsJpegAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.jpg"), cancellationToken); progress.Increment(64);
            await image.SaveAsPngAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.png"), pngEncoder, cancellationToken); progress.Increment(64);
            await image.SaveAsGifAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.gif"), cancellationToken); progress.Increment(64);
            await image.SaveAsWebpAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.webp"), webpEncoder, cancellationToken); progress.Increment(64);

            string blurHash;

            // 1024x1024
            using (var resized = image.Clone(x => x.Resize(1024, 1024)))
            {
                await resized.SaveAsJpegAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.jpg"), cancellationToken); progress.Increment(16);
                await resized.SaveAsPngAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.png"), pngEncoder, cancellationToken); progress.Increment(16);
                await resized.SaveAsGifAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.gif"), cancellationToken); progress.Increment(16);
                await resized.SaveAsWebpAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.webp"), webpEncoder, cancellationToken); progress.Increment(16);
            }

            // 512x512
            using (var resized = image.Clone(x => x.Resize(512, 512)))
            {
                await resized.SaveAsJpegAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.jpg"), cancellationToken); progress.Increment(4);
                await resized.SaveAsPngAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.png"), pngEncoder, cancellationToken); progress.Increment(4);
                await resized.SaveAsGifAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.gif"), cancellationToken); progress.Increment(4);
                await resized.SaveAsWebpAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.webp"), webpEncoder, cancellationToken); progress.Increment(4);
            }

            // 256x256
            using (var resized = image.Clone(x => x.Resize(256, 256)))
            {
                await resized.SaveAsJpegAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.jpg"), cancellationToken); progress.Increment(1);
                await resized.SaveAsPngAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.png"), pngEncoder, cancellationToken); progress.Increment(1);
                await resized.SaveAsGifAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.gif"), cancellationToken); progress.Increment(1);
                await resized.SaveAsWebpAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.webp"), webpEncoder, cancellationToken); progress.Increment(1);

                // Swap from BGR to RGB.
                // BlurHash doesn't use Alpha. There's an Rgba32 overload but alpha is ignored.
                using var blurImage = resized.CloneAs<Rgb24>();
                blurHash = Blurhasher.Encode(blurImage, 4, 4);
                progress.Increment(1);
            }

            progress.StopTask();
            return blurHash;
        }

        private async Task GenerateMapAssetsAsync(GameData data, TerritoryType territoryType, ProgressContext progressContext, CancellationToken cancellationToken = default)
        {
            var placeNames = data.GetExcelSheet<PlaceName>()!;
            var zoneName = $"{placeNames?.GetRowOrDefault(territoryType.PlaceName.RowId)?.Name.ExtractText() ?? "UNKNOWN ZONE"} ({territoryType.RowId})";

            var sizes = Enum.GetValues<MapSize>();
            string[] extensions = ["jpg", "png", "gif", "webp"];

            var progress = progressContext.AddTask($"[green]{zoneName.EscapeMarkup()}[/]", true, (64 + 16 + 4 + 1) * extensions.Length);
            foreach (var size in sizes)
            {
                foreach (var extension in extensions)
                {
                    var src = territoryType.GetMapImageAssetPath(size, extension);
                    var dst = territoryType.GetZoneImageAssetPath(size, extension);
                    if (File.Exists(dst)) File.Delete(dst);
                    try
                    {
                        FileSystemLink.CreateHardLink(src, dst);
                    }
                    catch
                    {
                        await File.WriteAllBytesAsync(dst, await File.ReadAllBytesAsync(src, cancellationToken), cancellationToken);
                    }
                    progress.Increment(Math.Pow(4, (int)size));
                }
            }
            progress.StopTask();
        }
    }
}
