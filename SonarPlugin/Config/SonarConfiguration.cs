using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Newtonsoft.Json;
using Sonar.Config;
using Sonar.Data;
using Sonar.Enums;
using SonarPlugin.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SonarPlugin.Config
{
    [JsonObject]
    [SuppressMessage(null!, "S1104", Justification = "Intentional")]
    public class SonarConfiguration : IPluginConfiguration
    {
        // Sonar configuration version
        public const int SonarConfigurationVersion = 2;
        public int Version { get; set; } = SonarConfigurationVersion;

        // Sonar.NET configuration reference
        public SonarConfig SonarConfig = new();
        public SonarConfigurationColors Colors = new();

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

        public void PerformVersionUpdate()
        {
            var huntConfig = this.SonarConfig.HuntConfig;
            var fateConfig = this.SonarConfig.FateConfig;
            SonarJurisdiction jurisdiction;

            // Instance Jurisdiction was introduced, however this means that all jurisdiction values from there on is increased by 1...
            if (this.Version == 1)
            {
                //PluginLog.LogInformation("Updating Sonar Configuration from v1 => v2: New Instance Jurisdiction added");
                foreach (var expansion in Enum.GetValues<ExpansionPack>())
                {
                    // Hunt expansions and ranks jurisdictions
                    foreach (var rank in Enum.GetValues<HuntRank>())
                    {
                        jurisdiction = huntConfig.GetJurisdiction(expansion, rank);
                        if (jurisdiction >= SonarJurisdiction.Instance)
                        {
                            //PluginLog.LogDebug($" - Hunts from {expansion} Rank {rank}: {jurisdiction} => {jurisdiction + 1}");
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
                            //PluginLog.LogDebug($" - Hunt {name}: {jurisdiction} => {jurisdiction + 1}");
                            huntConfig.SetJurisdictionOverride(or.Key, jurisdiction + 1);
                        }
                    }
                }

                jurisdiction = fateConfig.GetDefaultJurisdiction();
                if (jurisdiction >= SonarJurisdiction.Instance)
                {
                    //PluginLog.LogDebug($" - Fate Default Jurisdiction: {jurisdiction} => {jurisdiction + 1}");
                    fateConfig.SetDefaultJurisdiction(jurisdiction + 1);
                }

                foreach (var fate in fateConfig.GetJurisdictions())
                {
                    jurisdiction = fate.Value;
                    if (jurisdiction >= SonarJurisdiction.Instance)
                    {
                        var name = Database.Fates.GetValueOrDefault(fate.Key)?.Name.ToString() ?? $"Unknown (id: {fate.Key})";
                        //PluginLog.LogDebug($" - Fate {name}: {jurisdiction} => {jurisdiction + 1}");
                        fateConfig.SetJurisdiction(fate.Key, jurisdiction + 1);
                    }
                }

                this.Version = 2;
            }
        }
    }
}