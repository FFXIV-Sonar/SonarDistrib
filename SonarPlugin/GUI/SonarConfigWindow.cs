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

        public SonarConfigWindow(SonarPlugin plugin, SonarPluginStub stub, IDalamudPluginInterface pluginInterface, SonarClient client, IDataManager data, AudioPlaybackEngine audio, FileDialogManager fileDialogs, IndexProvider index, IPluginLog logger) : base("Sonar 설정")
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

            this._defaultAudioResourceList.Add(Loc.Localize("CustomSoundOption", "사용자 설정..."));

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
                if (ImGui.BeginTabItem($"{Loc.Localize("GeneralHeader", "일반")}##generalTab"))
                {
                    this.DrawGeneralTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{Loc.Localize("HuntsHeader", "마물")}##huntTab"))
                {
                    this.DrawHuntTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{Loc.Localize("FatesHeader", "돌발")}##fateTab"))
                {
                    this.DrawFateTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{Loc.Localize("AboutHeader", "정보")}##aboutTab"))
                {
                    this.DrawAboutTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("디버그##debugTab"))
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

            SonarImGui.Checkbox($"{Loc.Localize("ContributeGlobal", "전파 기여")}##contributeHunts", this.Client.Configuration.Contribute.Global, value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute.Global = value;
            });
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("ContributeGlobalTooltip", "마물과 돌발 전파 기여를 모두 비활성화 합니다.\n또한 /sonaron 와 /sonaroff 명령어로도 활성화 및 비활성화가 가능합니다.")}");
            }

            this._save |= ImGui.Checkbox($"{Loc.Localize("OverlayVisibleByDefault", "오버레이 항상 표시")}##overlayVisibleByDefault", ref this.Plugin.Configuration.OverlayVisibleByDefault);
            this._save |= ImGui.Checkbox($"{Loc.Localize("LockOverlaysConfig", "오버레이 잠그기")}##lockOverlays", ref this.Plugin.Configuration.EnableLockedOverlays);
            this._save |= ImGui.Checkbox($"{Loc.Localize("HideTitleBarConfig", "타이틀바 숨기기")}##hideTitlebar", ref this.Plugin.Configuration.HideTitlebar);
            this._save |= ImGui.Checkbox($"{Loc.Localize("EnableClickthroughConfig", "클릭 무시 활성화")}##enableWindowClickthrough", ref this.Plugin.Configuration.WindowClickThrough);
            if (this.Plugin.Configuration.WindowClickThrough)
            {
                ImGui.Indent();
                this._save |= ImGui.Checkbox($"{Loc.Localize("TabBarClickthroughConfig", "탭 바 클릭 무시 활성화")}##enableTabBarClickthrough", ref this.Plugin.Configuration.TabBarClickThrough);
                this._save |= ImGui.Checkbox($"{Loc.Localize("ListClickthroughConfig", "목록 클릭 무시 활성화")}##enableListClickthrough", ref this.Plugin.Configuration.ListClickThrough);
                ImGui.Unindent();
            }

            SonarImGui.Combo($"{Loc.Localize("SortingMode", "정렬 모드")}", (int)this.Plugin.Configuration.SortingMode, EnumCheapLocExtensions.CheapLoc<RelayStateSortingMode>().Values.ToArray(), value =>
            {
                this._save = true;
                this.Plugin.Configuration.SortingMode = (RelayStateSortingMode)value;
            });

            var index = this.jurisdictionsCombo.Keys.ToList().IndexOf(this.Client.Configuration.Contribute.ReceiveJurisdiction);
            if (ImGui.Combo($"{Loc.Localize("ReceiveJurisdiction", "수신 관할구역")}", ref index, this.jurisdictionsCombo.Values.ToArray(), this.jurisdictionsCombo.Count))
            {
                this._save = this._server = true;
                if (index == 0) index = 5;
                this.Client.Configuration.Contribute.ReceiveJurisdiction = this.jurisdictionsCombo.Keys.ToList()[index];
            }

            this._save |= ImGui.SliderFloat($"{Loc.Localize("OpacityConfig", "투명도")}##opacitySlider", ref this.Plugin.Configuration.Opacity, 0.0f, 1.0f);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.SliderFloat($"{Loc.Localize("AlertVolumeConfig", "알림음 음량")}##volumeSlider", ref this.Plugin.Configuration.SoundVolume, 0.0f, 1.0f))
            {
                this.Audio.Volume = this.Plugin.Configuration.SoundVolume;
                this._save = true;
            }

            // TODO: Add language combo when we add resource files and change text

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##generalTabDutySettings", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("DutySettings", "임무")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.Checkbox($"{Loc.Localize("DisableChatAlertsDuties", "임무 중 메시지 알림 비활성화")}##enableWindowClickthrough", ref this.Plugin.Configuration.DisableChatInDuty);
                this._save |= ImGui.Checkbox($"{Loc.Localize("DisableSoundAlertsDuties", "임무 중 소리 알림 비활성화")}##enableWindowClickthrough", ref this.Plugin.Configuration.DisableSoundInDuty);

                ImGui.Unindent();
            }

            if (ImGui.TreeNodeEx("##generalTabClicks", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ClickSettings", "클릭")}"))
            {
                ImGui.Indent();

                var actions = EnumCheapLocExtensions.CheapLoc<ClickAction>().Values.ToArray();
                SonarImGui.Combo($"{Loc.Localize("MiddleClick", "휠클릭")}##middleClickConfig", (int)this.Plugin.Configuration.MiddleClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 2;
                    this.Plugin.Configuration.MiddleClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                SonarImGui.Combo($"{Loc.Localize("RightClick", "우클릭")}##rightClickConfig", (int)this.Plugin.Configuration.RightClick, actions, index =>
                {
                    this._save = true;
                    if (index == 0) index = 3;
                    this.Plugin.Configuration.RightClick = EnumCheapLocExtensions.CheapLoc<ClickAction>().Keys.ToArray()[index];
                });

                ImGui.Spacing();

                var cityStates = Enum.GetValues<CityState>().Select(cityState => (cityState, cityState.GetMeta())).ToArray();
                var cityStateStrings = cityStates.Select(ct => ct.Item2?.GetZone()?.ToString() ?? string.Empty).ToArray();
                SonarImGui.Combo($"{Loc.Localize("PreferredCityState", "선호하는 도시국가")}##preferredCityState", (int)this.Plugin.Configuration.PreferredCityState, cityStateStrings, index =>
                {
                    this._save = true;
                    this.Plugin.Configuration.PreferredCityState = cityStates[index].cityState;
                });

                ImGui.Unindent();
            }

            if (ImGui.TreeNodeEx("##generalTabLodestone", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("LodestoneSettings", "로드스톤 인증")}"))
            {
                ImGui.Indent();
                var suppressions = EnumCheapLocExtensions.CheapLoc<SuppressVerification>().Values.ToArray();
                SonarImGui.Combo($"{Loc.Localize("SuppressVerification", "인증 요청 알림")}##suppressVerification", (int)this.Plugin.Configuration.SuppressVerification, suppressions, index =>
                {
                    this._save = true;
                    this.Plugin.Configuration.SuppressVerification = EnumCheapLocExtensions.CheapLoc<SuppressVerification>().Keys.ToArray()[index];
                });

                ImGui.Unindent();
            }

            if (ImGui.TreeNodeEx("##generalTabColorScheme", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ColorScheme", "Sonar 색상 스타일")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.ColorEdit4(Loc.Localize("HuntHealthyColor", "마물 - 생존"), ref this.Plugin.Configuration.Colors.HuntHealthy, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("HuntPulledColor", "마물 - 토벌 중"), ref this.Plugin.Configuration.Colors.HuntPulled, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("HuntDeadColor", "마물 - 토벌 완료"), ref this.Plugin.Configuration.Colors.HuntDead, ImGuiColorEditFlags.NoInputs);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                this._save |= ImGui.ColorEdit4(Loc.Localize("FateRunningColor", "돌발 - 진행 중 (0%)"), ref this.Plugin.Configuration.Colors.FateRunning, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateProgressColor", "돌발 - 진행 중"), ref this.Plugin.Configuration.Colors.FateProgress, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateCompleteColor", "돌발 - 완료"), ref this.Plugin.Configuration.Colors.FateComplete, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateFailedColor", "돌발 - 실패"), ref this.Plugin.Configuration.Colors.FateFailed, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FatePreparationColor", "돌발 - 준비 중"), ref this.Plugin.Configuration.Colors.FatePreparation, ImGuiColorEditFlags.NoInputs);
                this._save |= ImGui.ColorEdit4(Loc.Localize("FateUnknownColor", "돌발 - 알 수 없음"), ref this.Plugin.Configuration.Colors.FateUnknown, ImGuiColorEditFlags.NoInputs);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.TextUnformatted(Loc.Localize("ColorSchemePresets", "색상 스타일 설정값"));

                if (ImGui.Button(Loc.Localize("ColorSchemeDefault", "기본 스타일")))
                {
                    this._save = true;
                    this.Plugin.Configuration.Colors.SetDefaults();
                }

                ImGui.SameLine();
                if (ImGui.Button(Loc.Localize("ColorSchemeDefault", "초기 스타일")))
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
            { HuntRank.None, $"{Loc.Localize("NoRankText", "없음")}" },
            { HuntRank.B, $"{Loc.Localize("RankBText", "B급")}" },
            { HuntRank.A, $"{Loc.Localize("RankAText", "A급")}" },
            { HuntRank.S, $"{Loc.Localize("RankSText", "S급")}" },
            { HuntRank.SSMinion, $"{Loc.Localize("RankSSMinionText", "SS급 부하")}" },
            { HuntRank.SS, $"{Loc.Localize("RankSSText", "SS급")}" },
        };

        private void DrawHuntTab()
        {
            ImGui.BeginChild("##huntTab2ScrollRegion");

            SonarImGui.Checkbox($"{Loc.Localize("ContributeHunts", "마물 전파 기여")}{(this.Client.Configuration.Contribute.Global ? "" : " (Disabled)")}##contributeHunts", this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Hunt], value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Hunt] = value;
            });
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("ContributeHuntsTooltip", "다른 Sonar 유저들로부터 전파 정보를 수신하려면 마물 전파 기여 활성화를 요구합니다.\n기여를 비활성화할 경우 로컬 모드로 작동하고 자신이 직접 발견한 항목만 확인할 수 있으며 다른 유저들로부터 전파 정보를 수신할 수 없습니다.")}");
            }
            SonarImGui.Checkbox($"{Loc.Localize("TrackAllHunts", "모든 마물 추적")}##trackAllHunts", this.Client.Configuration.HuntConfig.TrackAll, value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.HuntConfig.TrackAll = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("TrackAllHuntsTooltip", "체크: 관할구역 설정에 상관없이 모든 마물을 추적합니다.\n미체크: 설정한 관할구역에 포함되는 마물만 추적합니다.")}");
            }

            if (ImGui.TreeNodeEx("##huntTabChatConfig", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("ConfigureHuntChat", "마물 전파 메시지 알림")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnabledConfig", "대화 창에 표시")}##huntChatEnabled", ref this.Plugin.Configuration.EnableGameChatReports);
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

                    this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnableItalic", "기울임체 활성화")}##huntChatEnableItalic", ref this.Plugin.Configuration.EnableGameChatItalicFont);
                    this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnableCrosswordIcon", "타 서버 아이콘 활성화")}##huntChatEnableCrossworldIcon", ref this.Plugin.Configuration.EnableGameChatCrossworldIcon);
                    this._save |= ImGui.Checkbox($"{Loc.Localize("HuntChatEnableDeathChat", "토벌 완료 메시지 활성화")}##huntChatEnableDeaths", ref this.Plugin.Configuration.EnableGameChatReportsDeaths);
                    ImGui.Unindent();
                }

                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##huntTabSoundAlertConfig", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("ConfigureHuntSounds", "마물 전파 소리 알림")}"))
            {
                ImGui.Indent();

                this._save |= ImGui.Checkbox($"{Loc.Localize("PlaySoundSConfig", "S급 소리 알림 활성화")}##sRankSoundEnabled", ref this.Plugin.Configuration.PlaySoundSRanks);
                if (this.Plugin.Configuration.PlaySoundSRanks)
                {
                    ImGui.Indent();
                    var selectedSound = Array.IndexOf(this.audioFilesForSRanks, this.Plugin.Configuration.SoundFileSRanks);
                    if (ImGui.Combo("##sRankSounds", ref selectedSound, this.audioFilesForSRanks, this.audioFilesForSRanks.Length))
                    {
                        if (selectedSound == this.audioFilesForSRanks.Length - 1)
                        {
                            this.FileDialogs.OpenFileDialog(Loc.Localize("SelectSRankSoundTitle", "S급 소리 파일 선택"), "Sound Files{.3g2,.3gp,.3gp2,.3gpp,.asf,.wma,.wmv,.aac,.adts,.avi,.mp3,.m4a,.m4v,.mov,.mp4,.sami,.smi,.wav}", (success, filename) =>
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

                this._save |= ImGui.Checkbox($"{Loc.Localize("PlaySoundAConfig", "A급 소리 알림 활성화")}##aRankSoundEnabled", ref this.Plugin.Configuration.PlaySoundARanks);
                if (this.Plugin.Configuration.PlaySoundARanks)
                {
                    ImGui.Indent();
                    var selectedSound = Array.IndexOf(this.audioFilesForARanks, this.Plugin.Configuration.SoundFileARanks);
                    if (ImGui.Combo("##aRankSounds", ref selectedSound, this.audioFilesForARanks, this.audioFilesForARanks.Length))
                    {
                        if (selectedSound == this.audioFilesForARanks.Length - 1)
                        {
                            this.FileDialogs.OpenFileDialog(Loc.Localize("SelectARankSoundTitle", "A급 소리 파일 선택"), "Sound Files{.3g2,.3gp,.3gp2,.3gpp,.asf,.wma,.wmv,.aac,.adts,.avi,.mp3,.m4a,.m4v,.mov,.mp4,.sami,.smi,.wav}", (success, filename) =>
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

            if (ImGui.TreeNodeEx("##huntTabReportNotifications", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("HuntNotifications", "마물 전파 알림")}"))
            {
                ImGui.Indent();
                SonarImGui.Checkbox($"{Loc.Localize("AllSRankSettings", "SS급, S급 구분")}##allsranks", this.Plugin.Configuration.AllSRankSettings, value =>
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

                SonarImGui.Checkbox($"{Loc.Localize("AdvancedHuntReportingSettings", "확장팩 단위")}##advanced", this.Plugin.Configuration.AdvancedHuntReportSettings, value =>
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

                SonarImGui.Combo($"{Loc.Localize("SSMinionNotificationMode", "SS 미니언 알림 모드")}", (int)this.Plugin.Configuration.SSMinionReportingMode, EnumCheapLocExtensions.CheapLoc<NotifyMode>().Values.ToArray(), value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.SSMinionReportingMode = (NotifyMode)value;
                });
                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##huntTabDisplayTimers", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("HuntDisplayTimers", "마물 표시 타이머")}"))
            {
                ImGui.Indent();

                ImGui.TextDisabled(Loc.Localize("DisplayTimersHelpText", "모든 타이머는 초 단위 입니다. CTRL+클릭으로 직접 입력."));
                ImGui.Spacing();

                SonarImGui.SliderInt($"{Loc.Localize("HuntDeadTimer", "토벌 완료로부터 지난 시간 (S, A)")}###huntDeadTimer", this.Plugin.Configuration.DisplayHuntDeadTimer, 0, 604800, value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.DisplayHuntDeadTimer = value;
                });

                SonarImGui.SliderInt($"{Loc.Localize("HuntUpdateTimer", "마지막 갱신으로부터 지난 시간 (S, A)")}###huntUpdateTimer", this.Plugin.Configuration.DisplayHuntUpdateTimer, 60, 604800, value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.DisplayHuntUpdateTimer = value;
                });

                SonarImGui.SliderInt($"{Loc.Localize("HuntUpdateOtherTimer", "마지막 갱신으로부터 지난 시간 (B)")}###huntUpdateOtherTimer", this.Plugin.Configuration.DisplayHuntUpdateTimerOther, 60, 604800, value =>
                {
                    this._save = true;
                    this.Plugin.Configuration.DisplayHuntUpdateTimerOther = value;
                });

                SonarImGui.SliderInt($"{Loc.Localize("HuntsDisplayLimit", "마물 목록 개수 제한")}###huntDisplayLimit", this.Plugin.Configuration.HuntsDisplayLimit, 1, 10000, value =>
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
            { ExpansionPack.Unknown , $"{Loc.Localize("UnknownText", "알 수 없음")}" },
            { ExpansionPack.ARealmReborn, $"{Loc.Localize("ARealmRebornText", "신생 에오르제아")}" },
            { ExpansionPack.Heavensward, $"{Loc.Localize("HeavenswardText", "창천의 이슈가르드")}" },
            { ExpansionPack.Stormblood, $"{Loc.Localize("StormbloodText", "홍련의 해방자")}" },
            { ExpansionPack.Shadowbringers, $"{Loc.Localize("ShadowbringersText", "칠흑의 반역자")}" },
            { ExpansionPack.Endwalker, $"{Loc.Localize("EndwalkerText", "효월의 종언")}" },
            { ExpansionPack.Dawntrail, $"{Loc.Localize("DawntrailText", "황금의 유산")}" },
        };

        private readonly Dictionary<SonarJurisdiction, string> jurisdictionsCombo = new Dictionary<SonarJurisdiction, string>()
        {
            { SonarJurisdiction.Default, $"{Loc.Localize("DefaultJurisdictionText", "기본값")}" },
            { SonarJurisdiction.None, $"{Loc.Localize("NoneJurisdictionText", "없음")}" },
            { SonarJurisdiction.Instance, $"{Loc.Localize("InstanceJurisdictionText", "인스턴스")}" },
            { SonarJurisdiction.Zone, $"{Loc.Localize("ZoneJurisdictionText", "지역")}" },
            { SonarJurisdiction.World, $"{Loc.Localize("WorldJurisdictionText", "서버")}" },
            { SonarJurisdiction.Datacenter, $"{Loc.Localize("DatacenterJurisdictionText", "데이터 센터")}" },
            { SonarJurisdiction.Region, $"{Loc.Localize("RegionJurisdictionText", "국가")}" },
            { SonarJurisdiction.Audience, $"{Loc.Localize("AudienceJurisdictionText", "한국서버")}" },
            { SonarJurisdiction.All, $"{Loc.Localize("AllJurisdictionText", "전체")}" },
        };

        private readonly Dictionary<SonarLogLevel, string> logLevelCombo = new()
        {
            { SonarLogLevel.Verbose, $"{Loc.Localize("SonarLogLevelVerbose", "상세히")}"},
            { SonarLogLevel.Debug, $"{Loc.Localize("SonarLogLevelDebug", "디버그")}" },
            { SonarLogLevel.Information, $"{Loc.Localize("SonarLogLevelInformation", "정보")}" },
            { SonarLogLevel.Warning, $"{Loc.Localize("SonarLogLevelWarning", "경고")}" },
            { SonarLogLevel.Error, $"{Loc.Localize("SonarLogLevelError", "오류")}" },
            { SonarLogLevel.Fatal, $"{Loc.Localize("SonarLogLevelFatal", "치명적")}" },
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

            SonarImGui.Checkbox($"{Loc.Localize("ContributeFates", "돌발 전파 기여")}{(this.Client.Configuration.Contribute.Global ? "" : " (Disabled)")}##contributeFates", this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Fate], value =>
            {
                this._save = this._server = true;
                this.Client.Configuration.Contribute[Sonar.Relays.RelayType.Fate] = value;
            });
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("ContributeHuntsTooltip", "다른 Sonar 유저들로부터 전파 정보를 수신하려면 마물 돌발 기여 활성화를 요구합니다.\n기여를 비활성화할 경우 로컬 모드로 작동하고 자신이 직접 발견한 항목만 확인할 수 있으며 다른 유저들로부터 전파 정보를 수신할 수 없습니다.")}");
            }

            SonarImGui.Checkbox($"{Loc.Localize("TrackAllFates", "모든 돌발 추적")}##trackAllFates", this.Client.Configuration.FateConfig.TrackAll, value =>
            {
                this.Plugin.SaveConfiguration(true);
                this.Client.Configuration.FateConfig.TrackAll = value;
            });

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{Loc.Localize("TrackAllFatesTooltip", "체크: 관할구역 설정에 상관없이 모든 마물을 추적합니다.\n미체크: 설정한 관할구역에 포함되는 마물만 추적합니다.")}");
            }

            if (ImGui.TreeNodeEx("##fateTabChatConfig", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ConfigureFateChat", "돌발 전파 메시지 알림")}"))
            {
                ImGui.Indent();
                if (ImGui.Checkbox($"{Loc.Localize("FateChatEnabledConfig", "대화 창에 표시")}##fateChatEnabled", ref this.Plugin.Configuration.EnableFateChatReports))
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

                    ImGui.Checkbox($"{Loc.Localize("FateChatEnableItalic", "기울임체 활성화")}##fateChatEnableItalic", ref this.Plugin.Configuration.EnableFateChatItalicFont);
                    ImGui.Checkbox($"{Loc.Localize("FateChatEnableCrosswordIcon", "타 서버 아이콘 활성화")}##fateChatEnableCrossworldIcon", ref this.Plugin.Configuration.EnableFateChatCrossworldIcon);

                    ImGui.Unindent();
                }

                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##fateTabSoundAlertConfig", ImGuiTreeNodeFlags.CollapsingHeader, $"{Loc.Localize("ConfigureFateSounds", "돌발 전파 소리 알림")}"))
            {
                ImGui.Indent();

                ImGui.Checkbox($"{Loc.Localize("PlaySoundFateConfig", "돌발 소리 알림 활성화")}##fateSoundEnabled", ref this.Plugin.Configuration.PlaySoundFates);
                if (this.Plugin.Configuration.PlaySoundFates)
                {
                    ImGui.Indent();
                    int selectedSound = Array.IndexOf(this.audioFilesForFates, this.Plugin.Configuration.SoundFileFates);
                    if (ImGui.Combo("##fateSounds", ref selectedSound, this.audioFilesForFates, this.audioFilesForFates.Length))
                    {
                        if (selectedSound == this.audioFilesForFates.Length - 1)
                        {
                            this.FileDialogs.OpenFileDialog(Loc.Localize("SelectFateSoundTitle", "돌발 소리 파일 선택"), "Sound Files{.3g2,.3gp,.3gp2,.3gpp,.asf,.wma,.wmv,.aac,.adts,.avi,.mp3,.m4a,.m4v,.mov,.mp4,.sami,.smi,.wav}", (success, filename) =>
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

            if (ImGui.TreeNodeEx("##fateTabDisplayTimers", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("FateDisplayTimers", "돌발 표시 타이머")}"))
            {
                ImGui.Indent();

                ImGui.TextDisabled(Loc.Localize("DisplayTimersHelpText", "모든 타이머는 초 단위 입니다. CTRL+클릭으로 직접 입력."));
                ImGui.Spacing();

                this._save |= ImGui.SliderInt($"{Loc.Localize("FateDeadTimer", "성공 및 실패로부터 지난 시간")}###fateDeadTimer", ref this.Plugin.Configuration.DisplayFateDeadTimer, 0, 604800);
                this._save |= ImGui.SliderInt($"{Loc.Localize("FateUpdateTimer", "마지막 갱신으로부터 지난 시간")}###fateUpdateTimer", ref this.Plugin.Configuration.DisplayFateUpdateTimer, 60, 604800);
                this._save |= ImGui.SliderInt($"{Loc.Localize("FatesDisplayLimit", "돌발 목록 개수 제한")}###fateDisplayLimit", ref this.Plugin.Configuration.FatesDisplayLimit, 1, 10000);

                ImGui.Unindent();
            }

            ImGui.Spacing();

            if (ImGui.TreeNodeEx("##fateSelection", ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen, $"{Loc.Localize("FateSelection", "돌발 선택")}"))
            {
                ImGui.Indent();

                ImGui.Text(Loc.Localize("DefaultReportJurisdiction", "기본 전파 관할구역")); ImGui.SameLine();
                int index = this.jurisdictionsCombo.Keys.ToList().IndexOf(this.Client.Configuration.FateConfig.GetDefaultJurisdiction());

                if (ImGui.Combo($"##fate_default", ref index, this.jurisdictionsCombo.Values.ToArray(), this.jurisdictionsCombo.Count))
                {
                    this._save = this._server = true;
                    this.Client.Configuration.FateConfig.SetDefaultJurisdiction(this.jurisdictionsCombo.Keys.ToList()[index]);
                }

                ImGui.Text(Loc.Localize("SearchText", "검색"));
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
                ImGui.Text(string.Format(Loc.Localize("SelectedFatesToChatText", "돌발 {0}개 메시지 알림"), selectedFates));
                ImGui.SameLine();
                ImGui.Text(" | ");
                ImGui.SameLine();
                selectedFates = this.Plugin.Configuration.SendFateToSound
                    .Select(id => Database.Fates.GetValueOrDefault(id))
                    .Where(fate => fate is not null)
                    .DistinctBy(fate => fate!.GroupId)
                    .Count();
                ImGui.Text(string.Format(Loc.Localize("SelectedFatesToSoundText", "돌발 {0}개 소리 알림"), selectedFates));
                ImGui.SameLine();
                ImGui.Text(" | ");
                ImGui.SameLine();
                selectedFates = this.Client.Configuration.FateConfig.GetJurisdictions()
                    .Where(kvp => kvp.Value != SonarJurisdiction.Default)
                    .Select(kvp => Database.Fates.GetValueOrDefault(kvp.Key))
                    .Where(fate => fate is not null)
                    .DistinctBy(fate => fate!.GroupId)
                    .Count();
                ImGui.Text(string.Format(Loc.Localize("SelectedFatesToNonDefaultJurisdiction", "돌발 {0}개가 기본 전파 관할구역을 사용하지 않음"), selectedFates));


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
                    ImGui.TableSetupColumn(Loc.Localize("FateTableChatColumn", "메시지"), ImGuiTableColumnFlags.PreferSortDescending, 0.0f, (int)FateSelectionColumns.Chat);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableSoundColumn", "소리"), ImGuiTableColumnFlags.PreferSortDescending, 0.0f, (int)FateSelectionColumns.Sound);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableJurisdictionColumn", "관할구역"), ImGuiTableColumnFlags.NoHide, 50.0f, (int)FateSelectionColumns.Jurisdiction);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableLevelColumn", "레벨"), ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed, 0.0f, (int)FateSelectionColumns.Level);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableNameColumn", "이름"), ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.NoHide, 0.0f, (int)FateSelectionColumns.Name);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableZoneColumn", "지역"), ImGuiTableColumnFlags.None, 0.0f, (int)FateSelectionColumns.Zone);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableExpansionColumn", "확장팩"), ImGuiTableColumnFlags.DefaultHide, 0.0f, (int)FateSelectionColumns.Expansion);
                    ImGui.TableSetupColumn(Loc.Localize("FateTableAchievementColumn", "업적"), ImGuiTableColumnFlags.DefaultHide, 0.0f, (int)FateSelectionColumns.AchievementName);
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
                ImGui.Text($"{Loc.Localize("AboutSonarBroughtBy", "Sonar 팀이 제공합니다")}");

                if (ImGui.Button("Sonar Support Discord##SonarDiscord"))
                {
                    this._tasker.AddTask(Task.Run(() => { ShellExecute("https://discord.gg/K7y24Rr"); }));
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("AboutSonarSupport", "Sonar Support Discord 서버에서 질문 또는 버그 신고를 해주세요.\ngoat place's discord 서버에서는 Sonar Support가 제공되지 않습니다.")}");

                ImGui.SameLine();

                if (ImGui.Button("Sonar Patreon##SonarPatreon"))
                {
                    this._tasker.AddTask(Task.Run(() => { ShellExecute("https://www.patreon.com/ffxivsonar"); }));
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("AboutSonarPatreon", "Patreon에서 Sonar를 지원해 주세요.\n지원금은 Sonar 서버와 Discord 봇 운영에 사용됩니다.")}");
                // Earnings will be used for the Sonar server and discord bots hosting costs.
                // Earnings will be used for the Sonar Server and Discord Bot hosting costs.
                // Earnings will be used for the hosting costs of the Sonar Server and Discord Bots.

                ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

                ImGui.Text($"{Loc.Localize("SonarStatus", "Sonar 상태")}: "); ImGui.SameLine();
                if (this.Client.Connection.IsConnected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ff33);
                    ImGui.Text($"{Loc.Localize("SonarConnected", "연결됨")}");
                    ImGui.PopStyleColor();
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{this.Client.Ping:F0}ms");
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff0099ff);
                    ImGui.Text($"{Loc.Localize("SonarDisconnected", "연결 끊김")}");
                    ImGui.PopStyleColor();
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("SonarDisconnectedTooltip", "Sonar는 자동으로 재연결을 시도합니다")}");
                }

                ImGui.SameLine();
                if (ImGui.Button($"{Loc.Localize("SonarReconnect", "재연결")}"))
                {
                    this.Client.Connection.Reconnect();
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{Loc.Localize("SonarReconnectTooltip", "Sonar 서버 재연결을 시도합니다")}");
            }

            ImGui.EndChild(); // End scroll region
        }

        private bool _showIdentifier;
        private void DrawDebugTab()
        {
            ImGui.BeginChild("##debugTabScrollRegion");
            {
                ImGui.Text("버전 정보");
                ImGui.BeginChild("##debugVersionInfo", new Vector2(0, 100 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"{this.Stub.PluginName} v{Assembly.GetExecutingAssembly().GetName().Version}");
                    ImGui.Text($"Dalamud {VersionUtils.GetDalamudVersion()} (Git: {VersionUtils.GetDalamudBuild()})");
                    ImGui.Text($"FFXIV {VersionUtils.GetGameVersion(this.Data)}");

                    ImGui.Text($"클라이언트 해시: ");
                    ImGui.SameLine();
                    if (this._showIdentifier)
                    {
                        ImGui.Text($"{this.Client.ClientHash ?? "Unknown"}");
                        ImGui.SameLine();
                    }

                    if (ImGui.Button($"{(this._showIdentifier ? "가리기" : "표시")}")) this._showIdentifier = !this._showIdentifier;
                    ImGui.SameLine();
                    if (ImGui.Button($"복사")) ImGui.SetClipboardText(this.Client.ClientHash);
                }
                ImGui.EndChild(); // debugVersionInfo

                ImGui.Spacing();

                ImGui.Text("플레이어 트래커");
                ImGui.BeginChild("##DebugPlayerTracker", new Vector2(0, 35 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"지역: {this.Client.Meta.PlayerPosition}");
                }
                ImGui.EndChild(); // debugPlayerTracker
                ImGui.Spacing();

                ImGui.Text("마물 트래커");
                ImGui.BeginChild("##DebugHuntTracker", new Vector2(0, 80 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"개수: {this.Client.Trackers.Hunts.Data.Count} | 색인: {this.Client.Trackers.Hunts.Data.IndexCount}");

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    if (this._debugHuntTask.IsCompleted)
                    {
                        if (ImGui.Button("정리"))
                        {
                            this.Client.Trackers.Hunts.Data.Clear();
                            this.Logger.Information("마물 트래커 정리");
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("확인"))
                        {
                            this._debugHuntTask = Task.Run(() =>
                            {
                                this.Logger.Information("마물 색인 일관성 확인 시작");
                                var result = this.Client.Trackers.Hunts.Data.DebugIndexConsistencyCheck();
                                if (!result.Any())
                                {
                                    this.Logger.Information($"마물 색인 일관성 확인 완료");
                                }
                                else
                                {
                                    this.Logger.Warning($"마물 색인 일관성 확인 실패!\n{string.Join("\n", result)}");
                                }
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("마물 색인 일관성 확인\n결과는 /xllog 에 출력됩니다");
                        ImGui.SameLine();
                        if (ImGui.Button("다시 빌드"))
                        {
                            this._debugHuntTask = Task.Run(() =>
                            {
                                this.Logger.Information("마물 색인 다시 빌드 시작");
                                this.Client.Trackers.Hunts.Data.RebuildIndex();
                                this.Logger.Information("마물 색인 다시 빌드 완료");
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("마물 색인 다시 빌드\n결과는 /xllog 에 출력됩니다\n\n경고: 끊김 현상이 있을 수 있습니다");
                    }
                    else
                    {
                        ImGui.Text("Busy");
                    }
                }
                ImGui.EndChild(); // debugHuntTracker
                ImGui.Spacing();

                ImGui.Text("돌발 트래커");
                ImGui.BeginChild("##DebugFateTracker", new Vector2(0, 80 * ImGui.GetIO().FontGlobalScale), true, ImGuiWindowFlags.None);
                {
                    ImGui.Text($"개수: {this.Client.Trackers.Fates.Data.Count} | 색인: {this.Client.Trackers.Fates.Data.IndexCount}");

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    if (this._debugFateTask.IsCompleted)
                    {
                        if (ImGui.Button("정리"))
                        {
                            this.Client.Trackers.Fates.Data.Clear();
                            this.Logger.Information("돌발 트래커 정리");
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("확인"))
                        {
                            this._debugFateTask = Task.Run(() =>
                            {
                                this.Logger.Information("돌발 색인 일관성 확인 시작");
                                var result = this.Client.Trackers.Fates.Data.DebugIndexConsistencyCheck();
                                if (!result.Any())
                                {
                                    this.Logger.Information($"돌발 색인 일관성 확인 성공");
                                }
                                else
                                {
                                    this.Logger.Warning($"돌발 색인 일관성 확인 실패!\n{string.Join("\n", result)}");
                                }
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("돌발 색인 일관성 확인\n결과는 /xllog 에 출력됩니다");
                        ImGui.SameLine();
                        if (ImGui.Button("다시 빌드"))
                        {
                            this._debugFateTask = Task.Run(() =>
                            {
                                this.Logger.Information("돌발 색인 다시 빌드 시작");
                                this.Client.Trackers.Fates.Data.RebuildIndex();
                                this.Logger.Information("돌발 색인 다시 빌드 완료");
                            });
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip("돌발 색인 다시 빌드\n결과는 /xllog 에 출력됩니다\n\n경고: 끊김 현상이 있을 수 있습니다");
                    }
                    else
                    {
                        ImGui.Text("Busy");
                    }
                }
                ImGui.EndChild(); // debugFateTracker
                ImGui.Spacing();

                if (ImGui.Button("릴레이 데이터 요청"))
                {
                    this.Plugin.Configuration.SonarConfig.HuntConfig.TrackAll = true;
                    this.Plugin.Configuration.SonarConfig.FateConfig.TrackAll = true;
                    this.Plugin.SaveConfiguration();
                    this.Client.RequestRelayData();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("모든 릴레이 정보를 받도록 요청합니다.\n모든 마물 및 돌발 추적 옵션이 활성화 됩니다.\n이 기능은 단 한번만 작동합니다.\n\n경고: Sonar 서버가 알고있는 모든 정보를 받게 됩니다!\n이 기능은 현재 시험 중 입니다.");
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            var index = this.logLevelCombo.Keys.ToList().IndexOf(this.Client.Configuration.LogLevel);
            if (ImGui.Combo($"{Loc.Localize("LogLevel", "로그 단계")}", ref index, this.logLevelCombo.Values.ToArray(), this.logLevelCombo.Count))
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
