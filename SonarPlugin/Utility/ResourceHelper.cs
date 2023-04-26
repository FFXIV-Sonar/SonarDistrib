using ImGuiScene;
using System;
using Dalamud.Logging;
using System.Reflection;
using Dalamud.Interface;

namespace SonarPlugin.Utility
{
    [SingletonService]
    public sealed class ResourceHelper
    {
        private UiBuilder Ui { get; }
        
        public ResourceHelper(UiBuilder ui)
        {
            this.Ui = ui;
        }

        public TextureWrap LoadIcon(string filename)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"SonarPlugin.Resources.Icons.{filename}");
            if (stream is null)
            {
                PluginLog.LogWarning($"Embedded resource not found while loading icon image: {filename}");
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
                PluginLog.LogError(ex, $"Failed to load icon image: {filename}, loading fallback");
                return this.Ui.LoadImageRaw(new byte[] { 255, 0, 0, 127 }, 1, 1, 4);
            }
        }
    }
}
