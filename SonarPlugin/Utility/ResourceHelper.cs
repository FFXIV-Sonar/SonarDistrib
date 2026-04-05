using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using System;
using System.Reflection;

namespace SonarPlugin.Utility
{
    [ExportMany]
    [SingletonReuse]
    public sealed class ResourceHelper
    {
        private ITextureProvider Textures { get; }
        private IPluginLog Logger { get; }
        
        public ResourceHelper(ITextureProvider textures, IPluginLog logger)
        {
            this.Textures = textures;
            this.Logger = logger;
        }

        public IDalamudTextureWrap LoadIcon(string filename)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SonarPlugin.Resources.Icons.{filename}");
            if (stream is null)
            {
                this.Logger.Warning($"Embedded resource not found while loading icon image: {filename}");
                return this.Textures.CreateFromRaw(new(1, 1, 28), [255, 0, 0, 127]);
            }

            var bytes = new byte[(int)stream.Length];
            stream.ReadExactly(bytes);
            try
            {
                return this.Textures.CreateFromImageAsync(bytes).Result;
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"Failed to load icon image: {filename}, loading fallback");
                return this.Textures.CreateFromRaw(new(1, 1, 28), [255, 0, 0, 127]);
            }
        }
    }
}
