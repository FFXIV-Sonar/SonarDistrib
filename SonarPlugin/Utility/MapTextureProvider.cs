using ImGuiScene;
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
        private UiBuilder Ui { get; }
        private IPluginLog Logger { get; }

        public MapTextureProvider(IDataManager data, UiBuilder ui, IPluginLog logger)
        {
            this.Data = data;
            this.Ui = ui;
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
            var texture = this.BuildMapImage(path, "s");
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
            var mapTexFile = this.Data.GetFile<TexFile>(filePath);
            if (mapTexFile is null) return null;

            var maskPath = string.Format(MapFileFormat, mapId, fileName, "m", size);
            var maskTexFile = this.Data.GetFile<TexFile>(maskPath);

            if (maskTexFile is not null)
                return this.Ui.LoadImageRaw(MultiplyBlend(mapTexFile, maskTexFile), mapTexFile.Header.Width, mapTexFile.Header.Width, 4);
            return this.Ui.LoadImageRaw(mapTexFile.GetRgbaImageData(), mapTexFile.Header.Width, mapTexFile.Header.Width, 4);
        }

        private static byte[] MultiplyBlend(TexFile image, TexFile mask)
        {
            if (image.Header.Width != mask.Header.Width || image.Header.Height != mask.Header.Height)
                throw new ArgumentException("image and mask sizes are not the same");

            // Using 32bit color
            const int BytesPerPixel = 4;

            var aRgba = image.GetRgbaImageData();
            var bRgba = mask.GetRgbaImageData();
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
