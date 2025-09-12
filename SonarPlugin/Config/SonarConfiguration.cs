using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Sonar.Config;
using Sonar.Config.Experimental;
using Sonar.Data;
using Sonar.Enums;
using SonarPlugin.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SonarPlugin.Config
{
    [JsonObject]
    [SuppressMessage(null!, "S1104", Justification = "Intentional")]
    public class SonarConfiguration : IPluginConfiguration
    {
        // Sonar configuration version
        public const int SonarConfigurationVersion = 3;
        public int Version { get; set; } = SonarConfigurationVersion;

        // Sonar.NET configuration reference
        public SonarConfig SonarConfig = new();

        // General configurations
        public bool OverlayVisibleByDefault = false;
        public bool EnableLockedOverlays = false;
        public bool HideTitlebar = false;
        public bool WindowClickThrough = false;
        public bool TabBarClickThrough = false; //true
        public bool ListClickThrough = false; //true
        public bool EnableGameChatReports = true;
        public bool EnableGameChatReportsDeaths = true;
        public bool EnableGameChatItalicFont = false;
        public bool EnableGameChatCrossworldIcon = true;
        public XivChatType HuntOutputChannel = XivChatType.Echo;
        public float Opacity = 0.75f;
        public float SoundVolume = 0.5f;
        public PluginLanguage Language = PluginLanguage.English;
        public SuppressVerification SuppressVerification = SuppressVerification.None;
        public LocalizationConfig Localization = new();
        public SonarConfigurationColors Colors = new();

        public ClickAction MiddleClick = ClickAction.Chat;
        public ClickAction RightClick = ClickAction.Map;
        public ClickAction CtrlMiddleClick = ClickAction.None;
        public ClickAction CtrlRightClick = ClickAction.Remove;
        public ClickAction ShiftMiddleClick = ClickAction.None;
        public ClickAction ShiftRightClick = ClickAction.None;
        public ClickAction AltMiddleClick = ClickAction.None;
        public ClickAction AltRightClick = ClickAction.None;

        public CityState PreferredCityState = CityState.Limsa;

        private RelayStateSortingMode _sortingMode = RelayStateSortingMode.LastFound;
        public RelayStateSortingMode SortingMode
        {
            get => this._sortingMode != RelayStateSortingMode.Default ? this._sortingMode : RelayStateSortingMode.LastFound;
            set => this._sortingMode = value;
        }

        // Duty Instances
        public bool DisableChatInDuty = false;
        public bool DisableSoundInDuty = false;


        // Hunts configuration
        public bool AllSRankSettings = false;
        public bool AdvancedHuntReportSettings = false;
        public int DisplayHuntDeadTimer = 300;
        public int DisplayHuntUpdateTimer = 3600;
        public int DisplayHuntUpdateTimerOther = 300;
        public int HuntsDisplayLimit = 100;

        private NotifyMode _SSMinionReportingMode;
        public NotifyMode SSMinionReportingMode
        {
            get => this._SSMinionReportingMode != NotifyMode.Default ? this._SSMinionReportingMode : NotifyMode.Single;
            set => this._SSMinionReportingMode = value;
        }

        // Fates configuration
        public int DisplayFateUpdateTimer = 1800;
        public int DisplayFateDeadTimer = 300;
        public int FatesDisplayLimit = 100;

        // S Rank configurations
        public bool PlaySoundSRanks = false;
        public string? SoundFileSRanks = string.Empty;

        // A Rank configurations
        public bool PlaySoundARanks = false;
        public string? SoundFileARanks = string.Empty;

        // FATE configurations
        public HashSet<uint> SendFateToChat = new();
        public HashSet<uint> SendFateToSound = new();
        public bool PlaySoundFates = false;
        public string? SoundFileFates = string.Empty;
        public bool EnableFateChatReports = true;
        public bool EnableFateChatItalicFont = false;
        public bool EnableFateChatCrossworldIcon = true;
        public XivChatType FateOutputChannel = XivChatType.Echo;

        public void Sanitize()
        {
            this.SonarConfig ??= new();
            this.Colors ??= new();
            this.SoundVolume = Math.Clamp(this.SoundVolume, 0f, 1f);
            this.SoundFileARanks ??= string.Empty;
            this.SoundFileSRanks ??= string.Empty;
            this.SoundFileFates ??= string.Empty;

            this.DisplayHuntDeadTimer = MathFunctions.Clamp(this.DisplayHuntDeadTimer, 0, 604800);
            this.DisplayHuntUpdateTimer = MathFunctions.Clamp(this.DisplayHuntUpdateTimer, 60, 604800);
            this.DisplayHuntUpdateTimerOther = MathFunctions.Clamp(this.DisplayHuntUpdateTimerOther, 60, 604800);
            this.HuntsDisplayLimit = MathFunctions.Clamp(this.HuntsDisplayLimit, 1, 10000);
        }

        public bool PerformVersionUpdate(IPluginLog? logger = null)
        {
            if (this.Version == SonarConfigurationVersion) return false;

            // Make sure version is within a valid range.
            var clampedVersion = Math.Clamp(this.Version, 0, SonarConfigurationVersion);
            if (clampedVersion != this.Version)
            {
                logger?.Warning($"Clamping Sonar Configuration version from v{this.Version} to {clampedVersion}, unintended side effects may happen!");
                this.Version = clampedVersion;
            }

            // Perform version updates incrementally.
            if (this.Version is 0) this.PerformVersionUpdateCore0to1(logger);
            if (this.Version is 1) this.PerformVersionUpdateCore1to2(logger);
            if (this.Version is 2) this.PerformVersionUpdateCore2to3(logger);

            // Debug output
            if (this.Version is not SonarConfigurationVersion)
            {
                logger?.Warning($"Sonar configuration v{this.Version} is not v{SonarConfigurationVersion}");
                if (Debugger.IsAttached) Debugger.Break();
            }

            return true;
        }

        /// <summary>There are no version 0 configuration.</summary>
        private void PerformVersionUpdateCore0to1(IPluginLog? logger)
        {
            logger?.Info("Updating Sonar Configuration from v0 => v1: Should never happen");
            this.Version = 1;
        }

        /// <summary>Instance Jurisdiction was introduced, however this means that all jurisdiction values from there on is increased by 1...</summary>
        private void PerformVersionUpdateCore1to2(IPluginLog? logger)
        {
            var huntConfig = this.SonarConfig.HuntConfig;
            var fateConfig = this.SonarConfig.FateConfig;
            SonarJurisdiction jurisdiction;

            logger?.Info("Updating Sonar Configuration from v1 => v2: New Instance Jurisdiction added");
            foreach (var expansion in Enum.GetValues<ExpansionPack>())
            {
                // Hunt expansions and ranks jurisdictions
                foreach (var rank in Enum.GetValues<HuntRank>())
                {
                    jurisdiction = huntConfig.GetJurisdiction(expansion, rank);
                    if (jurisdiction >= SonarJurisdiction.Instance)
                    {
                        logger?.Debug($" - Hunts from {expansion} Rank {rank}: {jurisdiction} => {jurisdiction + 1}");
                        huntConfig.SetJurisdiction(expansion, rank, jurisdiction + 1);
                    }
                }

                // Hunt overrides (in case anyone went creative)
                foreach (var or in huntConfig.GetJurisdictionOverrides())
                {
                    jurisdiction = or.Value;
                    if (jurisdiction >= SonarJurisdiction.Instance)
                    {
                        var name = Database.Hunts.GetValueOrDefault(or.Key)?.Name.ToString() ?? $"Unknown (id: {or.Key})";
                        logger?.Debug($" - Hunt {name}: {jurisdiction} => {jurisdiction + 1}");
                        huntConfig.SetJurisdictionOverride(or.Key, jurisdiction + 1);
                    }
                }
            }

            jurisdiction = fateConfig.GetDefaultJurisdiction();
            if (jurisdiction >= SonarJurisdiction.Instance)
            {
                logger?.Debug($" - Fate Default Jurisdiction: {jurisdiction} => {jurisdiction + 1}");
                fateConfig.SetDefaultJurisdiction(jurisdiction + 1);
            }

            foreach (var fate in fateConfig.GetJurisdictions())
            {
                jurisdiction = fate.Value;
                if (jurisdiction >= SonarJurisdiction.Instance)
                {
                    var name = Database.Fates.GetValueOrDefault(fate.Key)?.Name.ToString() ?? $"Unknown (id: {fate.Key})";
                    logger?.Debug($" - Fate {name}: {jurisdiction} => {jurisdiction + 1}");
                    fateConfig.SetJurisdiction(fate.Key, jurisdiction + 1);
                }
            }

            this.Version = 2;
        }

        /// <summary>Turn off localization's debug fallbacks.</summary>
        private void PerformVersionUpdateCore2to3(IPluginLog? logger)
        {
            logger?.Info("Updating Sonar Configuration from v2 => v3: Turn off localization's debug fallbacks");

            this.Localization.DebugFallbacks = false;

            this.Version = 3;
        }
    }
}