using AG;
using AG.EnumLocalization;
using Sonar;
using Sonar.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SonarPlugin.Config
{
    public sealed partial class LocalizationConfig
    {
        public static readonly FrozenDictionary<LocalizationPreset, PresetConfig> s_presets = new Dictionary<LocalizationPreset, PresetConfig>()
        {
            { LocalizationPreset.Default, new(SonarLanguage.Default, null, null) },
            { LocalizationPreset.Japanese, new(SonarLanguage.Japanese, null, null) },
            { LocalizationPreset.English, new(SonarLanguage.English, null, null) },
            { LocalizationPreset.German, new(SonarLanguage.German, null, null) },
            { LocalizationPreset.French, new(SonarLanguage.French, null, null) },
            { LocalizationPreset.Chinese, new(SonarLanguage.ChineseSimplified, null, null) },
            { LocalizationPreset.Korean, new(SonarLanguage.Korean, null, null) },

        }.ToFrozenDictionary();

        private static readonly Regex s_resourceNameRegex = new(@"^.*\.lang\.json$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly ConcurrentDictionary<Assembly, ImmutableArray<string>> s_languages = new();

        private static void SetupCore(bool debug)
        {
            Span<Assembly> assemblies = [typeof(SonarPlugin).Assembly, typeof(SonarClient).Assembly];
            foreach (var assembly in assemblies)
            {
                try
                {
                    SetupAssemblyCore(assembly, debug);
                    var langCode = EnumLoc.GetDefaultLanguage(assembly);
                    if (langCode?.StartsWith("file:") is true) SetLanguageCore(assembly, langCode);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    GC.KeepAlive(ex);
                }
            }
        }

        private static void SetupAssemblyCore(Assembly assembly, bool debug)
        {
            EnumLoc.SetupAssembly(assembly, debug);
            foreach (var name in GetLanguageResources(assembly))
            {
                using var stream = assembly.GetManifestResourceStream(name);
                ThrowHelper.ThrowIf(stream is null, static () => new NullReferenceException("Opened stream was null."));
                LoadLanguageCore(assembly, name, stream);
            }
        }

        private static ImmutableArray<string> GetLanguageResources(Assembly assembly) => s_languages.GetOrAdd(assembly, GetLanguageResourcesCore);

        private static ImmutableArray<string> GetLanguageResourcesCore(Assembly assembly)
        {
            var result = new List<string>();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                var match = s_resourceNameRegex.Match(name);
                if (!match.Success) continue;
                result.Add(name);
            }
            return [.. result];
        }

        private static void SetLanguage(Assembly assembly, string? langCode)
        {
            EnumLoc.SetDefaultLanguage(langCode, assembly);
            if (langCode?.StartsWith("file:") is true)
            {
                try
                {
                    SetLanguageCore(assembly, langCode);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    GC.KeepAlive(ex);
                }   
            }
        }

        /// <summary>Precondition: <paramref name="name"/> starts with <c>file:</c></summary>
        private static void SetLanguageCore(Assembly assembly, string? name)
        {
            Debug.Assert(name?.StartsWith("file:") is true);
            try
            {
                var stream = File.OpenRead(name[5..]);
                ThrowHelper.ThrowIf(stream is null, () => new NullReferenceException("Opened stream was null."));
                LoadLanguageCore(assembly, name, stream);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                GC.KeepAlive(ex);
            }
        }

        private static void LoadLanguageCore(Assembly assembly, string name, Stream stream)
        {
            var data = JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(stream);
            ThrowHelper.ThrowIf(data is null, () => new NullReferenceException("Stream deserialization returned null."));
            EnumLoc.SetupLanguage(name, data, assembly);
        }
    }
}
