using Lumina;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using Lumina.Data.Files;
using Dalamud.Utility;
using Dalamud.Data;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures.TextureWraps;
using System.Linq.Expressions;
using MessagePack.Resolvers;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace SonarPlugin.Utility
{
    [SingletonService]
    public sealed class MapTextureProvider : IDisposable
    {
        private readonly Tasker _tasker = new();
        private readonly Dictionary<string, IDalamudTextureWrap> _textures = new();
        private readonly HashSet<string> _loading = new();
        private readonly object _texturesLock = new();

        private IDataManager Data { get; }
        private ITextureProvider Textures { get; }
        private IPluginLog Logger { get; }

        public MapTextureProvider(IDataManager data, ITextureProvider textures, IPluginLog logger)
        {
            this.Data = data;
            this.Textures = textures;
            this.Logger = logger;

            this.Logger.Information("Map Texture Provider initialized");
        }

        public IDalamudTextureWrap? GetMapTexture(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            lock (this._texturesLock)
            {
                if (this._textures.ContainsKey(path)) return this._textures[path];
                if (this._loading.Add(path))
                {
                    this._tasker.AddTask(this.LoadMapTextureAsync(path));
                }
            }
            return null;
        }

        private async Task LoadMapTextureAsync(string path)
        {
            await Task.Yield();
            var texture = this.BuildMapImage(path, "m");
            if (texture is null) return;
            lock (this._texturesLock)
            {
                if (this._disposed != 0)
                {
                    texture.Dispose();
                    return;
                }
                this._textures[path] = texture;
            }
        }

        // Adapted from https://github.com/ufx/SaintCoinach/blob/master/SaintCoinach/Xiv/Map.cs
        public IDalamudTextureWrap? BuildMapImage(string mapId, string size)
        {
            const string MapFileFormat = "ui/map/{0}/{1}{2}_{3}.tex";
            var fileName = mapId.Replace("/", "");

            var filePath = string.Format(MapFileFormat, mapId, fileName, string.Empty, size);
            var maskPath = string.Format(MapFileFormat, mapId, fileName, "m", size);

            try
            {
                var mapTexBuffer = this.Data.GetFile<TexFile>(filePath)?.TextureBuffer.Filter(0, 0, TexFile.TextureFormat.B8G8R8A8);
                if (mapTexBuffer is null) return null;
                var mapTexBytes = Downscale(Downscale(mapTexBuffer.RawData, mapTexBuffer.Width, mapTexBuffer.Height), mapTexBuffer.Width / 2, mapTexBuffer.Height / 2);

                var maskTexBuffer = this.Data.GetFile<TexFile>(maskPath)?.TextureBuffer.Filter(0, 0, TexFile.TextureFormat.B8G8R8A8);

                if (maskTexBuffer is not null)
                {
                    var maskTexBytes = Downscale(Downscale(maskTexBuffer.RawData, mapTexBuffer.Width, mapTexBuffer.Height), mapTexBuffer.Width / 2, mapTexBuffer.Height / 2);
                    return this.Textures.CreateFromRaw(new(mapTexBuffer.Width / 4, mapTexBuffer.Height / 4, 87), MultiplyBlend(mapTexBytes, maskTexBytes), $"Sonar MAP {mapId}");
                }
                return this.Textures.CreateFromRaw(new(mapTexBuffer.Width / 4, mapTexBuffer.Height / 4, 87), mapTexBytes, $"Sonar MAP {mapId}");
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Exception occured while building map image");
                throw;
            }
        }

        public static byte[] Downscale(ReadOnlySpan<byte> origImage, int origWidth, int origHeight)
        {
            static int GetOffset(int x, int y, int width) => (y * width + x) * 4;
            var newWidth = origWidth / 2; var newHeight = origHeight / 2;
            var newImage = new byte[newWidth * newHeight * 4];

            for (var y = 0; y < newHeight; y++)
            {
                for (var x = 0; x < newWidth; x++)
                {
                    var newOffset = GetOffset(x, y, newWidth);
                    var c0 = 0; var c1 = 0; var c2 = 0; var c3 = 0;
                    for (var oy = 0; oy < 2; oy++)
                    {
                        for (var ox = 0; ox < 2; ox++)
                        {
                            var origOffset = GetOffset(x * 2 + ox, y * 2 + oy, origWidth);
                            c0 += origImage[origOffset + 0];
                            c1 += origImage[origOffset + 1];
                            c2 += origImage[origOffset + 2];
                            c3 += origImage[origOffset + 3];
                        }
                    }
                    newImage[newOffset + 0] = (byte)(c0 / 4);
                    newImage[newOffset + 1] = (byte)(c1 / 4);
                    newImage[newOffset + 2] = (byte)(c2 / 4);
                    newImage[newOffset + 3] = (byte)(c3 / 4);
                }
            }
            return newImage;
        }


        private static byte[] MultiplyBlend(byte[] image, byte[] mask)
        {
            if (image.Length != mask.Length) throw new InvalidOperationException("Image sizes are not the same");

            // Using 32bit color
            const int BytesPerPixel = 4;

            var aRgba = image;
            var bRgba = mask;
            var result = new byte[aRgba.Length];

            for (var i = 0; i < aRgba.Length; i += BytesPerPixel)
            {
                for (var j = 0; j < 3; ++j)
                    result[i + j] = (byte)(aRgba[i + j] * bRgba[i + j] / 255);
                result[i + 3] = aRgba[i + 3];
            }
            return result;
        }

        #region IDisposable Pattern
        private int _disposed;
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this._disposed, 1, 0) != 0) return;
            this._tasker.Dispose();
            lock (this._texturesLock)
            {
                foreach (var tex in this._textures.Values)
                {
                    tex.Dispose();
                }
                this._textures.Clear();
            }
        }
        #endregion
    }
}
