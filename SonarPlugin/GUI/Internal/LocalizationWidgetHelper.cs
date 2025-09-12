using AG.EnumLocalization;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using Sonar;
using Sonar.Localization;
using SonarPlugin.Config;
using SonarPlugin.Localization;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using SonarPlugin.Utility;

namespace SonarPlugin.GUI.Internal
{
    internal static class LocalizationWidgetHelper
    {
        public const string LanguageExtension = ".json";
        public const string MetaLanguageKey = "__Meta.Language";
        public const string MetaAuthorKey = "__Meta.Author";
        public const string MetaNotesKey = "__Meta.Notes";

        public sealed record FileDialogResult(string Id, string Path, /* export only */ bool Fallbacks = false);
        private static FileDialogResult? s_importDialogResult;
        private static FileDialogResult? s_exportDialogResult;

        public static bool Draw(LocalizationConfig config, FileDialogManager fileDialogs)
        {
            var result = false;

            var preset = config.Preset;
            if (SonarWidgets.EnumCombo($"{PluginLoc.Presets.GetLocString()}###preset", ref preset, 100))
            {
                config.Preset = preset;
                result = true;
            }

            using (var node = ImRaii.TreeNode($"{PluginLoc.Advanced}###advanced"))
            {
                if (node.Success)
                {
                    var dbLanguage = config.Db;
                    if (SonarWidgets.EnumCombo($"{PluginLoc.Resources.GetLocString()}###resources", ref dbLanguage, 100))
                    {
                        config.Db = dbLanguage;
                        result = true;
                    }

                    ImGui.Spacing(); ImGui.Spacing(); ImGui.Spacing();

                    var plugin = config.Plugin;
                    if (LanguageSelector("plugin", PluginLoc.Plugin.GetLocString(), ref plugin, config.GetAvailablePluginLanguages(), fileDialogs, typeof(SonarPlugin).Assembly))
                    {
                        config.Plugin = plugin;
                        result = true;
                    }

                    ImGui.Spacing(); ImGui.Spacing(); ImGui.Spacing();

                    var sonar = config.Sonar;
                    if (LanguageSelector("sonar", SonarLoc.Sonar.GetLocString(), ref sonar, config.GetAvailableSonarLanguages(), fileDialogs, typeof(SonarClient).Assembly))
                    {
                        config.Sonar = sonar;
                        result = true;
                    }

                    ImGui.Spacing(); ImGui.Spacing(); ImGui.Spacing();

                    using (var debugNode = ImRaii.TreeNode($"{PluginLoc.Debug.GetLocString()}###debug"))
                    {
                        if (debugNode.Success)
                        {
                            var debug = config.DebugFallbacks;
                            var setup = false;
                            if (ImGui.Checkbox($"{PluginLocLoc.DebugFallbacks.GetLocString()}###debugFallbacks", ref debug))
                            {
                                config.DebugFallbacks = debug;
                                result = true;
                                setup = true;
                            }

                            if (ImGui.Button($"{PluginLoc.Setup.GetLocString()}###setup") || setup)
                            {
                                EnumLocUtils.Setup(config.DebugFallbacks);
                            }
                        }
                    }
                }
            }

            var export = ExportDestination();
            if (export is not null)
            {
                using var stream = File.Create(export.Path);
#pragma warning disable CA1869 // Justification = Configurable Write Indentation
                var jsonOptions = new JsonSerializerOptions() { WriteIndented = !config.Minified };
#pragma warning restore CA1869

                var assembly =
                    export.Id == "plugin" ? typeof(SonarPlugin).Assembly :
                    export.Id == "sonar" ? typeof(SonarClient).Assembly :
                    null; // ASSERT: Should never happen
                Debug.Assert(assembly is not null);

                var data = (export.Fallbacks ? EnumLoc.GetKeysAndFallbacks(assembly) : EnumLoc.GetKeysAndStrings(assembly: assembly))
                    .OrderBy(kvp => kvp.Key)
                    .ToDictionary();

                JsonSerializer.Serialize(stream, data, jsonOptions);
            }

            return result;
        }

        public static bool LanguageSelector(string id, string label, ref string? language, ImmutableArray<string> languages, FileDialogManager fileDialogs, Assembly assembly)
        {
            var result = false;

            if (s_importDialogResult?.Id == id)
            {
                language = $"file:{s_importDialogResult.Path}";
                s_importDialogResult = null;
                result = true;
            }

            var preview = 
                language is null ? PluginLoc.Fallbacks.GetLocString() :
                language.StartsWith("file:") ? language :
                EnumLoc.GetLocString(MetaLanguageKey, language, assembly);

            using (var combo = ImRaii.Combo($"{label}###{id}", preview))
            {
                if (combo.Success)
                {
                    if (ImGui.Selectable(PluginLoc.Fallbacks.GetLocString(), language is null))
                    {
                        language = null;
                        result = true;
                    }
                    else if (ImGui.IsItemHovered()) ShowTooltip(assembly);

                    foreach (var lang in languages)
                    {
                        if (ImGui.Selectable(EnumLoc.GetLocString(MetaLanguageKey, lang, assembly), language == lang))
                        {
                            language = lang;
                            result = true;
                        }
                        else if (ImGui.IsItemHovered()) ShowTooltip(lang, assembly);
                    }

                    if (language?.StartsWith("file:") is true)
                    {
                        if (ImGui.Selectable(language, true))
                        {
                            result = true;
                        }
                        else if (ImGui.IsItemHovered()) ShowTooltip(language, assembly);
                    }
                }
                else if (ImGui.IsItemHovered()) ShowTooltip(null, assembly);
            }

            if (ImGui.Button($"{PluginLoc.Import}###import-{id}"))
            {
                fileDialogs.OpenFileDialog($"{PluginLocLoc.ImportPrompt}###import-{id}", $"{LanguageExtension}{{{LanguageExtension}}}", (success, path) =>
                {
                    if (success) s_importDialogResult = new(id, path);
                });
            }

            ImGui.SameLine();

            if (ImGui.Button($"{PluginLoc.Export}###export-{id}"))
            {
                fileDialogs.SaveFileDialog($"{PluginLocLoc.ExportPrompt}###export-{id}", $"{LanguageExtension}{{{LanguageExtension}}}", $"{id}.lang.json", LanguageExtension, (success, path) =>
                {
                    if (success) s_exportDialogResult = new(id, path, false);
                });
            }

            ImGui.SameLine();

            if (ImGui.Button($"{PluginLoc.ExportFallbacks}###exportfallbacks-{id}"))
            {
                fileDialogs.SaveFileDialog($"{PluginLocLoc.ExportPrompt}###exportfallbacks-{id}", $"{LanguageExtension}{{{LanguageExtension}}}", $"{id}.lang.json", LanguageExtension, (success, path) =>
                {
                    if (success) s_exportDialogResult = new(id, path, true);
                });
            }

            return result;
        }

        private static FileDialogResult? ExportDestination()
        {
            var result = s_exportDialogResult;
            s_exportDialogResult = null;
            return result;
        }

        private static void ShowTooltip(string? langCode, Assembly assembly)
        {
            using var tooltip = ImRaii.Tooltip();
            if (tooltip.Success)
            {
                ImGui.TextUnformatted(EnumLoc.GetLocString(MetaLanguageKey, langCode, assembly));
                ImGui.Spacing();
                ImGui.TextUnformatted(EnumLoc.GetLocString(MetaAuthorKey, langCode, assembly));
                ImGui.Spacing();
                ImGui.TextUnformatted(EnumLoc.GetLocString(MetaNotesKey, langCode, assembly));
            }
        }

        private static void ShowTooltip(Assembly assembly)
        {
            using var tooltip = ImRaii.Tooltip();
            if (tooltip.Success)
            {
                ImGui.TextUnformatted(EnumLoc.GetLocFallback(MetaLanguageKey, assembly));
                ImGui.Spacing();
                ImGui.TextUnformatted(EnumLoc.GetLocFallback(MetaAuthorKey, assembly));
                ImGui.Spacing();
                ImGui.TextUnformatted(EnumLoc.GetLocFallback(MetaNotesKey, assembly));
            }
        }
    }
}
