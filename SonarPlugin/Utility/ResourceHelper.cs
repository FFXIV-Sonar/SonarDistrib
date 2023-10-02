using ImGuiScene;
using System;
using Dalamud.Logging;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Internal;

namespace SonarPlugin.Utility
{
    [SingletonService]
    public sealed class ResourceHelper
    {
        private UiBuilder Ui { get; }
        private IPluginLog Logger { get; }
        
        public ResourceHelper(UiBuilder ui, IPluginLog logger)
        {
            this.Ui = ui;
            this.Logger = logger;
        }

        public IDalamudTextureWrap LoadIcon(string filename)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SonarPlugin.Resources.Icons.{filename}");
            if (stream is null)
            {
                this.Logger.Warning($"Embedded resource not found while loading icon image: {filename}");
                return this.Ui.LoadImageRaw(new byte[] { 255, 0, 0, 127 }, 1, 1, 4);
            }

            var bytes = new byte[(int)stream.Length];
            stream.Read(bytes);
            try
            {
                return this.Ui.LoadImage(bytes);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"Failed to load icon image: {filename}, loading fallback");
                return this.Ui.LoadImageRaw(new byte[] { 255, 0, 0, 127 }, 1, 1, 4);
            }
        }
    }
}
