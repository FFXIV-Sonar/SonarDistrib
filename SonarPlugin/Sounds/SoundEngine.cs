using AG.EnumLocalization;
using AG.EnumLocalization.Attributes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina;
using Lumina.Data.Files;
using Org.BouncyCastle.Security;
using SonarPlugin.GUI.Internal;
using SonarPlugin.Utility;
using SonarUtils.Text.Placeholders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SonarPlugin.Sounds
{
    [SingletonService]
    public sealed class SoundEngine
    {
        private static readonly Regex s_soundRegex = new(@"^sound:(?<id>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        private static readonly Regex s_resourceRegex = new(@"^res:(?<name>.*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        private static readonly Regex s_fileRegex = new(@"^file:(?<path>.*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        [ThreadStatic]
        private static Dictionary<string, string>? s_sharedDict;

        private GameData GameData { get; }
        private AudioPlaybackEngine AudioEngine { get; }
        private IPluginLog Logger { get; }
        public ImmutableArray<string> Sounds { get; }
        public ImmutableArray<string> SoundNames { get; }

        public SoundEngine(GameData gameData, AudioPlaybackEngine audioEngine, IPluginLog logger)
        {
            this.GameData = gameData;
            this.AudioEngine = audioEngine;
            this.Logger = logger;

            this.Sounds = [..this.Ctor_InitializeSounds(), ..Ctor_InitializeSoundResources()];
            this.SoundNames = [..this.Sounds.Select(GetSoundName)!];

            Debug.Assert(this.SoundNames.All(name => name is not null));
            for (var index = 0; index < this.Sounds.Length; index++)
            {
                this.Logger.Verbose(" - {sound} => {name}", this.Sounds[index], this.SoundNames[index]);
            }
        }

        private async Task DebugSounds()
        {
            await Task.Delay(15000);
            for (var index = 0; index < this.Sounds.Length; index++)
            {
                this.Logger.Debug("Playing {sound}: {name}", this.Sounds[index], this.SoundNames[index]);
                this.PlaySound(this.Sounds[index]);
                await Task.Delay(2000);
            }
        }

        private IEnumerable<string> Ctor_InitializeSounds()
        {
            var scdFile = this.GameData.GetFile<ScdFile>("sound/system/SE_UI.scd");
            if (scdFile is null)
            {
                this.Logger.Error("Unable to load game sound effects");
                yield break;
            }

            var count = scdFile.SoundDataCount;
            this.Logger.Debug("Found {count} game sound effects", count);
            for (var index = 0; index < count; index++)
            {
                var sound = scdFile.GetSound(index);

                // Ensure its not a looping sound
                if ((sound.SoundBasicDesc.Attribute & Lumina.Data.Parsing.Scd.SoundAttribute.Loop) != 0) continue;

                yield return $"sound:{index}";
            }
        }

        // TODO: For now assuming any file that doesn't have an extension is an .mp3 file since I'm logically changing the name in the project file
        //       This is probably not the BEST way of handling this but makes the logic of display and getting at the sound file easier for now.
        private static IEnumerable<string> Ctor_InitializeSoundResources()
            => typeof(SoundsWidgetHelper).Assembly.GetManifestResourceNames()
                .Where(resource => Path.GetExtension(resource) == string.Empty)
                .Select(resource => $"res:{resource}");

        public bool PlaySound(SoundConfig config)
        {
            if (!config.Enabled || string.IsNullOrWhiteSpace(config.Sound)) return true;
            return this.PlaySound(config.Sound);
        }

        public bool PlaySound(string sound)
        {
            if (TryParseSoundId(sound, out var id))
            {
                try
                {
                    UIGlobals.PlaySoundEffect(id);
                    return true;
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Failed to play game sound effect");
                }
            }
            else if (TryParseResourceName(sound, out var name))
            {
                try
                {
                    this.AudioEngine.PlaySound(name);
                    return true;
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Failed to play resource sound effect");
                }
            }
            else if (TryParseFilePath(sound, out var path))
            {
                try
                {
                    this.AudioEngine.PlaySound(path);
                    return true;
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Failed to play custom sound effect");
                }
            }
            return false;
        }

        [return: NotNullIfNotNull(nameof(sound))]
        public static string? GetSoundName(string? sound)
        {
            if (sound is null) return null;

            if (TryParseSoundId(sound, out var id))
            {
                var key = $"Sounds.Sound{id}";
                var result = EnumLoc.GetLocString(key);
                if (string.IsNullOrWhiteSpace(result) || result == key || result == $"Sound{id}")
                {
                    var dict = s_sharedDict ??= [];
                    dict["id"] = $"{id}";
                    return PlaceholderFormatter.Default.Format(SoundsLoc.SoundGeneric.GetLocString(), dict);
                }
                return result;
            }
            if (TryParseResourceName(sound, out var name))
            {
                if (name == "Enter Chat") return SoundsLoc.ResEnterChat.GetLocString();
                if (name == "Fanfare") return SoundsLoc.ResFanfare.GetLocString();
                if (name == "Feature Unlocked") return SoundsLoc.ResFeatureUnlocked.GetLocString();
                if (name == "Incoming Tell 1") return SoundsLoc.ResIncomingTell1.GetLocString();
                if (name == "Incoming Tell 2") return SoundsLoc.ResIncomingTell2.GetLocString();
                if (name == "Limit Break Charged") return SoundsLoc.ResLimitBreakCharged.GetLocString();
                if (name == "Limit Break Unlocked") return SoundsLoc.ResLimitBreakUnlocked.GetLocString();
                if (name == "Linkshell Transmission") return SoundsLoc.ResLinkshellTransmission.GetLocString();
                if (name == "Notification") return SoundsLoc.ResNotification.GetLocString();
                return name;
            }
            if (TryParseFilePath(sound, out var path)) return path;
            return sound; // Fallback
        }

        public static bool TryParseSoundId(string sound, out uint id)
        {
            var match = s_soundRegex.Match(sound);
            if (match.Success && uint.TryParse(match.Groups["id"].ValueSpan, out id)) return true;
            id = 0; return false;
        }

        public static bool TryParseResourceName(string sound, [NotNullWhen(true)] out string? name) => TryParseCore(sound, s_resourceRegex, "name", out name);

        public static bool TryParseFilePath(string sound, [NotNullWhen(true)] out string? path) => TryParseCore(sound, s_fileRegex, "path", out path);

        [return: NotNullIfNotNull(nameof(sound))]
        public static string? OldToNewConfig(string? sound)
        {
            if (sound is null) return null;

            // Known sounds - Migration
            if (sound is "Enter Chat" or "대화 창 입력") return "res:Enter Chat";
            if (sound is "Fanfare" or "팡파레") return "res:Fanfare";
            if (sound is "Feature Unlocked" or "도전 가능") return "res:Feature Unlocked";
            if (sound is "Incoming Tell 1" or "효과음 1") return "res:Incoming Tell 1";
            if (sound is "Incoming Tell 2" or "효과음 2") return "res:Incoming Tell 2";
            if (sound is "Limit Break Charged" or "리미트 브레이크 충전") return "res:Limit Break Charged";
            if (sound is "Limit Break Unlocked" or "리미트 브레이크 잠금해제") return "res:Limit Break Unlocked";
            if (sound is "Linkshell Transmission" or "링크셸 통신") return "res:Linkshell Transmission";
            if (sound is "Notification" or "알림음") return "res:Notification";

            // Custom sounds
            return $"file:{sound}";
        }

        private static bool TryParseCore(string sound, Regex regex, string groupId, [NotNullWhen(true)] out string? result)
        {
            var match = regex.Match(sound);
            if (match.Success)
            {
                result = match.Groups[groupId].Value;
                return true;
            }
            result = null;
            return false;
        }
    }
}
