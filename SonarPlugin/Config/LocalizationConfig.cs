using AG.EnumLocalization;
using Sonar;
using Sonar.Data;
using Sonar.Enums;
using SonarPlugin.Utility;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SonarPlugin.Config
{
    /// <summary>Localization configuration.</summary>
    /// <remarks>This class is almost fully static as most properties rely on static sources.</remarks>
    public sealed partial class LocalizationConfig
    {
        private bool _debugFallbacks;
        
        /// <summary>Sonar language presets.</summary>
        // This property is computed and therefore best not serialized / deserialized
        // as this changes the other properties. Setting undefined does nothing.
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public LocalizationPreset Preset
        {
            get => this.GetPresetCore();
            set => this.SetPresetCore(value);
        }

        /// <summary>Sonar Database strings language.</summary>
        /// <remarks>Sourced from game data.</remarks>
        [SuppressMessage("Minor Code Smell", "S2325", Justification = "Intended.")]
        [SuppressMessage("Performance", "CA1822", Justification = "Intended.")]
        public SonarLanguage Db
        {
            get => Database.DefaultLanguage;
            set => Database.DefaultLanguage = value;
        }

        /// <summary>Plugin's language.</summary>
        [SuppressMessage("Minor Code Smell", "S2325", Justification = "Intended.")]
        [SuppressMessage("Performance", "CA1822", Justification = "Intended.")]
        public string? Plugin
        {
            get => EnumLoc.GetDefaultLanguage(typeof(SonarPlugin).Assembly);
            set => EnumLocUtils.SetLanguage(typeof(SonarPlugin).Assembly, value);
        }

        /// <summary>Sonar language.</summary>
        [SuppressMessage("Minor Code Smell", "S2325", Justification = "Intended.")]
        [SuppressMessage("Performance", "CA1822", Justification = "Intended.")]
        public string? Sonar
        {
            get => EnumLoc.GetDefaultLanguage(typeof(SonarClient).Assembly);
            set => EnumLocUtils.SetLanguage(typeof(SonarClient).Assembly, value);
        }

        /// <summary>Use debug fallbacks during <see cref="Setup"/>.</summary>
        /// <remarks>Debug fallbacks will use keys as fallbacks.</remarks>
        public bool DebugFallbacks
        {
            get => this._debugFallbacks;
            set
            {
                this._debugFallbacks = value;
                EnumLocUtils.Setup(value);
            }
        }

        /// <summary>Export .lang.json files minified (<c><see cref="System.Text.Json.JsonSerializerOptions.WriteIndented"/> = <see langword="false"/></c>).</summary>
        public bool Minified { get; set; }

        /// <summary>Get available plugin languages.</summary>
        /// <returns>Plugin languages.</returns>
        [SuppressMessage("Minor Code Smell", "S2325", Justification = "Intended.")]
        [SuppressMessage("Performance", "CA1822", Justification = "Intended.")]
        public ImmutableArray<string> GetAvailablePluginLanguages() => EnumLocUtils.GetLanguageResources(typeof(SonarPlugin).Assembly);

        /// <summary>Get available sonar Languages.</summary>
        /// <returns>Sonar languages.</returns>
        [SuppressMessage("Minor Code Smell", "S2325", Justification = "Intended.")]
        [SuppressMessage("Performance", "CA1822", Justification = "Intended.")]
        public ImmutableArray<string> GetAvailableSonarLanguages() => EnumLocUtils.GetLanguageResources(typeof(SonarClient).Assembly);

        private LocalizationPreset GetPresetCore()
        {
            foreach (var (preset, config) in s_presets)
            {
                if (this.Db == config.Db && this.Plugin == config.Plugin && this.Sonar == config.Dll) return preset;
            }
            return LocalizationPreset.Undefined;
        }

        private void SetPresetCore(LocalizationPreset preset)
        {
            if (!s_presets.TryGetValue(preset, out var config)) return;
            this.Db = config.Db; this.Plugin = config.Plugin; this.Sonar = config.Dll;
        }
    }
}
