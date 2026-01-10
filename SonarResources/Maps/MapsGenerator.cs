using DryIoc.FastExpressionCompiler.LightExpression;
using FileSystemLinks;
using ImageMagick;
using ImageMagick.Formats;
using Lumina;
using Lumina.Data.Files;
using Lumina.Excel.Sheets;
using Org.BouncyCastle.Crypto.Digests;
using Sonar.Data.Details;
using SonarUtils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonarResources.Maps
{
    public struct MapTextures
    {
        public TexFile? Image { get; set; }
        public TexFile? Mask { get; set; }

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
                    if (blurHash is not null)
                    {
                        var mapRow = db.Maps.GetValueOrDefault(map.RowId);
                        if (mapRow is not null) mapRow.BlurHash = blurHash;
                    }
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

            const int maxProgress = (64 + 16 + 4 + 1) * 4 + 1 + 1;
            var progress = progressContext.AddTask($"[green]{mapName.EscapeMarkup()}[/]", true, maxProgress);

            // Load map image
            var texImage = map.GetImageTex(data);
            if (texImage is null) return null;
            var texMask = map.GetMaskTex(data);

            // Image dimensions from the game files are assumed to be constant.
            Debug.Assert(texImage.Header.Width is 2048 && texImage.Header.Height is 2048, "Expected 2048x2048 texture");

            // Mask files are assumed to be the same dimensions as the image files.
            Debug.Assert(texMask is null || (texImage.Header.Width == texMask.Header.Width && texImage.Header.Height == texMask.Header.Height), "Expected texImage dimensions to be equal texMask dimensions");

            // Prepare digester.
            var digest = new Sha256Digest();
            var digestSize = digest.GetDigestSize();
            var newDigestBytes = new byte[digestSize];

            // Compute new digest.
            digest.BlockUpdate(texImage.Data);
            if (texMask is not null) digest.BlockUpdate(texMask.Data);
            _ = digest.DoFinal(newDigestBytes);

            // Compare previous digest.
            var checkFile = Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}.rawsum");
            try
            {
                // Read previously computed digest bytes.
                var oldDigestBytes = await File.ReadAllBytesAsync(checkFile, cancellationToken);

                // Shortcut completion if digests match.
                if (oldDigestBytes.SequenceEqual(newDigestBytes))
                {
                    progress.Increment(maxProgress);
                    progress.StopTask();
                    return null;
                }
            }
            catch
            {
                /* Swallow */
            }

            // Generate MagickMap
            using var image = texImage.ToMagickImageWithMask(texMask);
            Debug.Assert(image is not null);
            image.Quality = 75;

            // Increment progress
            progress.Increment(1);

            // Magick Defines
            var jpegDefines = new JpegWriteDefines()
            {
                DctMethod = JpegDctMethod.Float,
                SamplingFactor = JpegSamplingFactor.Ratio420,
                OptimizeCoding = true,
            };

            var pngDefines = new PngWriteDefines()
            {
                CompressionLevel = 9,
            };

            var webpDefines = new WebPWriteDefines()
            {
                AlphaCompression = WebPAlphaCompression.Compressed,
                AlphaFiltering = WebPAlphaFiltering.Best,
                AlphaQuality = 100,

                AutoFilter = true,
                FilterType = WebPFilterType.Strong,
                Method = 6,
                Preprocessing = WebPPreprocessing.PseudoRandom,

                Pass = 10,
                ThreadLevel = false,
                UseSharpYuv = true,
            };

            // Blurhash
            string blurHash;

            // 2048x2048 (l)
            using (var resized = image.CloneAndMutate(source => source.Resize(2048, 2048)))
            {
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.jpg"), jpegDefines, cancellationToken); progress.Increment(64);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.png"), pngDefines, cancellationToken); progress.Increment(64);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.gif"), MagickFormat.Gif, cancellationToken); progress.Increment(64);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-l.webp"), webpDefines, cancellationToken); progress.Increment(64);
            }

            // 1024x1024 (m)
            using (var resized = image.CloneAndMutate(source => source.Resize(1024, 1024)))
            {
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.jpg"), jpegDefines, cancellationToken); progress.Increment(16);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.png"), pngDefines, cancellationToken); progress.Increment(16);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.gif"), MagickFormat.Gif, cancellationToken); progress.Increment(16);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-m.webp"), webpDefines, cancellationToken); progress.Increment(16);
            }

            // 512x512 (s)
            using (var resized = image.CloneAndMutate(source => source.Resize(512, 512)))
            {
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.jpg"), jpegDefines, cancellationToken); progress.Increment(4);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.png"), pngDefines, cancellationToken); progress.Increment(4);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.gif"), MagickFormat.Gif, cancellationToken); progress.Increment(4);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-s.webp"), webpDefines, cancellationToken); progress.Increment(4);
            }

            // 256x256 (t)
            using (var resized = image.CloneAndMutate(source => source.Resize(256, 256)))
            {
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.jpg"), jpegDefines, cancellationToken); progress.Increment(1);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.png"), pngDefines, cancellationToken); progress.Increment(1);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.gif"), MagickFormat.Gif, cancellationToken); progress.Increment(1);
                using (var clone = resized.Clone()) await clone.WriteAsync(Path.Join(Program.Config.AssetsPath, "images", $"map-{map.RowId}-t.webp"), webpDefines, cancellationToken); progress.Increment(1);

                var imagePixels = resized.GetPixels();
                var blurPixels = new Blurhash.Pixel[256, 256];
                var pos = 0;
                foreach (var imagePixel in imagePixels)
                {
                    var (y, x) = Math.DivRem(pos, 256);
                    ref var blurPixel = ref blurPixels[x, y];
                    blurPixel.Red = Blurhash.MathUtils.SRgbToLinear(imagePixel.GetChannel(0));
                    blurPixel.Green = Blurhash.MathUtils.SRgbToLinear(imagePixel.GetChannel(1));
                    blurPixel.Blue = Blurhash.MathUtils.SRgbToLinear(imagePixel.GetChannel(2));
                    pos++;
                }
                blurHash = Blurhash.Core.Encode(blurPixels, 4, 4);
                progress.Increment(1);
            }

            // Write newly computed digest.
            await File.WriteAllBytesAsync(checkFile, newDigestBytes, cancellationToken);

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
