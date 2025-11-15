using AG.EnumLocalization;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using Sonar;
using Sonar.Localization;
using SonarPlugin.Config;
using SonarPlugin.GUI.Internal;
using SonarPlugin.Localization;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using SonarPlugin.Sounds;

namespace SonarPlugin.GUI
{
    /// <summary>Basic Sonar Widgets.</summary>
    public static class SonarWidgets
    {
        public static bool EnumCombo<T>(string label, ref T value, int max_height = -1, bool updateStrings = true, string? langCode = null) where T : struct, Enum
        {
            return EnumWidgetHelper<T>.Combo(label, ref value, max_height, updateStrings, langCode);
        }

        public static bool Localization(LocalizationConfig config, FileDialogManager fileDialogs)
        {
            return LocalizationWidgetHelper.Draw(config, fileDialogs);
        }

        public static bool SoundAlertsConfig(string id, SoundConfig config, SoundEngine engine, FileDialogManager fileDialogs)
        {
            return SoundsWidgetHelper.SoundSelector(id, config, engine, fileDialogs);
        }
    }
}
