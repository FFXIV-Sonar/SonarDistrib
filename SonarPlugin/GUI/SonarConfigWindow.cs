using AG.EnumLocalization;
using CheapLoc;
using Dalamud.Data;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using Sonar;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Logging;
using Sonar.Relays;
using Sonar.Utilities;
using SonarPlugin.Attributes;
using SonarPlugin.Config;
using SonarPlugin.GUI.Internal;
using SonarPlugin.Localization;
using SonarPlugin.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static SonarPlugin.Utility.ShellUtils;
using Dalamud.Interface;
using SonarPlugin.Sounds;
using Dalamud.Plugin.VersionInfo;

namespace SonarPlugin.GUI
{
    [SingletonService]
    public sealed class SonarConfigWindow : Window, IDisposable
    {
        private Task _debugHuntTask = Task.CompletedTask;
        private Task _debugFateTask = Task.CompletedTask;
        public bool IsVisible { get; set; }

        private bool fatesNeedSorting = true;

        private SonarPlugin Plugin { get; }
        private SonarPluginStub Stub { get; }
        private IDalamudPluginInterface PluginInterface { get; }
        private SonarClient Client { get; }
        private IDataManager Data { get; }
        private IDalamudVersionInfo DalamudVersion { get; }
        private SoundEngine Sounds { get; }
        private FileDialogManager FileDialogs { get; }
        private IndexProvider Index { get; }
        private IPluginLog Logger { get; }

        private AudioPlaybackEngine Audio { get; }

        private readonly Vector2 tableOuterSize = new(0.0f, ImGui.GetTextLineHeightWithSpacing() * 30);

        private readonly Tasker _tasker = new();

        private readonly string[] _chatTypes;
        private string fateSearchText = string.Empty;

        private List<FateRow> filteredFateData;
        private readonly Dictionary<uint, string> _fateZonesCache = new();
        private readonly int fateTableColumnCount = Enum.GetNames(typeof(FateSelectionColumns)).Length;

        public SonarConfigWindow(SonarPlugin plugin, SonarPluginStub stub, IDalamudPluginInterface pluginInterface, SonarClient client, IDataManager data, IDalamudVersionInfo dalamudVersion, AudioPlaybackEngine audio, SoundEngine sounds, FileDialogManager fileDialogs, IndexProvider index, IPluginLog logger) : base("Sonar Configuration")
        {
            this.Plugin = plugin;
            this.Stub = stub;
            this.PluginInterface = pluginInterface;
            this.Client = client;
            this.Data = data;
            this.DalamudVersion = dalamudVersion;
            this.Sounds = sounds;
            this.Audio = audio;
            this.FileDialogs = fileDialogs;
            this.Index = index;
            this.Logger = logger;

            this.Plugin.Windows.AddWindow(this);

            this.SizeConstraints = new()
            {
                MinimumSize = new(300, 100),
                MaximumSize = new(float.MaxValue, float.MaxValue)
            };
            this.Size = new(300, 500);
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.BgAlpha = this.Plugin.Configuration.Opacity;

            // Chat Types for determining which chat log to send text messages into the chat window
            this._chatTypes = Enum.GetValues<XivChatType>()
                .Select(t => t.GetDetails()?.FancyName ?? t.ToString())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToArray()!;

            // Set initial fate information
            this.filteredFateData = Database.Fates.Values
                .DistinctBy(fate => fate.GroupId)
                .ToList();

            this.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfig;
        }

        private void OpenConfig()
        {
            this.Toggle();
        }

        private bool _save;
        private bool _server;
        public override void PreDraw()
        {
            this.WindowName = $"{ConfigWindowLoc.WindowTitle.GetLocString()}###SonarConfigWindow";
        }

        public override void Draw()
        {
            using var bar = ImRaii.TabBar("###tabBar");
            if (!bar.Success) return;

            using (var tab = ImRaii.TabItem($"{ConfigWindowLoc.GeneralTab.GetLocString()}###generalTab"))
            {
                if (tab.Success) this.DrawGeneralTab();
            }

            using (var tab = ImRaii.TabItem($"{ConfigWindowLoc.HuntsTab.GetLocString()}###huntsTab"))
            {
                if (tab.Success) this.DrawHuntTab();
            }

            using (var tab = ImRaii.TabItem($"{ConfigWindowLoc.FatesTab.GetLocString()}###fatesTab"))
            {
                if (tab.Success) this.DrawFateTab();
            }

            using (var tab = ImRaii.TabItem($"{ConfigWindowLoc.AboutTab.GetLocString()}###aboutTab"))
            {
                if (tab.Success) this.DrawAboutTab();
            }

            using (var tab = ImRaii.TabItem($"{ConfigWindowLoc.DebugTab.GetLocString()}###debugTab"))
            {
                if (tab.Success) this.DrawDebugTab();
            }

            if (this._save) this.Plugin.SaveConfiguration(this._server);
            this._save = this._server = false;
        }

        private void DrawGeneralTab()
        {
            using var child = ImRaii.Child("###generalTabScrollRegion");
            if (!child.Success) return;

            SonarImGui.Checkbox($"{ConfigLoc.GlobalContribute.GetLocString()}###globalContribute", this.Client.Configuration.Contribute.Global, value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute.Global = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(ConfigLoc.GlobalContributeTooltip.GetLocString());
            }

            this._save |= ImGui.Checkbox($"{ConfigWindowLoc.OverlaysVisibleByDefault.GetLocString()}###overlayVisibleByDefault", ref this.Plugin.Configuration.OverlayVisibleByDefault);
            this._save |= ImGui.Checkbox($"{ConfigWindowLoc.LockOverlays.GetLocString()}###lockOverlays", ref this.Plugin.Configuration.EnableLockedOverlays);
            this._save |= ImGui.Checkbox($"{ConfigWindowLoc.HideTitleBar.GetLocString()}###hideTitlebar", ref this.Plugin.Configuration.HideTitlebar);
            this._save |= ImGui.Checkbox($"{ConfigWindowLoc.EnableClickthrough.GetLocString()}###enableWindowClickthrough", ref this.Plugin.Configuration.WindowClickThrough);
            if (this.Plugin.Configuration.WindowClickThrough)
            {
                using var indent = ImRaii.PushIndent();
                this._save |= ImGui.Checkbox($"{ConfigWindowLoc.TabsClickthrough.GetLocString()}###enableTabBarClickthrough", ref this.Plugin.Configuration.TabBarClickThrough);
                this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ListClickthrough.GetLocString()}###enableListClickthrough", ref this.Plugin.Configuration.ListClickThrough);
            }

            var sortingMode = this.Plugin.Configuration.SortingMode;
            if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.SortingMode.GetLocString()}###sortingMode", ref sortingMode, 100))
            {
                this._save = true;
                if (sortingMode is RelayStateSortingMode.Default) sortingMode = RelayStateSortingMode.LastFound;
                this.Plugin.Configuration.SortingMode = sortingMode;
            }

            var jurisdiction = this.Client.Configuration.Contribute.ReceiveJurisdiction;
            if (SonarWidgets.EnumCombo($"{ConfigLoc.ReceiveJurisdiction.GetLocString()}###receiveJurisdiction", ref jurisdiction, 100))
            {
                this._save = this._server = true;
                if (jurisdiction is SonarJurisdiction.Default) jurisdiction = SonarJurisdiction.Datacenter;
                this.Client.Configuration.Contribute.ReceiveJurisdiction = jurisdiction;
            }

            if (ImGui.SliderFloat($"{ConfigWindowLoc.Opacity.GetLocString()}###opacitySlider", ref this.Plugin.Configuration.Opacity, 0.0f, 1.0f))
            {
                this._save = true;
                this.BgAlpha = this.Plugin.Configuration.Opacity;
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.SliderFloat($"{ConfigWindowLoc.AlertVolume.GetLocString()}###volumeSlider", ref this.Plugin.Configuration.SoundVolume, 0.0f, 1.0f))
            {
                this.Audio.Volume = this.Plugin.Configuration.SoundVolume;
                this._save = true;
            }

            // TODO: Add language combo when we add resource files and change text

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.DutySettingsHeader.GetLocString()}###dutySettingsHeader", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();
                    this._save |= ImGui.Checkbox($"{ConfigWindowLoc.DutyDisableChatAlerts.GetLocString()}###disableChatAlerts", ref this.Plugin.Configuration.DisableChatInDuty);
                    this._save |= ImGui.Checkbox($"{ConfigWindowLoc.DutyDisableSoundAlerts.GetLocString()}###disableSoundAlerts", ref this.Plugin.Configuration.DisableSoundInDuty);
                }
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.ClickSettingsHeader.GetLocString()}###clickSettingsHeader", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();
                    ClickAction action;

                    action = this.Plugin.Configuration.MiddleClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.MiddleClick.GetLocString()}###.MiddleClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.Chat;
                        this.Plugin.Configuration.MiddleClick = action;
                        this._save = true;
                    }

                    action = this.Plugin.Configuration.RightClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.RightClick.GetLocString()}###.RightClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.Map;
                        this.Plugin.Configuration.RightClick = action;
                        this._save = true;
                    }

                    // ==

                    action = this.Plugin.Configuration.ShiftMiddleClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.ShiftMiddleClick.GetLocString()}###.ShiftMiddleClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.None;
                        this.Plugin.Configuration.ShiftMiddleClick = action;
                        this._save = true;
                    }

                    action = this.Plugin.Configuration.ShiftRightClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.ShiftRightClick.GetLocString()}###.ShiftRightClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.None;
                        this.Plugin.Configuration.ShiftRightClick = action;
                        this._save = true;
                    }

                    // ==

                    action = this.Plugin.Configuration.AltMiddleClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.AltMiddleClick.GetLocString()}###.AltMiddleClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.None;
                        this.Plugin.Configuration.AltMiddleClick = action;
                        this._save = true;
                    }

                    action = this.Plugin.Configuration.AltRightClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.AltRightClick.GetLocString()}###.AltRightClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.None;
                        this.Plugin.Configuration.AltRightClick = action;
                        this._save = true;
                    }

                    // ==

                    action = this.Plugin.Configuration.CtrlMiddleClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.CtrlMiddleClick.GetLocString()}###.CtrlMiddleClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.None;
                        this.Plugin.Configuration.CtrlMiddleClick = action;
                        this._save = true;
                    }

                    action = this.Plugin.Configuration.CtrlRightClick;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.CtrlRightClick.GetLocString()}###.CtrlRightClick", ref action, 100))
                    {
                        if (action is ClickAction.Default) action = ClickAction.Teleport;
                        this.Plugin.Configuration.CtrlRightClick = action;
                        this._save = true;
                    }

                    ImGui.Spacing();

                    var cityStateValues = Enum.GetValues<CityState>().Select(cityState => (cityState, cityState.GetMeta())).ToArray();
                    var cityStateStrings = cityStateValues.Select(ct => ct.Item2?.GetZone()?.ToString() ?? string.Empty).ToArray();
                    SonarImGui.Combo($"{ConfigWindowLoc.PreferredCityState.GetLocString()}###preferredCityState", (int)this.Plugin.Configuration.PreferredCityState, cityStateStrings, index =>
                    {
                        this.Plugin.Configuration.PreferredCityState = cityStateValues[index].cityState;
                        this._save = true;
                    });
                }
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.LocalizationHeader.GetLocString()}###localizationHeader", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();
                    using (var color = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudYellow))
                    {
                        ImGui.TextUnformatted("Localization is still in development!");
                        ImGui.TextUnformatted("- Lots of strings are still not covered");
                        ImGui.TextUnformatted("- Strings may change between releases");
                        ImGui.TextUnformatted("Use localization at your own risk!");
                    }
                    this._save |= SonarWidgets.Localization(this.Plugin.Configuration.Localization, this.FileDialogs);
                }
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.LodestoneHeader.GetLocString()}###lodestoneHeader", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();
                    var suppressMode = this.Plugin.Configuration.SuppressVerification;
                    if (SonarWidgets.EnumCombo($"{ConfigWindowLoc.SuppressLodestoneVerifications.GetLocString()}###suppressVerification", ref suppressMode, 100))
                    {
                        this.Plugin.Configuration.SuppressVerification = suppressMode;
                        this._save = true;
                    }
                }
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.ColorSettingsHeader.GetLocString()}###colorHeader", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.HealthyHuntColor.GetLocString()}###healthyHuntColor", ref this.Plugin.Configuration.Colors.HuntHealthy, ImGuiColorEditFlags.NoInputs);
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.PulledHuntColor.GetLocString()}###pulledHuntColor", ref this.Plugin.Configuration.Colors.HuntPulled, ImGuiColorEditFlags.NoInputs);
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.DeadHuntColor.GetLocString()}###deadHuntColor", ref this.Plugin.Configuration.Colors.HuntDead, ImGuiColorEditFlags.NoInputs);

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.RunningFateColor.GetLocString()}###runningFateColor", ref this.Plugin.Configuration.Colors.FateRunning, ImGuiColorEditFlags.NoInputs);
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.ProgressFateColor.GetLocString()}###progressFateColor", ref this.Plugin.Configuration.Colors.FateProgress, ImGuiColorEditFlags.NoInputs);
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.CompleteFateColor.GetLocString()}###completeFateColor", ref this.Plugin.Configuration.Colors.FateComplete, ImGuiColorEditFlags.NoInputs);
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.FailedFateColor.GetLocString()}###failedFateColor", ref this.Plugin.Configuration.Colors.FateFailed, ImGuiColorEditFlags.NoInputs);
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.PreparingFateColor.GetLocString()}###preparingFateColor", ref this.Plugin.Configuration.Colors.FatePreparation, ImGuiColorEditFlags.NoInputs);
                    this._save |= ImGui.ColorEdit4($"{ConfigWindowLoc.UnknownFateColor.GetLocString()}###unknownFateColor", ref this.Plugin.Configuration.Colors.FateUnknown, ImGuiColorEditFlags.NoInputs);

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.TextUnformatted(ConfigWindowLoc.ColorPresets.GetLocString());

                    if (ImGui.Button(ConfigWindowLoc.DefaultColorPreset.GetLocString()))
                    {
                        this.Plugin.Configuration.Colors.SetDefaults();
                        this._save = true;
                    }

                    ImGui.SameLine();

                    if (ImGui.Button(ConfigWindowLoc.OriginalColorPreset.GetLocString()))
                    {
                        this.Plugin.Configuration.Colors.HuntHealthy = ColorPalette.Green;
                        this.Plugin.Configuration.Colors.HuntPulled = ColorPalette.Yellow;
                        this.Plugin.Configuration.Colors.HuntDead = ColorPalette.Red;

                        this.Plugin.Configuration.Colors.FateRunning = ColorPalette.Green;
                        this.Plugin.Configuration.Colors.FateProgress = ColorPalette.Yellow;
                        this.Plugin.Configuration.Colors.FateComplete = ColorPalette.Red;
                        this.Plugin.Configuration.Colors.FateFailed = ColorPalette.Red;
                        this.Plugin.Configuration.Colors.FatePreparation = ColorPalette.White;
                        this.Plugin.Configuration.Colors.FateUnknown = ColorPalette.Red;
                        this._save = true;
                    }
                }
            }
        }

        public override void OnClose()
        {
            this.Plugin.SaveConfiguration(true);
        }

        private readonly Dictionary<HuntRank, string> rankStrings = new()
        {
            { HuntRank.None, $"{Loc.Localize("NoRankText", "None")}" },
            { HuntRank.B, $"{Loc.Localize("RankBText", "Rank B")}" },
            { HuntRank.A, $"{Loc.Localize("RankAText", "Rank A")}" },
            { HuntRank.S, $"{Loc.Localize("RankSText", "Rank S")}" },
            { HuntRank.SSMinion, $"{Loc.Localize("RankSSMinionText", "Rank SS Minions")}" },
            { HuntRank.SS, $"{Loc.Localize("RankSSText", "Rank SS")}" },
        };

        private void DrawHuntTab()
        {
            using var child = ImRaii.Child("##huntTabScrollRegion");

            SonarImGui.Checkbox($"{ConfigLoc.ContributeHunts.GetLocString()}###contributeHunts", this.Client.Configuration.Contribute[RelayType.Hunt], value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute[RelayType.Hunt] = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(ConfigLoc.ContributeTooltip.GetLocString());
            }

            if (!this.Client.Configuration.Contribute.Global)
            {
                ImGui.SameLine();
                using var color = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.TextUnformatted(ConfigLoc.ContributeGlobalDisabled.GetLocString());
            }

            SonarImGui.Checkbox($"{ConfigLoc.TrackAllHunts.GetLocString()}###trackAllHunts", this.Client.Configuration.HuntConfig.TrackAll, value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.HuntConfig.TrackAll = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(ConfigLoc.TrackAllTooltip.GetLocString());
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.ChatReportsConfig.GetLocString()}###huntChatConfig", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();
                    this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ChatReportsEnabled.GetLocString()}###huntChatEnabled", ref this.Plugin.Configuration.EnableGameChatReports);
                    if (this.Plugin.Configuration.EnableGameChatReports)
                    {
                        using var indent2 = ImRaii.PushIndent();
                        // TODO: might need to do extra checks here and default to Echo channel on failure.
                        // TODO: Chat types localization
                        var currentChat = XivChatTypeExtensions.GetDetails(this.Plugin.Configuration.HuntOutputChannel)?.FancyName ?? this.Plugin.Configuration.HuntOutputChannel.ToString();
                        var selectedChat = Array.IndexOf(this._chatTypes, currentChat);

                        if (ImGui.Combo("###chatTypes", ref selectedChat, this._chatTypes, this._chatTypes.Length))
                        {
                            this._save = true;
                            var value = XivChatTypeUtils.GetValueFromInfoAttribute(this._chatTypes[selectedChat]);
                            this.Plugin.Configuration.HuntOutputChannel = value;
                        }

                        this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ChatEnableItalics.GetLocString()}###huntChatEnableItalic", ref this.Plugin.Configuration.EnableGameChatItalicFont);
                        this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ChatEnableCwIcon.GetLocString()}###huntChatEnableCrossworldIcon", ref this.Plugin.Configuration.EnableGameChatCrossworldIcon);
                        this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ChatEnableDeaths.GetLocString()}###huntChatEnableDeaths", ref this.Plugin.Configuration.EnableGameChatReportsDeaths);
                    }
                }
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.SoundAlertsConfig.GetLocString()}###huntAlertsConfig", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();

                    using (var node2 = ImRaii.TreeNode($"{ConfigWindowLoc.HuntSRankSounds.GetLocString()}###huntSRankSounds"))
                    {
                        if (node2.Success)
                        {
                            using var indent2 = ImRaii.PushIndent();
                            this._save |= SonarWidgets.SoundAlertsConfig("huntSRankSoundsConfig", this.Plugin.Configuration.SoundSRanksConfig, this.Sounds, this.FileDialogs);
                        }
                    }

                    using (var node2 = ImRaii.TreeNode($"{ConfigWindowLoc.HuntARankSounds.GetLocString()}###huntARankSounds"))
                    {
                        if (node2.Success)
                        {
                            using var indent2 = ImRaii.PushIndent();
                            this._save |= SonarWidgets.SoundAlertsConfig("huntARankSoundsConfig", this.Plugin.Configuration.SoundFileARanksConfig, this.Sounds, this.FileDialogs);
                        }
                    }

                    using (var node2 = ImRaii.TreeNode($"{ConfigWindowLoc.HuntBRankSounds.GetLocString()}###huntBRankSounds"))
                    {
                        if (node2.Success)
                        {
                            using var indent2 = ImRaii.PushIndent();
                            this._save |= SonarWidgets.SoundAlertsConfig("huntBRankSoundsConfig", this.Plugin.Configuration.SoundFileBRanksConfig, this.Sounds, this.FileDialogs);
                        }
                    }
                }
            }
            
            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##huntTabReportNotifications", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("HuntNotifications", "Hunt Report Notifications")}"))
            {
                ImGui.Indent();
                SonarImGui.Checkbox($"{Loc.Localize("AllSRankSettings", "Separate Rank SS and Minions from Rank S")}##allsranks", this.Plugin.Configuration.AllSRankSettings, value =>
                {
                    this._save = this._server = true;
                    this.Plugin.Configuration.AllSRankSettings = value;
                    if (!value)
                    {
                        foreach (ExpansionPack expansion in Enum.GetValues(typeof(ExpansionPack)))
                        {
                            SonarJurisdiction jurisdiction = this.Client.Configuration.HuntConfig.GetJurisdiction(expansion, HuntRank.S);
                            foreach (HuntRank rank in new HuntRank[] { HuntRank.SS, HuntRank.SSMinion })
                            {
                                this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, rank, jurisdiction);
                            }
                        }
                    }
                });

                SonarImGui.Checkbox($"{Loc.Localize("AdvancedHuntReportingSettings", "Granular Hunt Reporting Settings")}##advanced", this.Plugin.Configuration.AdvancedHuntReportSettings, value =>
                {
                    this._save = this._server = true;
                    this.Plugin.Configuration.AdvancedHuntReportSettings = value;
                    if (!value)
                    {
                        foreach (HuntRank rank in Enum.GetValues<HuntRank>())
                        {
                            SonarJurisdiction jurisdiction = this.Client.Configuration.HuntConfig.GetJurisdiction(ExpansionPack.ARealmReborn, rank);
                            foreach (ExpansionPack expansion in Enum.GetValues(typeof(ExpansionPack)))
                            {
                                this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, rank, jurisdiction);
                            }
                        }
                    }
                });

                ImGui.Spacing();

                var ranks = Enum.GetValues<HuntRank>();
                foreach (var rank in ranks.AsEnumerable().Reverse())
                {
                    if (rank == HuntRank.None || ((int)rank & 0x80) == 0x80) continue;
                    if (!this.Plugin.Configuration.AllSRankSettings && (rank == HuntRank.SS || rank == HuntRank.SSMinion)) continue;
                    if (!this.Plugin.Configuration.AdvancedHuntReportSettings)
                    {
                        this.DrawHuntTabRankBasic(rank);
                    }
                    else
                    {
                        this.DrawHuntTabRankAdvanced(rank);
                    }
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                SonarImGui.Combo($"{Loc.Localize("SSMinionNotificationMode", "SS Minion Notifications Mode")}", (int)this.Plugin.Configuration.SSMinionReportingMode, EnumCheapLocExtensions.CheapLoc<NotifyMode>().Values.ToArray(), value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.SSMinionReportingMode = (NotifyMode)value;
                });
                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##huntTabDisplayTimers", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("HuntDisplayTimers", "Hunt Display Timers")}"))
            {
                ImGui.Indent();

                ImGui.TextDisabled(Loc.Localize("DisplayTimersHelpText", "All timers are in seconds. CTRL+Click to input directly."));
                ImGui.Spacing();

                SonarImGui.SliderInt($"{Loc.Localize("HuntDeadTimer", "Time since death (S and A)")}###huntDeadTimer", this.Plugin.Configuration.DisplayHuntDeadTimer, 0, 604800, value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.DisplayHuntDeadTimer = value;
                });

                SonarImGui.SliderInt($"{Loc.Localize("HuntUpdateTimer", "Time since last update (S and A)")}###huntUpdateTimer", this.Plugin.Configuration.DisplayHuntUpdateTimer, 60, 604800, value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.DisplayHuntUpdateTimer = value;
                });

                SonarImGui.SliderInt($"{Loc.Localize("HuntUpdateOtherTimer", "Time since last update (B)")}###huntUpdateOtherTimer", this.Plugin.Configuration.DisplayHuntUpdateTimerOther, 60, 604800, value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.DisplayHuntUpdateTimerOther = value;
                });

                SonarImGui.SliderInt($"{Loc.Localize("HuntsDisplayLimit", "Hunts Display Limit")}###huntDisplayLimit", this.Plugin.Configuration.HuntsDisplayLimit, 1, 10000, value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.HuntsDisplayLimit = value;
                });

                ImGui.Unindent();
            }
        }

        private readonly Dictionary<ExpansionPack, string> expansionStrings = new Dictionary<ExpansionPack, string>()
        {
            { ExpansionPack.Unknown , $"{Loc.Localize("UnknownText", "Unknown")}" },
            { ExpansionPack.ARealmReborn, $"{Loc.Localize("ARealmRebornText", "A Realm Reborn")}" },
            { ExpansionPack.Heavensward, $"{Loc.Localize("HeavenswardText", "Heavensward")}" },
            { ExpansionPack.Stormblood, $"{Loc.Localize("StormbloodText", "Stormblood")}" },
            { ExpansionPack.Shadowbringers, $"{Loc.Localize("ShadowbringersText", "Shadowbringers")}" },
            { ExpansionPack.Endwalker, $"{Loc.Localize("EndwalkerText", "Endwalker")}" },
            { ExpansionPack.Dawntrail, $"{Loc.Localize("DawntrailText", "Dawntrail")}" },
        };

        private readonly Dictionary<SonarJurisdiction, string> jurisdictionsCombo = new Dictionary<SonarJurisdiction, string>()
        {
            { SonarJurisdiction.Default, $"{Loc.Localize("DefaultJurisdictionText", "Default")}" },
            { SonarJurisdiction.None, $"{Loc.Localize("NoneJurisdictionText", "None")}" },
            { SonarJurisdiction.Instance, $"{Loc.Localize("InstanceJurisdictionText", "Instance")}" },
            { SonarJurisdiction.Zone, $"{Loc.Localize("ZoneJurisdictionText", "Zone")}" },
            { SonarJurisdiction.World, $"{Loc.Localize("WorldJurisdictionText", "World")}" },
            { SonarJurisdiction.Datacenter, $"{Loc.Localize("DatacenterJurisdictionText", "Data Center")}" },
            { SonarJurisdiction.Region, $"{Loc.Localize("RegionJurisdictionText", "Region")}" },
            { SonarJurisdiction.Audience, $"{Loc.Localize("AudienceJurisdictionText", "Audience")}" },
            { SonarJurisdiction.All, $"{Loc.Localize("AllJurisdictionText", "All")}" },
        };

        private readonly Dictionary<SonarLogLevel, string> logLevelCombo = new()
        {
            { SonarLogLevel.Verbose, $"{Loc.Localize("SonarLogLevelVerbose", "Verbose")}"},
            { SonarLogLevel.Debug, $"{Loc.Localize("SonarLogLevelDebug", "Debug")}" },
            { SonarLogLevel.Information, $"{Loc.Localize("SonarLogLevelInformation", "Information")}" },
            { SonarLogLevel.Warning, $"{Loc.Localize("SonarLogLevelWarning", "Warning")}" },
            { SonarLogLevel.Error, $"{Loc.Localize("SonarLogLevelError", "Error")}" },
            { SonarLogLevel.Fatal, $"{Loc.Localize("SonarLogLevelFatal", "Fatal")}" },
        };

        private void DrawHuntTabRankBasic(HuntRank rank)
        {
            ImGui.Text($"{this.rankStrings[rank]}");
            ImGui.SameLine(125.5f * ImGui.GetIO().FontGlobalScale);

            int index = this.jurisdictionsCombo.Keys.ToList().IndexOf(this.Client.Configuration.HuntConfig.GetJurisdiction(ExpansionPack.ARealmReborn, rank));
            if (ImGui.Combo($"##hunt{rank}", ref index, this.jurisdictionsCombo.Values.ToArray(), this.jurisdictionsCombo.Count))
            {
                this._save = this._server = true;
                foreach (ExpansionPack expansion in Enum.GetValues(typeof(ExpansionPack)))
                {
                    this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, rank, this.jurisdictionsCombo.Keys.ToList()[index]);
                    if (!this.Plugin.Configuration.AllSRankSettings && (rank == HuntRank.S))
                    {
                        this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, HuntRank.SS, this.jurisdictionsCombo.Keys.ToList()[index]);
                        this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, HuntRank.SSMinion, this.jurisdictionsCombo.Keys.ToList()[index]);
                    }
                }
            }
        }

        private void DrawHuntTabRankAdvanced(HuntRank rank)
        {
            if (ImGui.TreeNodeEx($"##Rank{rank}", ImGuiTreeNodeFlags.CollapsingHeader, this.rankStrings[rank]))
            {
                foreach (ExpansionPack expansion in Enum.GetValues(typeof(ExpansionPack)))
                {
                    if (expansion == ExpansionPack.Unknown) continue;
                    if (rank is HuntRank.SSMinion or HuntRank.SS && expansion <= ExpansionPack.Stormblood) continue;

                    ImGui.Text(this.expansionStrings[expansion]);
                    ImGui.SameLine(125.0f * ImGui.GetIO().FontGlobalScale);

                    int index = this.jurisdictionsCombo.Keys.ToList().IndexOf(this.Client.Configuration.HuntConfig.GetJurisdiction(expansion, rank));
                    if (ImGui.Combo($"##hunt{expansion}{rank}", ref index, this.jurisdictionsCombo.Values.ToArray(), this.jurisdictionsCombo.Count))
                    {
                        this._save = this._server = true;
                        this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, rank, this.jurisdictionsCombo.Keys.ToList()[index]);
                        if (!this.Plugin.Configuration.AllSRankSettings && (rank == HuntRank.S))
                        {
                            this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, HuntRank.SS, this.jurisdictionsCombo.Keys.ToList()[index]);
                            this.Client.Configuration.HuntConfig.SetJurisdiction(expansion, HuntRank.SSMinion, this.jurisdictionsCombo.Keys.ToList()[index]);
                        }
                    }
                }
            }
        }

        private void DrawFateTab()
        {
            using var child = ImRaii.Child("##huntTabScrollRegion");

            SonarImGui.Checkbox($"{ConfigLoc.ContributeFates.GetLocString()}###contributeFates", this.Client.Configuration.Contribute[RelayType.Fate], value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute[RelayType.Fate] = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(ConfigLoc.ContributeTooltip.GetLocString());
            }

            if (!this.Client.Configuration.Contribute.Global)
            {
                ImGui.SameLine();
                using var color = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                ImGui.TextUnformatted(ConfigLoc.ContributeGlobalDisabled.GetLocString());
            }

            SonarImGui.Checkbox($"{ConfigLoc.TrackAllFates.GetLocString()}###trackAllFates", this.Client.Configuration.FateConfig.TrackAll, value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.FateConfig.TrackAll = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(ConfigLoc.TrackAllTooltip.GetLocString());
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.ChatReportsConfig.GetLocString()}###fateChatConfig", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();
                    this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ChatReportsEnabled.GetLocString()}###fateChatEnabled", ref this.Plugin.Configuration.EnableFateChatReports);
                    if (this.Plugin.Configuration.EnableFateChatReports)
                    {
                        using var indent2 = ImRaii.PushIndent();
                        // TODO: might need to do extra checks here and default to Echo channel on failure.
                        // TODO: Chat types localization

                        var currentChat = XivChatTypeExtensions.GetDetails(this.Plugin.Configuration.FateOutputChannel)?.FancyName ?? this.Plugin.Configuration.FateOutputChannel.ToString();
                        var selectedChat = Array.IndexOf(this._chatTypes, currentChat);
                        if (ImGui.Combo("###chatTypes", ref selectedChat, this._chatTypes, this._chatTypes.Length))
                        {
                            this._save = true;
                            var value = XivChatTypeUtils.GetValueFromInfoAttribute(this._chatTypes[selectedChat]);
                            this.Plugin.Configuration.FateOutputChannel = value;
                        }

                        this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ChatEnableItalics.GetLocString()}###fateChatEnableItalic", ref this.Plugin.Configuration.EnableFateChatItalicFont);
                        this._save |= ImGui.Checkbox($"{ConfigWindowLoc.ChatEnableCwIcon.GetLocString()}###fateChatEnableCrossworldIcon", ref this.Plugin.Configuration.EnableFateChatCrossworldIcon);
                    }
                }
            }

            using (var node = ImRaii.TreeNode($"{ConfigWindowLoc.SoundAlertsConfig.GetLocString()}###fateAlertsConfig", ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (node.Success)
                {
                    using var indent = ImRaii.PushIndent();

                    using (var node2 = ImRaii.TreeNode($"{ConfigWindowLoc.FateSounds.GetLocString()}###fateSounds"))
                    {
                        if (node2.Success)
                        {
                            using var indent2 = ImRaii.PushIndent();
                            this._save |= SonarWidgets.SoundAlertsConfig("fateSoundsConfig", this.Plugin.Configuration.SoundFileFatesConfig, this.Sounds, this.FileDialogs);
                        }
                    }
                }
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##fateTabDisplayTimers", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("FateDisplayTimers", "Fate Display Timers")}"))
            {
                ImGui.Indent();

                ImGui.TextDisabled(Loc.Localize("DisplayTimersHelpText", "All timers are in seconds. CTRL+Click to input directly."));
                ImGui.Spacing();

                this._save |= ImGui.SliderInt($"{Loc.Localize("FateDeadTimer", "Time since completion or failure")}###fateDeadTimer", ref this.Plugin.Configuration.DisplayFateDeadTimer, 0, 604800);
                this._save |= ImGui.SliderInt($"{Loc.Localize("FateUpdateTimer", "Time since last update")}###fateUpdateTimer", ref this.Plugin.Configuration.DisplayFateUpdateTimer, 60, 604800);
                this._save |= ImGui.SliderInt($"{Loc.Localize("FatesDisplayLimit", "Fates Display Limit")}###fateDisplayLimit", ref this.Plugin.Configuration.FatesDisplayLimit, 1, 10000);

                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##fateSelection", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("FateSelection", "Fate Selection")}"))
            {
                ImGui.Indent();

                ImGui.Text(Loc.Localize("DefaultReportJurisdiction", "Default Report Jurisdiction")); ImGui.SameLine();
                int index = this.jurisdictionsCombo.Keys.ToList().IndexOf(this.Client.Configuration.FateConfig.GetDefaultJurisdiction());

                if (ImGui.Combo($"##fate_default", ref index, this.jurisdictionsCombo.Values.ToArray(), this.jurisdictionsCombo.Count))
                {
                    this._save = this._server = true;
                    this.Client.Configuration.FateConfig.SetDefaultJurisdiction(this.jurisdictionsCombo.Keys.ToList()[index]);
                }

                ImGui.Text(Loc.Localize("SearchText", "Search"));
                ImGui.SameLine();
                if (ImGui.InputText("##fateSearchText", ref this.fateSearchText, 100))
                {
                    // Default to false for IsField if unable to be retrieved to avoid showing fates that are setup incorrectly
                    this.filteredFateData = this.Index.Fates.Search(this.fateSearchText)
                        .DistinctBy(fate => fate.GroupId)
                        .ToList();
                    
                    //this.filteredFateData = _fateData
                    //    .Where(o => o.GetZone()?.IsField ?? false)
                    //    .Where(o => $"{o.Name}".IndexOf(fateSearchText, StringComparison.OrdinalIgnoreCase) != -1 ||
                    //                $"{o.GetZone()}".IndexOf(fateSearchText, StringComparison.OrdinalIgnoreCase) != -1 ||
                    //                $"{o.Level}".IndexOf(fateSearchText, StringComparison.OrdinalIgnoreCase) != -1)
                    //    .ToList();

                    fatesNeedSorting = true;
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                int selectedFates = this.Plugin.Configuration.SendFateToChat
                    .Select(id => Database.Fates.GetValueOrDefault(id))
                    .Where(fate => fate is not null)
                    .DistinctBy(fate => fate!.GroupId)
                    .Count();
                ImGui.Text(string.Format(Loc.Localize("SelectedFatesToChatText", "{0} fate(s) selected to chat"), selectedFates));
                ImGui.SameLine();
                ImGui.Text(" | ");
                ImGui.SameLine();
                selectedFates = this.Plugin.Configuration.SendFateToSound
                    .Select(id => Database.Fates.GetValueOrDefault(id))
                    .Where(fate => fate is not null)
                    .DistinctBy(fate => fate!.GroupId)
                    .Count();
                ImGui.Text(string.Format(Loc.Localize("SelectedFatesToSoundText", "{0} fate(s) selected to sound"), selectedFates));
                ImGui.SameLine();
                ImGui.Text(" | ");
                ImGui.SameLine();
                selectedFates = this.Client.Configuration.FateConfig.GetJurisdictions()
                    .Where(kvp => kvp.Value != SonarJurisdiction.Default)
                    .Select(kvp => Database.Fates.GetValueOrDefault(kvp.Key))
                    .Where(fate => fate is not null)
                    .DistinctBy(fate => fate!.GroupId)
                    .Count();
                ImGui.Text(string.Format(Loc.Localize("SelectedFatesToNonDefaultJurisdiction", "{0} fate(s) not set to default jurisdiction"), selectedFates));


                ImGui.Spacing();
                ImGui.Separator();

                // Start of fate selection grid and columns

                ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable
                    | ImGuiTableFlags.Sortable
                    | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.NoBordersInBody
                    | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY
                    | ImGuiTableFlags.SizingFixedFit;

                if (ImGui.BeginTable("##fateSelectionTable", fateTableColumnCount, flags, tableOuterSize, 0.0f))
                {
                    // TODO : Jurisdiction is currently set as no sort, will fix later once I find a good way to handle it
                    ImGui.TableSetupColumn(Loc.Localize("FateTableChatColumn", "Chat"), ImGuiTableColumnFlags.PreferSortDescending, 0.0f, (int)FateSelectionColumns.Chat);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableSoundColumn", "Sounds"), ImGuiTableColumnFlags.PreferSortDescending, 0.0f, (int)FateSelectionColumns.Sound);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableJurisdictionColumn", "Jurisdiction"), ImGuiTableColumnFlags.NoHide, 50.0f, (int)FateSelectionColumns.Jurisdiction);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableLevelColumn", "Level"), ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed, 0.0f, (int)FateSelectionColumns.Level);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableNameColumn", "Name"), ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.NoHide, 0.0f, (int)FateSelectionColumns.Name);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableZoneColumn", "Zone"), ImGuiTableColumnFlags.None, 0.0f, (int)FateSelectionColumns.Zone);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableExpansionColumn", "Expansion"), ImGuiTableColumnFlags.DefaultHide, 0.0f, (int)FateSelectionColumns.Expansion);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableAchievementColumn", "Achievement"), ImGuiTableColumnFlags.DefaultHide, 0.0f, (int)FateSelectionColumns.AchievementName);
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

                    if (sortSpecs.SpecsDirty || fatesNeedSorting)
                    {
                        if (filteredFateData.Count > 1)
                        {
                            filteredFateData = SortFateDataWithSortSpecs(sortSpecs, filteredFateData);
                            sortSpecs.SpecsDirty = false;
                        }
                        fatesNeedSorting = false;
                    }

                    Dictionary<uint, SonarJurisdiction> jurisdictions = this.Client.Configuration.FateConfig.GetJurisdictions();

                    for (int i = 0; i < this.filteredFateData.Count; i++)
                    {
                        var currentFate = filteredFateData[i];

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();

                        SonarImGui.Checkbox($"##fate_{currentFate.Id}_chat", this.Plugin.Configuration.SendFateToChat.Contains(currentFate.Id), _ =>
                        {
                            this._save = true;
                            if (this.Plugin.Configuration.SendFateToChat.Contains(currentFate.Id))
                            {
                                this.Plugin.Configuration.SendFateToChat.ExceptWith(currentFate.GroupFateIds);
                            }
                            else
                            {
                                this.Plugin.Configuration.SendFateToChat.UnionWith(currentFate.GroupFateIds);
                            }
                        });

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip($"{Loc.Localize("SendToChat", "Send to chat")}");
                        }

                        ImGui.TableNextColumn();

                        SonarImGui.Checkbox($"##fate_{currentFate.Id}_sound", this.Plugin.Configuration.SendFateToSound.Contains(currentFate.Id), _ =>
                        {
                            this._save = true;
                            if (this.Plugin.Configuration.SendFateToSound.Contains(currentFate.Id))
                            {
                                this.Plugin.Configuration.SendFateToSound.ExceptWith(currentFate.GroupFateIds);
                            }
                            else
                            {
                                this.Plugin.Configuration.SendFateToSound.UnionWith(currentFate.GroupFateIds);
                            }
                        });

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip($"{Loc.Localize("SendToSound", "Play Sounds")}");
                        }

                        ImGui.TableNextColumn();

                        if (!jurisdictions.TryGetValue(currentFate.Id, out SonarJurisdiction jurisdiction))
                        {
                            jurisdiction = SonarJurisdiction.Default;
                        }

                        index = this.jurisdictionsCombo.Keys.ToList().IndexOf(jurisdiction);
                        if (ImGui.Combo($"##fate_{currentFate.Id}_jurisdiction", ref index, this.jurisdictionsCombo.Values.ToArray(), this.jurisdictionsCombo.Count))
                        {
                            this._save = this._server = true;
                            foreach (var fateId in currentFate.GroupFateIds)
                            {
                                this.Client.Configuration.FateConfig.SetJurisdiction(fateId, this.jurisdictionsCombo.Keys.ToList()[index]);
                            }
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{currentFate.Level}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{currentFate.Name}");
                        ImGui.TableNextColumn();
                        ref var fateZones = ref CollectionsMarshal.GetValueRefOrAddDefault(this._fateZonesCache, currentFate.Id, out var cacheHit);
                        if (!cacheHit) fateZones = string.Join(", ", currentFate.GetGroupZones().Select(zone => zone?.ToString()).OrderBy(name => name));
                        ImGui.Text(fateZones);
                        ImGui.TableNextColumn();
                        ImGui.Text($"{ExpansionPackHelper.GetExpansionPackLongString(currentFate.Expansion)}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{currentFate.AchievementName}");
                    }

                    ImGui.EndTable();
                }

                ImGui.Unindent();
            }
        }

        private void DrawAboutTab()
        {
            ImGui.BeginChild("##aboutTabScrollRegion");
            {
                ImGui.Text($"{this.Stub.PluginName} v{Assembly.GetExecutingAssembly().GetName().Version}");
                ImGui.Text($"{Loc.Localize("AboutSonarBroughtBy", "Brought to you by the Sonar Team")}");

                if (ImGui.Button("Sonar Support Discord##SonarDiscord"))
                {
                    this._tasker.AddTask(Task.Run(() => { ShellExecute("https://discord.gg/K7y24Rr"); }));
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("AboutSonarSupport", "Ask questions and report bugs at the Sonar Support Discord Server.\nNote that Sonar Support is not provided in the goat place's discord.")}");

                ImGui.SameLine();

                if (ImGui.Button("Sonar Patreon##SonarPatreon"))
                {
                    this._tasker.AddTask(Task.Run(() => { ShellExecute("https://www.patreon.com/ffxivsonar"); }));
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("AboutSonarPatreon", "Support Sonar on Patreon.\nEarnings will be used for the hosting costs of the Sonar Server and Discord Bots.")}");
                // Earnings will be used for the Sonar server and discord bots hosting costs.
                // Earnings will be used for the Sonar Server and Discord Bot hosting costs.
                // Earnings will be used for the hosting costs of the Sonar Server and Discord Bots.

                ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

                ImGui.Text($"{Loc.Localize("SonarStatus", "Sonar Status")}: "); ImGui.SameLine();
                if (this.Client.Connection.IsConnected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ff33);
                    ImGui.Text($"{Loc.Localize("SonarConnected", "Connected")}");
                    ImGui.PopStyleColor();
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{this.Client.Ping:F0}ms");
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff0099ff);
                    ImGui.Text($"{Loc.Localize("SonarDisconnected", "Disconnected")}");
                    ImGui.PopStyleColor();
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("SonarDisconnectedTooltip", "Sonar will keep trying to reconnect automatically")}");
                }

                ImGui.SameLine();
                if (ImGui.Button($"{Loc.Localize("SonarReconnect", "Reconnect")}"))
                {
                    this.Client.Connection.Reconnect();
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("SonarReconnectTooltip", "Signal Sonar to attempt reconnecting to server")}");
            }

            ImGui.EndChild(); // End scroll region
        }

        private bool _showClientHash;
        private void DrawDebugTab()
        {
            ImGui.BeginChild("##debugTabScrollRegion");
            {
                ImGui.Text("Version Information");
                ImGui.BeginChild("##debugVersionInfo", new Vector2(0, 100 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"{this.Stub.PluginName} v{Assembly.GetExecutingAssembly().GetName().Version}");
                    ImGui.Text($"Dalamud {this.DalamudVersion.Version} (Git: {this.DalamudVersion.GitHash})");
                    ImGui.Text($"FFXIV {VersionUtils.GetGameVersion(this.Data)}");

                    ImGui.Text($"Client Hash: ");
                    ImGui.SameLine();
                    if (this._showClientHash)
                    {
                        ImGui.Text($"{this.Client.ClientHash ?? "Unknown"}");
                        ImGui.SameLine();
                    }

                    if (ImGui.Button($"{(this._showClientHash ? "Hide" : "Show")}")) this._showClientHash = !this._showClientHash;
                    ImGui.SameLine();
                    if (ImGui.Button($"Copy")) ImGui.SetClipboardText(this.Client.ClientHash);
                }
                ImGui.EndChild(); // debugVersionInfo

                ImGui.Spacing();

                ImGui.Text("Player Tracker");
                ImGui.BeginChild("##DebugPlayerTracker", new Vector2(0, 35 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"Zone: {this.Client.Meta.PlayerPosition}");
                }
                ImGui.EndChild(); // debugPlayerTracker
                ImGui.Spacing();

                ImGui.Text("Hunts Tracker");
                ImGui.BeginChild("##DebugHuntTracker", new Vector2(0, 80 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"Count: {this.Client.Trackers.Hunts.Data.Count} | Index: {this.Client.Trackers.Hunts.Data.IndexCount}");

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    if (this._debugHuntTask.IsCompleted)
                    {
                        if (ImGui.Button("Clear"))
                        {
                            this.Client.Trackers.Hunts.Data.Clear();
                            this.Logger.Information("Hunts Tracker Reset");
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Check"))
                        {
                            this._debugHuntTask = Task.Run(() =>
                            {
                                this.Logger.Information("Hunt index consistency check started");
                                var result = this.Client.Trackers.Hunts.Data.DebugIndexConsistencyCheck();
                                if (!result.Any())
                                {
                                    this.Logger.Information($"Hunt index debug consistency check successful");
                                }
                                else
                                {
                                    this.Logger.Warning($"Hunt index consistency check failed!\n{string.Join("\n", result)}");
                                }
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Perform a consistency check of the index\nOutput will be at /xllog");
                        ImGui.SameLine();
                        if (ImGui.Button("Rebuild"))
                        {
                            this._debugHuntTask = Task.Run(() =>
                            {
                                this.Logger.Information("Hunt index debug rebuild started");
                                this.Client.Trackers.Hunts.Data.RebuildIndex();
                                this.Logger.Information("Hunt index debug rebuild complete");
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Rebuild index\nOutput will be at /xllog\n\nWarning: You may experience stuttering");
                    }
                    else
                    {
                        ImGui.Text("Busy");
                    }
                }
                ImGui.EndChild(); // debugHuntTracker
                ImGui.Spacing();

                ImGui.Text("Fates Tracker");
                ImGui.BeginChild("##DebugFateTracker", new Vector2(0, 80 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"Count: {this.Client.Trackers.Fates.Data.Count} | Index: {this.Client.Trackers.Fates.Data.IndexCount}");

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    if (this._debugFateTask.IsCompleted)
                    {
                        if (ImGui.Button("Clear"))
                        {
                            this.Client.Trackers.Fates.Data.Clear();
                            this.Logger.Information("Fates Tracker Reset");
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Check"))
                        {
                            this._debugFateTask = Task.Run(() =>
                            {
                                this.Logger.Information("Fate index debug consistency check started");
                                var result = this.Client.Trackers.Fates.Data.DebugIndexConsistencyCheck();
                                if (!result.Any())
                                {
                                    this.Logger.Information($"Fate index consistency check successful");
                                }
                                else
                                {
                                    this.Logger.Warning($"Fate consistency check failed!\n{string.Join("\n", result)}");
                                }
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Perform a consistency check of the index\nOutput will be at /xllog");
                        ImGui.SameLine();
                        if (ImGui.Button("Rebuild"))
                        {
                            this._debugFateTask = Task.Run(() =>
                            {
                                this.Logger.Information("Fate index debug rebuild started");
                                this.Client.Trackers.Fates.Data.RebuildIndex();
                                this.Logger.Information("Fate index debug rebuild complete");
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Rebuild index\nOutput will be at /xllog\n\nWarning: You may experience stuttering");
                    }
                    else
                    {
                        ImGui.Text("Busy");
                    }
                }
                ImGui.EndChild(); // debugFateTracker
                ImGui.Spacing();

                if (ImGui.Button("Request Relay Data"))
                {
                    this.Plugin.Configuration.SonarConfig.HuntConfig.TrackAll = true;
                    this.Plugin.Configuration.SonarConfig.FateConfig.TrackAll = true;
                    this.Plugin.SaveConfiguration();
                    this.Client.RequestRelayData();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Request all relay information to be received.\nTrack All options will automatically be enabled for both Hunts and Fates.\nThis can only be done once.\n\nWarning: You'll receive everything the Sonar server knows!\nThis is currently under testing.");
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            var index = this.logLevelCombo.Keys.ToList().IndexOf(this.Client.Configuration.LogLevel);
            if (ImGui.Combo($"{Loc.Localize("LogLevel", "Log Level")}", ref index, this.logLevelCombo.Values.ToArray(), this.logLevelCombo.Count))
            {
                this._save = this._server = true;
                this.Client.LogLevel = this.logLevelCombo.Keys.ToList()[index]; // This also updates configuration
            }

            #if DEBUG
            #pragma warning disable S1215
            #pragma warning disable S125
            if (ImGui.Button("GC.Collect"))
            {
                GC.Collect();
            }
            ImGui.SameLine();
            if (ImGui.Button("Hard GC.Collect"))
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            #pragma warning restore S125
            #pragma warning restore S1215
            #endif

            ImGui.EndChild(); // debugTabScrollRegion
        }

        #region Fate Selection Table
        private struct FateTableRow
        {
            public bool SendToChat { get; set; }
            public bool SendToSound { get; set; }
            public FateRow Fate { get; set; }
        }

        private enum FateSelectionColumns
        {
            Sound,
            Chat,
            Jurisdiction,
            Level,
            Name,
            Zone,
            Expansion,
            AchievementName
        }

        private List<FateRow> SortFateDataWithSortSpecs(ImGuiTableSortSpecsPtr sortSpecs, List<FateRow> fateDataToSort)
        {
            IEnumerable<FateRow> sortedFateData = fateDataToSort;

            this.Logger.Debug($"Sort Spec count - {sortSpecs.SpecsCount}");

            for (int i = 0; i < sortSpecs.SpecsCount; i++)
            {
                ImGuiTableColumnSortSpecsPtr columnSortSpec = sortSpecs.Specs;

                switch ((FateSelectionColumns)columnSortSpec.ColumnUserID)
                {
                    case FateSelectionColumns.Sound:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => this.Plugin.Configuration.SendFateToSound.Contains(o.Id));
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => this.Plugin.Configuration.SendFateToSound.Contains(o.Id));
                        }
                        break;
                    case FateSelectionColumns.Chat:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => this.Plugin.Configuration.SendFateToChat.Contains(o.Id));
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => this.Plugin.Configuration.SendFateToChat.Contains(o.Id));
                        }
                        break;
                    case FateSelectionColumns.Jurisdiction:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => (int)this.Client.Configuration.FateConfig.GetJurisdiction(o.Id));
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => (int)this.Client.Configuration.FateConfig.GetJurisdiction(o.Id));
                        }
                        break;
                    case FateSelectionColumns.Level:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => o.Level);
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => o.Level);
                        }
                        break;
                    case FateSelectionColumns.Name:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => o.Name.ToString());
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => o.Name.ToString());
                        }
                        break;
                    case FateSelectionColumns.Zone:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => o.GetZone()?.Name.ToString());
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => o.GetZone()?.Name.ToString());
                        }
                        break;
                    case FateSelectionColumns.Expansion:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => ExpansionPackHelper.GetExpansionPackLongString(o.Expansion));
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => ExpansionPackHelper.GetExpansionPackLongString(o.Expansion));
                        }
                        break;
                    case FateSelectionColumns.AchievementName:
                        if (columnSortSpec.SortDirection == ImGuiSortDirection.Ascending)
                        {
                            sortedFateData = sortedFateData.OrderBy(o => o.AchievementName.ToString());
                        }
                        else
                        {
                            sortedFateData = sortedFateData.OrderByDescending(o => o.AchievementName.ToString());
                        }
                        break;
                    default:
                        break;
                }
            }

            return sortedFateData.ToList();
        }
        #endregion

        #region IDisposable Support
        public void Dispose()
        {
            this.Plugin.Windows.RemoveWindow(this);
            this.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfig;
            this._tasker?.Dispose();
        }
        #endregion
    }
}
