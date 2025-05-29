using CheapLoc;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Sonar;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Logging;
using Sonar.Utilities;
using SonarPlugin.Attributes;
using SonarPlugin.Config;
using SonarPlugin.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Logging;
using static SonarPlugin.Utility.ShellUtils;
using Dalamud.Data;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Interface.ImGuiFileDialog;
using System.Runtime;
using System.Runtime.InteropServices;

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
        private FileDialogManager FileDialogs { get; }
        private IndexProvider Index { get; }
        private IPluginLog Logger { get; }

        private AudioPlaybackEngine Audio { get; }

        private readonly Vector2 tableOuterSize = new(0.0f, ImGui.GetTextLineHeightWithSpacing() * 30);

        private readonly Tasker _tasker = new();

        private readonly string[] _chatTypes;
        private string[] audioFilesForSRanks = default!;
        private string[] audioFilesForARanks = default!;
        private readonly List<string> _defaultAudioResourceList;
        private string fateSearchText = string.Empty;

        private List<FateRow> filteredFateData;
        private readonly Dictionary<uint, string> _fateZonesCache = new();
        private string[] audioFilesForFates = default!;
        private readonly Dictionary<uint, HashSet<uint>> _combinedFates = new();
        private readonly int fateTableColumnCount = Enum.GetNames(typeof(FateSelectionColumns)).Length;

        public SonarConfigWindow(SonarPlugin plugin, SonarPluginStub stub, IDalamudPluginInterface pluginInterface, SonarClient client, IDataManager data, AudioPlaybackEngine audio, FileDialogManager fileDialogs, IndexProvider index, IPluginLog logger) : base("Sonar Configuration")
        {
            this.Plugin = plugin;
            this.Stub = stub;
            this.PluginInterface = pluginInterface;
            this.Client = client;
            this.Data = data;
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

            // Audio setup for choosing either existing sounds or custom sound
            this._defaultAudioResourceList = new List<string>();

            // TODO: For now assuming any file that doesn't have an extension is an .mp3 file since I'm logically changing the name in the project file
            //       This is probably not the BEST way of handling this but makes the logic of display and getting at the sound file easier for now.
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (Path.GetExtension(resource) != string.Empty) continue;
                _defaultAudioResourceList.Add(resource);
            }

            this._defaultAudioResourceList.Add(Loc.Localize("CustomSoundOption", "Custom..."));

            this.SetupSRankAudioSelection();
            this.SetupARankAudioSelection();
            this.SetupFateAudioSelection();

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
        public override void Draw()
        {
            if (ImGui.BeginTabBar("##tabBar", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem($"{Loc.Localize("GeneralHeader", "General")}##generalTab"))
                {
                    this.DrawGeneralTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{Loc.Localize("HuntsHeader", "Hunts")}##huntTab"))
                {
                    this.DrawHuntTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{Loc.Localize("FatesHeader", "FATEs")}##fateTab"))
                {
                    this.DrawFateTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{Loc.Localize("AboutHeader", "About")}##aboutTab"))
                {
                    this.DrawAboutTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Debug##debugTab"))
                {
                    this.DrawDebugTab();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
            if (this._save) this.Plugin.SaveConfiguration(this._server);
            this._save = this._server = false;
        }

        private void DrawGeneralTab()
        {
            ImGui.BeginChild("##generalTabScrollRegion");

            SonarImGui.Checkbox($"{Loc.Localize("ContributeGlobal", "Global Contribute")}##contributeHunts", this.Client.Configuration.Contribute.Global, value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute.Global = value;
            });
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("ContributeGlobalTooltip", "Disable this to disable contributing both hunts and fates.\nAccessible via /sonaron and /sonaroff commands.")}");
            }

            this._save |= ImGui.Checkbox($"{Loc.Localize("OverlayVisibleByDefault", "Overlay Visible by default")}##overlayVisibleByDefault", ref this.Plugin.Configuration.OverlayVisibleByDefault);
            this._save |= ImGui.Checkbox($"{Loc.Localize("LockOverlaysConfig", "Lock overlays")}##lockOverlays", ref this.Plugin.Configuration.EnableLockedOverlays);
            this._save |= ImGui.Checkbox($"{Loc.Localize("HideTitleBarConfig", "Hide Title Bar")}##hideTitlebar", ref this.Plugin.Configuration.HideTitlebar);
            this._save |= ImGui.Checkbox($"{Loc.Localize("EnableClickthroughConfig", "Enable window clickthrough")}##enableWindowClickthrough", ref this.Plugin.Configuration.WindowClickThrough);
            if (this.Plugin.Configuration.WindowClickThrough)
            {
                ImGui.Indent();
                this._save |= ImGui.Checkbox($"{Loc.Localize("TabBarClickthroughConfig", "Enable tab bar clickthrough")}##enableTabBarClickthrough", ref this.Plugin.Configuration.TabBarClickThrough);
                this._save |= ImGui.Checkbox($"{Loc.Localize("ListClickthroughConfig", "Enable list clickthrough")}##enableListClickthrough", ref this.Plugin.Configuration.ListClickThrough);
                ImGui.Unindent();
            }

            SonarImGui.Combo($"{Loc.Localize("SortingMode", "Sorting Mode")}", (int)this.Plugin.Configuration.SortingMode, EnumCheapLocExtensions.CheapLoc<RelayStateSortingMode>().Values.ToArray(), value =>
            {
                this._save = true;
                this.Plugin.Configuration.SortingMode = (RelayStateSortingMode)value;
            });

            var index = this.jurisdictionsCombo.Keys.ToList().IndexOf(this.Client.Configuration.Contribute.ReceiveJurisdiction);
            if (ImGui.Combo($"{Loc.Localize("ReceiveJurisdiction", "Receive Jurisdiction")}", ref index, this.jurisdictionsCombo.Values.ToArray(), this.jurisdictionsCombo.Count))
            {
                this._save = this._server = true;
                if (index == 0) index = 5;
                this.Client.Configuration.Contribute.ReceiveJurisdiction = this.jurisdictionsCombo.Keys.ToList()[index];
            }

            this._save |= ImGui.SliderFloat($"{Loc.Localize("OpacityConfig", "Opacity")}##opacitySlider", ref this.Plugin.Configuration.Opacity, 0.0f, 1.0f);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.SliderFloat($"{Loc.Localize("AlertVolumeConfig", "Alert Volume")}##volumeSlider", ref this.Plugin.Configuration.SoundVolume, 0.0f, 1.0f))
            {
                this.Audio.Volume = this.Plugin.Configuration.SoundVolume;
                this._save = true;
            }

            // TODO: Add language combo when we add resource files and change text

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##generalTabDutySettings", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("DutySettings", "Duty Settings")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.Checkbox($"{Loc.Localize("DisableChatAlertsDuties", "Disable Chat Alerts during Duties")}##enableWindowClickthrough", ref this.Plugin.Configuration.DisableChatInDuty);
                this._save |= ImGui.Checkbox($"{Loc.Localize("DisableSoundAlertsDuties", "Disable Sound Alerts during Duties")}##enableWindowClickthrough", ref this.Plugin.Configuration.DisableSoundInDuty);

                ImGui.Unindent();
            }

            if (ImGui.TreeNodeEx("##generalTabClicks", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ClickSettings", "Click Settings")}"))
            {
                ImGui.Indent();

                var actions = EnumCheapLocExtensions.CheapLoc<ClickAction>().Values.ToArray();
                SonarImGui.Combo($"{Loc.Localize("MiddleClick", "Middle Click")}##middleClickConfig", (int)this.Plugin.Configuration.MiddleClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 2;
                    this.Plugin.Configuration.MiddleClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                SonarImGui.Combo($"{Loc.Localize("RightClick", "Right Click")}##rightClickConfig", (int)this.Plugin.Configuration.RightClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 3;
                    this.Plugin.Configuration.RightClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                // ==

                SonarImGui.Combo($"{Loc.Localize("ShiftMiddleClick", "Shift Middle Click")}##shiftmiddleClickConfig", (int)this.Plugin.Configuration.ShiftMiddleClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 1;
                    this.Plugin.Configuration.ShiftMiddleClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                SonarImGui.Combo($"{Loc.Localize("ShiftRightClick", "Shift Right Click")}##shiftrightClickConfig", (int)this.Plugin.Configuration.ShiftRightClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 1;
                    this.Plugin.Configuration.ShiftRightClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                // ==

                SonarImGui.Combo($"{Loc.Localize("AltMiddleClick", "AltMiddle Click")}##AltmiddleClickConfig", (int)this.Plugin.Configuration.AltMiddleClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 1;
                    this.Plugin.Configuration.AltMiddleClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                SonarImGui.Combo($"{Loc.Localize("AltRightClick", "AltRight Click")}##AltrightClickConfig", (int)this.Plugin.Configuration.AltRightClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 1;
                    this.Plugin.Configuration.AltRightClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                // ==

                SonarImGui.Combo($"{Loc.Localize("CtrlMiddleClick", "CtrlMiddle Click")}##CtrlmiddleClickConfig", (int)this.Plugin.Configuration.CtrlMiddleClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 1;
                    this.Plugin.Configuration.CtrlMiddleClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                SonarImGui.Combo($"{Loc.Localize("CtrlRightClick", "CtrlRight Click")}##CtrlrightClickConfig", (int)this.Plugin.Configuration.CtrlRightClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 4;
                    this.Plugin.Configuration.CtrlRightClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                ImGui.Spacing();

                var cityStates = Enum.GetValues<CityState>().Select(cityState => (cityState, cityState.GetMeta())).ToArray();
                var cityStateStrings = cityStates.Select(ct => ct.Item2?.GetZone()?.ToString() ?? string.Empty).ToArray();
                SonarImGui.Combo($"{Loc.Localize("PreferredCityState", "Preferred City State")}##preferredCityState", (int)this.Plugin.Configuration.PreferredCityState, cityStateStrings, index =>
                {
                    this._save = true;
                    this.Plugin.Configuration.PreferredCityState = cityStates[index].cityState;
                });

                ImGui.Unindent();
            }

            if (ImGui.TreeNodeEx("##generalTabLodestone", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("LodestoneSettings", "Lodestone Verification Settings")}"))
            {
                ImGui.Indent();
                var suppressions = EnumCheapLocExtensions.CheapLoc<SuppressVerification>().Values.ToArray();
                SonarImGui.Combo($"{Loc.Localize("SuppressVerification", "Suppress Verification Requests")}##suppressVerification", (int)this.Plugin.Configuration.SuppressVerification, suppressions, index =>
                {
                    this._save = true;
                    this.Plugin.Configuration.SuppressVerification = EnumCheapLocExtensions.CheapLoc<SuppressVerification>().Keys.ToArray()[index];
                });

                ImGui.Unindent();
            }

            if (ImGui.TreeNodeEx("##generalTabColorScheme", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ColorScheme", "Sonar Color Scheme")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.ColorEdit4(Loc.Localize("HuntHealthyColor", "Hunt - Healthy"), ref this.Plugin.Configuration.Colors.HuntHealthy, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("HuntPulledColor", "Hunt - PulledStatus"), ref this.Plugin.Configuration.Colors.HuntPulled, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("HuntDeadColor", "Hunt - DeadStatus"), ref this.Plugin.Configuration.Colors.HuntDead, ImGuiColorEditFlags.NoInputs);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                this._save |= ImGui.ColorEdit4(Loc.Localize("FateRunningColor", "Fate - Running"), ref this.Plugin.Configuration.Colors.FateRunning, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateProgressColor", "Fate - Progress"), ref this.Plugin.Configuration.Colors.FateProgress, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateCompleteColor", "Fate - Complete"), ref this.Plugin.Configuration.Colors.FateComplete, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateFailedColor", "Fate - Failed"), ref this.Plugin.Configuration.Colors.FateFailed, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FatePreparationColor", "Fate - Preparation"), ref this.Plugin.Configuration.Colors.FatePreparation, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateUnknownColor", "Fate - Unknown"), ref this.Plugin.Configuration.Colors.FateUnknown, ImGuiColorEditFlags.NoInputs);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.TextUnformatted(Loc.Localize("ColorSchemePresets", "Presets"));

                if (ImGui.Button(Loc.Localize("ColorSchemeDefault", "Defaults")))
                {
                    this._save = true;
                    this.Plugin.Configuration.Colors.SetDefaults();
                }

                ImGui.SameLine();
                if (ImGui.Button(Loc.Localize("ColorSchemeDefault", "Original")))
                {
                    this._save = true;
                    this.Plugin.Configuration.Colors.HuntHealthy = ColorPalette.Green;
                    this.Plugin.Configuration.Colors.HuntPulled = ColorPalette.Yellow;
                    this.Plugin.Configuration.Colors.HuntDead = ColorPalette.Red;

                    this.Plugin.Configuration.Colors.FateRunning = ColorPalette.Green;
                    this.Plugin.Configuration.Colors.FateProgress = ColorPalette.Yellow;
                    this.Plugin.Configuration.Colors.FateComplete = ColorPalette.Red;
                    this.Plugin.Configuration.Colors.FateFailed = ColorPalette.Red;
                    this.Plugin.Configuration.Colors.FatePreparation = ColorPalette.White;
                    this.Plugin.Configuration.Colors.FateUnknown = ColorPalette.Red;
                }

                ImGui.Unindent();
            }
            ImGui.EndChild(); // End scroll region
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
            ImGui.BeginChild("##huntTab2ScrollRegion");

            SonarImGui.Checkbox($"{Loc.Localize("ContributeHunts", "Contribute Hunt Reports")}{(this.Client.Configuration.Contribute.Global ? "" : " (Disabled)")}##contributeHunts", this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Hunt], value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Hunt] = value;
            });
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("ContributeHuntsTooltip", "Contributing hunt reports is required in order to receive hunt reports from other Sonar users.\nIf disabled Sonar will continue to work in local mode, where you'll see what's detected within your game but you'll not receive from others.")}");
            }
            SonarImGui.Checkbox($"{Loc.Localize("TrackAllHunts", "Track All Hunts")}##trackAllHunts", this.Client.Configuration.HuntConfig.TrackAll, value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.HuntConfig.TrackAll = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("TrackAllHuntsTooltip", "Checked: Track all hunts regardless of jurisdiction settings.\nUnchecked: Track hunts within jurisdiction settings only.")}");
            }

            if (ImGui.TreeNodeEx("##huntTabChatConfig", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("ConfigureHuntChat", "Hunt Chat Configuration")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnabledConfig", "Show hunt reports in game chat")}##huntChatEnabled", ref this.Plugin.Configuration.EnableGameChatReports);
                if (this.Plugin.Configuration.EnableGameChatReports)
                {
                    ImGui.Indent();
                    // TODO: might need to do extra checks here and default to Echo channel on failure.
                    var currentChat = XivChatTypeExtensions.GetDetails(this.Plugin.Configuration.HuntOutputChannel)?.FancyName ?? this.Plugin.Configuration.HuntOutputChannel.ToString();
                    var selectedChat = Array.IndexOf(this._chatTypes, currentChat);

                    if (ImGui.Combo("##chatTypes", ref selectedChat, this._chatTypes, this._chatTypes.Length))
                    {
                        this._save = true;
                        var value = XivChatTypeUtils.GetValueFromInfoAttribute(_chatTypes[selectedChat]);
                        this.Plugin.Configuration.HuntOutputChannel = value;
                    }

                    this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnableItalic", "Enable italic font for game chat")}##huntChatEnableItalic", ref this.Plugin.Configuration.EnableGameChatItalicFont);
                    this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnableCrosswordIcon", "Enable cross world icon when relay is not from current world")}##huntChatEnableCrossworldIcon", ref this.Plugin.Configuration.EnableGameChatCrossworldIcon);
                    this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnableDeathChat", "Show hunt deaths in game chat")}##huntChatEnableDeaths", ref this.Plugin.Configuration.EnableGameChatReportsDeaths);
                    ImGui.Unindent();
                }

                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##huntTabSoundAlertConfig", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("ConfigureHuntSounds", "Hunt Sound Alerts Configuration")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.Checkbox($"{Loc.Localize("PlaySoundSConfig", "Play sound on Rank S")}##sRankSoundEnabled", ref this.Plugin.Configuration.PlaySoundSRanks);
                if (this.Plugin.Configuration.PlaySoundSRanks)
                {
                    ImGui.Indent();
                    var selectedSound = Array.IndexOf(this.audioFilesForSRanks, this.Plugin.Configuration.SoundFileSRanks);
                    if (ImGui.Combo("##sRankSounds", ref selectedSound, this.audioFilesForSRanks, this.audioFilesForSRanks.Length))
                    {
                        if (selectedSound == this.audioFilesForSRanks.Length - 1)
                        {
                            this.FileDialogs.OpenFileDialog(Loc.Localize("SelectSRankSoundTitle", "Select S Rank sound file"), "Sound Files{.3g2,.3gp,.3gp2,.3gpp,.asf,.wma,.wmv,.aac,.adts,.avi,.mp3,.m4a,.m4v,.mov,.mp4,.sami,.smi,.wav}", (success, filename) =>
                            {
                                if (!success) return;
                                this._save = true;
                                this.Plugin.Configuration.SoundFileSRanks = filename;
                                this.SetupSRankAudioSelection();
                                this.Audio.PlaySound(filename);
                            });
                        }
                        else
                        {
                            this._save = true;
                            this.Plugin.Configuration.SoundFileSRanks = audioFilesForSRanks[selectedSound];
                            this.Audio.PlaySound(this.Plugin.Configuration.SoundFileSRanks);
                        }
                    }
                    ImGui.Unindent();
                }

                this._save |= ImGui.Checkbox($"{Loc.Localize("PlaySoundAConfig", "Play sound on Rank A")}##aRankSoundEnabled", ref this.Plugin.Configuration.PlaySoundARanks);
                if (this.Plugin.Configuration.PlaySoundARanks)
                {
                    ImGui.Indent();
                    var selectedSound = Array.IndexOf(this.audioFilesForARanks, this.Plugin.Configuration.SoundFileARanks);
                    if (ImGui.Combo("##aRankSounds", ref selectedSound, this.audioFilesForARanks, this.audioFilesForARanks.Length))
                    {
                        if (selectedSound == this.audioFilesForARanks.Length - 1)
                        {
                            this.FileDialogs.OpenFileDialog(Loc.Localize("SelectARankSoundTitle", "Select A Rank sound file"), "Sound Files{.3g2,.3gp,.3gp2,.3gpp,.asf,.wma,.wmv,.aac,.adts,.avi,.mp3,.m4a,.m4v,.mov,.mp4,.sami,.smi,.wav}", (success, filename) =>
                            {
                                if (!success) return;
                                this._save = true;
                                this.Plugin.Configuration.SoundFileARanks = filename;
                                this.SetupARankAudioSelection();
                                this.Audio.PlaySound(filename);
                            });
                        }
                        else
                        {
                            this._save = true;
                            this.Plugin.Configuration.SoundFileARanks = audioFilesForARanks[selectedSound];
                            this.Audio.PlaySound(this.Plugin.Configuration.SoundFileARanks);
                        }
                    }
                    ImGui.Unindent();
                }
                ImGui.Unindent();
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
                        foreach (HuntRank rank in Enum.GetValues(typeof(HuntRank)))
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
                foreach (var rank in ranks.Reverse())
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

            ImGui.EndChild(); // ##huntTabScrollRegion
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
            ImGui.BeginChild("##fateTabScrollRegion");

            SonarImGui.Checkbox($"{Loc.Localize("ContributeFates", "Contribute Fate Reports")}{(this.Client.Configuration.Contribute.Global ? "" : " (Disabled)")}##contributeFates", this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Fate], value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Fate] = value;
            });
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("ContributeHuntsTooltip", "Contributing fate reports is required in order to receive fate reports from other Sonar users.\nIf disabled Sonar will continue to work in local mode, where you'll see what's detected within your game but you'll not receive from others.")}");
            }

            SonarImGui.Checkbox($"{Loc.Localize("TrackAllFates", "Track All Fates")}##trackAllFates", this.Client.Configuration.FateConfig.TrackAll, value =>
            {
                this.Plugin.SaveConfiguration(true);
                this.Client.Configuration.FateConfig.TrackAll = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("TrackAllFatesTooltip", "Checked: Track all fates regardless of jurisdiction settings.\nUnchecked: Track fates within jurisdiction settings only.")}");
            }

            if (ImGui.TreeNodeEx("##fateTabChatConfig", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ConfigureFateChat", "Fate Chat Configuration")}"))
            {
                ImGui.Indent();
                if (ImGui.Checkbox($"{Loc.Localize("FateChatEnabledConfig", "Show fate reports in game chat")}##fateChatEnabled", ref this.Plugin.Configuration.EnableFateChatReports))
                {
                    this._save = true;
                }
                if (this.Plugin.Configuration.EnableFateChatReports)
                {
                    ImGui.Indent();
                    // TODO: might need to do extra checks here and default to Echo channel on failure.
                    string currentChat = XivChatTypeExtensions.GetDetails(this.Plugin.Configuration.FateOutputChannel)?.FancyName ?? this.Plugin.Configuration.FateOutputChannel.ToString();
                    int selectedChat = Array.IndexOf(this._chatTypes, currentChat);

                    if (ImGui.Combo("##chatTypes", ref selectedChat, this._chatTypes, this._chatTypes.Length))
                    {
                        this._save = true;
                        var value = XivChatTypeUtils.GetValueFromInfoAttribute(_chatTypes[selectedChat]);
                        this.Plugin.Configuration.FateOutputChannel = value;
                    }

                    ImGui.Checkbox($"{Loc.Localize("FateChatEnableItalic", "Enable italic font for game chat")}##fateChatEnableItalic", ref this.Plugin.Configuration.EnableFateChatItalicFont);
                    ImGui.Checkbox($"{Loc.Localize("FateChatEnableCrosswordIcon", "Enable cross world icon when relay is not from current world")}##fateChatEnableCrossworldIcon", ref this.Plugin.Configuration.EnableFateChatCrossworldIcon);

                    ImGui.Unindent();
                }

                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##fateTabSoundAlertConfig", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ConfigureFateSounds", "Fate Sound Alerts Configuration")}"))
            {
                ImGui.Indent();

                ImGui.Checkbox($"{Loc.Localize("PlaySoundFateConfig", "Play sound on Fates")}##fateSoundEnabled", ref this.Plugin.Configuration.PlaySoundFates);
                if (this.Plugin.Configuration.PlaySoundFates)
                {
                    ImGui.Indent();
                    int selectedSound = Array.IndexOf(this.audioFilesForFates, this.Plugin.Configuration.SoundFileFates);
                    if (ImGui.Combo("##fateSounds", ref selectedSound, this.audioFilesForFates, this.audioFilesForFates.Length))
                    {
                        if (selectedSound == this.audioFilesForFates.Length - 1)
                        {
                            this.FileDialogs.OpenFileDialog(Loc.Localize("SelectFateSoundTitle", "Select fate sound file"), "Sound Files{.3g2,.3gp,.3gp2,.3gpp,.asf,.wma,.wmv,.aac,.adts,.avi,.mp3,.m4a,.m4v,.mov,.mp4,.sami,.smi,.wav}", (success, filename) =>
                            {
                                if (!success) return;
                                this._save = true;
                                this.Plugin.Configuration.SoundFileFates = filename;
                                this.SetupFateAudioSelection();
                                this.Audio.PlaySound(filename);
                            });
                        }
                        else
                        {
                            this._save = true;
                            this.Plugin.Configuration.SoundFileFates = audioFilesForFates[selectedSound];
                            this.Audio.PlaySound(this.Plugin.Configuration.SoundFileFates);
                        }
                    }
                    ImGui.Unindent();
                }

                ImGui.Unindent();
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
                    ImGui.TableSetupColumn(Loc.Localize("FateTableSoundColumn", "Sound"), ImGuiTableColumnFlags.PreferSortDescending, 0.0f, (int)FateSelectionColumns.Sound);
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
                            ImGui.SetTooltip($"{Loc.Localize("SendToSound", "Play Sound")}");
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

            ImGui.EndChild(); // End scroll region
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

        private bool _showIdentifier;
        private void DrawDebugTab()
        {
            ImGui.BeginChild("##debugTabScrollRegion");
            {
                ImGui.Text("Version Information");
                ImGui.BeginChild("##debugVersionInfo", new Vector2(0, 100 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"{this.Stub.PluginName} v{Assembly.GetExecutingAssembly().GetName().Version}");
                    ImGui.Text($"Dalamud {VersionUtils.GetDalamudVersion()} (Git: {VersionUtils.GetDalamudBuild()})");
                    ImGui.Text($"FFXIV {VersionUtils.GetGameVersion(this.Data)}");

                    ImGui.Text($"Client Hash: ");
                    ImGui.SameLine();
                    if (this._showIdentifier)
                    {
                        ImGui.Text($"{this.Client.ClientHash ?? "Unknown"}");
                        ImGui.SameLine();
                    }

                    if (ImGui.Button($"{(this._showIdentifier ? "Hide" : "Show")}")) this._showIdentifier = !this._showIdentifier;
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

        private void SetupSRankAudioSelection()
        {
            if (!string.IsNullOrWhiteSpace(this.Plugin.Configuration.SoundFileSRanks))
            {
                List<string> tempList = new()
                {
                    this.Plugin.Configuration.SoundFileSRanks
                };
                tempList.AddRange(_defaultAudioResourceList);
                this.audioFilesForSRanks = tempList.Distinct().ToArray();
            }
            else
            {
                this.audioFilesForSRanks = _defaultAudioResourceList.ToArray();
            }
        }

        private void SetupARankAudioSelection()
        {
            if (!string.IsNullOrWhiteSpace(this.Plugin.Configuration.SoundFileARanks))
            {
                List<string> tempList = new()
                {
                    this.Plugin.Configuration.SoundFileARanks
                };
                tempList.AddRange(_defaultAudioResourceList);
                this.audioFilesForARanks = tempList.Distinct().ToArray();
            }
            else
            {
                this.audioFilesForARanks = _defaultAudioResourceList.ToArray();
            }
        }

        private void SetupFateAudioSelection()
        {
            if (!string.IsNullOrWhiteSpace(this.Plugin.Configuration.SoundFileFates))
            {
                List<string> tempList = new()
                {
                    this.Plugin.Configuration.SoundFileFates
                };
                tempList.AddRange(_defaultAudioResourceList);
                audioFilesForFates = tempList.Distinct().ToArray();
            }
            else
            {
                audioFilesForFates = _defaultAudioResourceList.ToArray();
            }
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
