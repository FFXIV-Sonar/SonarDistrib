using Sonar;
using Sonar.Enums;
using SonarPlugin.Utility;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace SonarPlugin.Config
{
    public sealed partial class LocalizationConfig
    {
        public record PresetConfig(SonarLanguage Db, string? Plugin, string? Dll);

        /// <summary>Language code of the bundled Simplified Chinese pack.</summary>
        public const string ChineseLanguageCode = "cn";

        /// <summary>Language code of the bundled Traditional Chinese pack.</summary>
        public const string TraditionalChineseLanguageCode = "tw";

        public static readonly FrozenDictionary<LocalizationPreset, PresetConfig> s_presets = new Dictionary<LocalizationPreset, PresetConfig>()
        {
            { LocalizationPreset.Default, new(SonarLanguage.Default, null, null) },
            { LocalizationPreset.Japanese, new(SonarLanguage.Japanese, null, null) },
            { LocalizationPreset.English, new(SonarLanguage.English, null, null) },
            { LocalizationPreset.German, new(SonarLanguage.German, null, null) },
            { LocalizationPreset.French, new(SonarLanguage.French, null, null) },
            { LocalizationPreset.Chinese, new(SonarLanguage.ChineseSimplified, FindChineseLanguageResource(typeof(SonarPluginIoC).Assembly), FindChineseLanguageResource(typeof(SonarClient).Assembly)) },
            { LocalizationPreset.ChineseTraditional, new(SonarLanguage.ChineseTraditional, FindTraditionalChineseLanguageResource(typeof(SonarPluginIoC).Assembly), FindTraditionalChineseLanguageResource(typeof(SonarClient).Assembly)) },
            { LocalizationPreset.Korean, new(SonarLanguage.Korean, null, null) },

        }.ToFrozenDictionary();

        /// <summary>Find the embedded Simplified Chinese language resource of the given assembly.</summary>
        public static string? FindChineseLanguageResource(Assembly assembly)
            => EnumLocUtils.GetLanguageResources(assembly).FirstOrDefault(name => name.Contains($".{ChineseLanguageCode}.", StringComparison.OrdinalIgnoreCase));

        /// <summary>Find the embedded Traditional Chinese language resource of the given assembly.</summary>
        public static string? FindTraditionalChineseLanguageResource(Assembly assembly)
            => EnumLocUtils.GetLanguageResources(assembly).FirstOrDefault(name => name.Contains($".{TraditionalChineseLanguageCode}.", StringComparison.OrdinalIgnoreCase));
    }
}
