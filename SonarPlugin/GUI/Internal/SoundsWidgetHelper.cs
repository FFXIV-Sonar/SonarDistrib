using AG.EnumLocalization;
using CheapLoc;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using DryIoc.FastExpressionCompiler.LightExpression;
using Lumina;
using Lumina.Data.Files;
using SonarPlugin.Sounds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkHistory.Delegates;
using static Lumina.Data.Files.ScdFile;

namespace SonarPlugin.GUI.Internal
{
    internal static class SoundsWidgetHelper
    {
        private const int HistorySize = 10;
        private static readonly Regex s_prefixRegex = new(@"^(?<prefix>[A-Za-z]*?):.*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        private static readonly Queue<string> s_history = [];

        private static FileDialogResult? s_openFileResult;
        
        public static bool SoundSelector(string id, SoundConfig config, SoundEngine engine, FileDialogManager fileDialogs)
        {
            var result = false;

            if (s_openFileResult?.Id == id)
            {
                var sound = $"file:{s_openFileResult.Path}";
                var previous = config.Sound ?? string.Empty;
                s_openFileResult = null;

                // Add previous selection to history
                AddToHistory(previous);

                // Set as selected file configuration
                config.Sound = sound;
                engine.PlaySound(sound);

                // Add new selection to history
                AddToHistory(sound);

                // A configuration change occurred, set result to true.
                result = true;
            }

            using var innerId = ImRaii.PushId(id);

            var enabled = config.Enabled;
            if (ImGui.Checkbox($"{SoundsLoc.PlaySound.GetLocString()}###enable", ref enabled))
            {
                result = true;
                config.Enabled = enabled;
            }
            if (!enabled) return result;

            var current = config.Sound ?? string.Empty;

            using (var combo = ImRaii.Combo($"{SoundsLoc.Sounds.GetLocString()}###sounds", SoundEngine.GetSoundName(current), ImGuiComboFlags.HeightLarge))
            {
                if (combo.Success)
                {
                    // Raw enumerator usage is a trickery here, the underlying
                    // list is nicely ordered with ingame sounds first then
                    // the resource based sounds.

                    // The custom part is then generated from s_history which
                    // contains up to HistorySize user selected files saved
                    // for the current session (in memory only).

                    var enumerator = engine.Sounds.GetEnumerator();
                    var currentPrefix = GetPrefix(current);

                    // Predefined selection
                    var more = enumerator.MoveNext();
                    while (more) // Outher while: Categories
                    {
                        var prefix = GetPrefix(enumerator.Current);
                        using var heading = PrefixHeading(prefix, currentPrefix);
                        using var indent = ImRaii.PushIndent();
                        if (heading.Success)
                        {
                            while (more && prefix == GetPrefix(enumerator.Current)) // Inner while: Entries
                            {
                                var sound = enumerator.Current;
                                var name = SoundEngine.GetSoundName(sound);
                                if (ImGui.Selectable(name, current == sound))
                                {
                                    config.Sound = sound;
                                    engine.PlaySound(config);
                                    result = true;
                                }
                                more = enumerator.MoveNext();
                            }
                        }
                        else
                        {
                            // Lets skip this prefix
                            while ((more = enumerator.MoveNext()) && prefix == GetPrefix(enumerator.Current)) { /* Empty */ }
                        }
                    }

                    // Custom selection
                    using (var customHeading = PrefixHeading("file", currentPrefix))
                    {
                        if (customHeading.Success)
                        {
                            using var indent = ImRaii.PushIndent();

                            // History
                            foreach (var sound in s_history)
                            {
                                var name = SoundEngine.GetSoundName(sound);
                                if (ImGui.Selectable(name, current == sound))
                                {
                                    config.Sound = sound;
                                    engine.PlaySound(config);
                                    result = true;
                                }
                            }

                            // Not in history
                            if (currentPrefix is "file" && !s_history.Contains(current))
                            {
                                var name = SoundEngine.GetSoundName(current);

                                // NOTE: This item would always be selected if shown.
                                if (ImGui.Selectable(name, true))
                                {
                                    // This is technically a no-op, however they're here
                                    // for pattern consistency purposes.
                                    config.Sound = current;
                                    engine.PlaySound(current);
                                    result = true;

                                    // Add to history in case of intent
                                    AddToHistory(current);
                                }
                            }

                            // Select file
                            if (ImGui.Selectable(SoundsLoc.SelectFile.GetLocString()))
                            {
                                fileDialogs.OpenFileDialog(SoundsLoc.SelectFilePrompt.GetLocString(), $"{SoundsLoc.SelectFilePromptSoundFiles.GetLocString()}{{.3g2,.3gp,.3gp2,.3gpp,.asf,.wma,.wmv,.aac,.adts,.avi,.mp3,.m4a,.m4v,.mov,.mp4,.sami,.smi,.wav}}", (success, filename) =>
                                {
                                    if (!success) return;
                                    s_openFileResult = new(id, filename);
                                });
                            }
                        }
                    }
                }
            }
            
            using (var font = ImRaii.PushFont(UiBuilder.IconFont))
            {
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.Play))) engine.PlaySound(config.Sound ?? string.Empty);
            }
            return result;
        }

        private static string GetPrefix(string sound)
        {
            var match = s_prefixRegex.Match(sound);
            return match.Success ? match.Groups["prefix"].Value : string.Empty;
        }

        private static ImRaii.IEndObject PrefixHeading(string prefix, string current)
        {
            var id = $"sounds_{prefix}";
            var name = prefix switch
            {
                "sound" => SoundsLoc.GameSound.GetLocString(),
                "res" => SoundsLoc.Resource.GetLocString(),
                "file" => SoundsLoc.Custom.GetLocString(),
                _ => $"{SoundsLoc.Unknown.GetLocString()}: {prefix}",
            };
            var tooltip = prefix switch
            {
                "sound" => SoundsLoc.GameSoundTooltip.GetLocString(),
                "res" => SoundsLoc.ResourceTooltip.GetLocString(),
                "file" => SoundsLoc.CustomTooltip.GetLocString(),
                _ => SoundsLoc.UnknownTooltip.GetLocString(),
            };

            var flags = ImGuiTreeNodeFlags.None;
            if (prefix == current) flags |= ImGuiTreeNodeFlags.DefaultOpen;

            var node = ImRaii.TreeNode($"{name}###{id}", flags);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip);

            return node;
        }

        private static void AddToHistory(string sound)
        {
            if (GetPrefix(sound) is not "file" || s_history.Contains(sound)) return;
            while (s_history.Count >= HistorySize) s_history.Dequeue();
            s_history.Enqueue(sound);
        }
    }
}
