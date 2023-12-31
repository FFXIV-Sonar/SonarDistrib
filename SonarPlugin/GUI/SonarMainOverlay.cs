using System.Diagnostics;
using CheapLoc;
using SonarPlugin.Config;
using Dalamud.Interface;
using ImGuiNET;
using ImGuiScene;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SonarPlugin.Utility;
using static Sonar.Utilities.UnixTimeHelper;
using static Sonar.SonarConstants;
using Sonar;
using System.Threading;
using SonarPlugin.Game;
using SonarPlugin.Trackers;
using Sonar.Trackers;
using SonarPlugin.Notifiers;
using System.Threading.Tasks;
using Dalamud.Logging;
using Dalamud.Game.Gui;
using Sonar.Relays;
using Sonar.Indexes;
using Sonar.Utilities;
using Dalamud.Plugin.Services;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Interface.Internal;
using SonarPlugin.Managers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace SonarPlugin.GUI
{
    public sealed class SonarMainOverlay : IHostedService, IDisposable
    {
        public string WindowTitle => windowTitle;
        private bool _visible; // used as ref

        [SuppressMessage("Minor Code Smell", "S2292", Justification = "Backing field is used with ref")]
        public bool IsVisible
        {
            get => this._visible;
            set => this._visible = value;
        }

        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private RelayTrackerViews Views { get; }
        private HuntNotifier HuntNotifier { get; }
        private FateNotifier FateNotifier { get; }
        private AetheryteManager Aetherytes { get; }
        private MapTextureProvider MapTextures { get; }
        private ResourceHelper Resources { get; }
        private UiBuilder Ui { get; }
        private IGameGui GameGui { get; }
        private IFramework Framework { get; }
        private IPluginLog Logger { get; }

        private static readonly string windowTitle = Loc.Localize("MainWindowTitle", "Sonar");
        private static Vector2 minimumWindowSize = new(300, 100);
        private static Vector2 maximumWindowSize = new(float.MaxValue, float.MaxValue);
        private static Vector2 mapSize = new(300, 300);
        private static Vector2 iconSize = new(16, 16);
        private static readonly float detailLabelOffset = 100.0f;

        private readonly IDalamudTextureWrap _redFlag;

        public SonarMainOverlay(SonarPlugin plugin, SonarClient client, RelayTrackerViews views, HuntNotifier huntsNotifier, FateNotifier fateNotifier, AetheryteManager aetherytes, MapTextureProvider mapTextures, ResourceHelper resources, UiBuilder ui, IGameGui gameGui, IFramework framework, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.Views = views;
            this.HuntNotifier = huntsNotifier;
            this.FateNotifier = fateNotifier;
            this.Aetherytes = aetherytes;
            this.MapTextures = mapTextures;
            this.Resources = resources;
            this.Ui = ui;
            this.GameGui = gameGui;
            this.Framework = framework;
            this.Logger = logger;

            this._visible = this.Plugin.Configuration.OverlayVisibleByDefault;
            this._redFlag = this.Resources.LoadIcon("redflag.png");

            this.Logger.Information("Sonar Main Overlay Initialized");
        }

        private ImGuiWindowFlags WindowFlags
        {
            get
            {
                var ret = ImGuiWindowFlags.None;
                if (this.Plugin.Configuration.WindowClickThrough) ret |= ImGuiWindowFlags.NoInputs;
                if (this.Plugin.Configuration.EnableLockedOverlays) ret |= ImGuiWindowFlags.NoMove;
                if (this.Plugin.Configuration.HideTitlebar) ret |= ImGuiWindowFlags.NoTitleBar;
                return ret;
            }
        }

        private ImGuiWindowFlags TabBarFlags
        {
            get
            {
                var ret = ImGuiWindowFlags.None;
                if (this.Plugin.Configuration.TabBarClickThrough && this.Plugin.Configuration.WindowClickThrough) ret |= ImGuiWindowFlags.NoInputs;
                if (this.Plugin.Configuration.EnableLockedOverlays) ret |= ImGuiWindowFlags.NoMove;
                return ret;
            }
        }

        private ImGuiWindowFlags ListFlags
        {
            get
            {
                var ret = ImGuiWindowFlags.None;
                if (this.Plugin.Configuration.ListClickThrough && this.Plugin.Configuration.WindowClickThrough) ret |= ImGuiWindowFlags.NoInputs;
                if (this.Plugin.Configuration.EnableLockedOverlays) ret |= ImGuiWindowFlags.NoMove;
                return ret;
            }
        }

        public void Draw()
        {
            if (!this.IsVisible) return;
            ImGui.SetNextWindowSize(minimumWindowSize * ImGui.GetIO().FontGlobalScale, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(minimumWindowSize * ImGui.GetIO().FontGlobalScale, maximumWindowSize * ImGui.GetIO().FontGlobalScale);

            ImGui.SetNextWindowBgAlpha(this.Plugin.Configuration.Opacity);

            // Setup flags for window based on config settings
            var windowFlags = ImGuiWindowFlags.HorizontalScrollbar | this.WindowFlags;

            if (ImGui.Begin(windowTitle, ref this._visible, windowFlags))
            {
                ImGui.BeginChild("##SonarTabBarWindow", (new Vector2(0, 24)) * ImGui.GetIO().FontGlobalScale, false, ImGuiWindowFlags.AlwaysAutoResize | this.TabBarFlags);
                if (ImGui.BeginTabBar("##SonarTabBar", ImGuiTabBarFlags.None))
                {
                    HuntRank[] ranks = (HuntRank[])(Enum.GetValues(typeof(HuntRank)));
                    foreach (HuntRank rank in ranks.Where(r => r != HuntRank.None).Reverse().Prepend(HuntRank.None))
                    {
                        if (!this.Plugin.Configuration.AllSRankSettings && (rank == HuntRank.SS || rank == HuntRank.SSMinion)) continue;

                        // TODO: Eventually localize these enum strings
                        var tabText = rank == HuntRank.None ? "All##SonarHuntAllTab" : $" {rank} ##SonarHuntRank{rank}Tab";
                        if (ImGui.BeginTabItem(tabText))
                        {
                            ImGui.EndTabItem();
                            this._huntRank = rank;
                            this._huntsVisible = true;
                            this._fatesVisible = rank == HuntRank.None;
                        }
                    }

                    if (ImGui.BeginTabItem("FATE##FateRelayTab"))
                    {
                        ImGui.EndTabItem();
                        this._huntsVisible = false;
                        this._fatesVisible = true;
                    }

                    ImGui.EndTabBar();
                }
                ImGui.EndChild();

                this.DrawTab();
            }

            ImGui.End();
        }

        private List<RelayState> _states = new();
        private readonly object _statesLock = new();
        private HuntRank _huntRank;
        private bool _huntsVisible;
        private bool _fatesVisible;
        private void Client_Tick(SonarClient source) => this.UpdateTrackers(false);
        private bool UpdateTrackers(bool force)
        {
            if (!this.IsVisible) return false;
            if (!Monitor.TryEnter(this._statesLock, force ? -1 : 0)) return false;

            try
            {
                var place = this.Client.Meta.PlayerPosition;
                var rank = this._huntRank;

                HuntRank[]? searchRanks = null;
                if (rank == HuntRank.S && !this.Plugin.Configuration.AllSRankSettings)
                {
                    searchRanks = new HuntRank[] { HuntRank.S, HuntRank.SSMinion, HuntRank.SS };
                }

#if DEBUG
                Stopwatch stopwatch = new();
                stopwatch.Start();
#endif
                var newStates = Enumerable.Empty<RelayState>();

                if (this._huntsVisible)
                {
                    var hunts = Enumerable.Empty<RelayState>();
                    if (searchRanks is not null) foreach (var r in searchRanks) hunts = hunts.Concat(this.Views.HuntsByRank[r].Data.States.Values);
                    else hunts = hunts.Concat(this.Views.HuntsByRank[rank].Data.States.Values);
                    hunts = hunts
                        .SortBy(this.Plugin.Configuration.SortingMode, place)
                        .Take(this.Plugin.Configuration.HuntsDisplayLimit);
                    newStates = newStates.Concat(hunts);
                }

                if (this._fatesVisible)
                {
                    var fates = this.Views.Fates.Data.States.Values
                        .SortBy(this.Plugin.Configuration.SortingMode, place)
                        .Take(this.Plugin.Configuration.FatesDisplayLimit);
                    newStates = newStates.Concat(fates);
                }

                this._states = newStates.SortBy(this.Plugin.Configuration.SortingMode, place)
                    .ToList(); // ToList() needs to stay
#if DEBUG
                stopwatch.Stop();
                this.Logger.Debug($"Tracker queries took {stopwatch.Elapsed.TotalMilliseconds}ms (Count: {this._states.Count})");
#endif
            }
            catch (Exception ex)
            {
                this.Logger.Error($"{ex}");
            }
            finally
            {
                Monitor.Exit(this._statesLock);
            }
            return true;
        }

        private void DrawTab()
        {
            var states = this._states;

            ImGui.BeginChild($"##{this._huntRank}TabScrollRegion", new Vector2(0, 0), false, this.ListFlags);

            foreach (var state in states.Where(s => s.IsAlive()))
            {
                switch (state)
                {
                    case RelayState<HuntRelay> huntState:
                        this.DrawHunt(huntState);
                        break;

                    case RelayState<FateRelay> fateState:
                        this.DrawFate(fateState);
                        break;
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            foreach (RelayState state in states.Where(s => s.IsDead()))
            {
                switch (state)
                {
                    case RelayState<HuntRelay> huntState:
                        this.DrawHunt(huntState);
                        break;

                    case RelayState<FateRelay> fateState:
                        this.DrawFate(fateState);
                        break;
                }
            }

            ImGui.EndChild();
        }

        private unsafe void PerformClickAction(RelayState state)
        {
            var action = ClickAction.None;
            if (ImGui.IsItemClicked(ImGuiMouseButton.Middle)) action = this.Plugin.Configuration.MiddleClick;
            else if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) action = this.Plugin.Configuration.RightClick;
            this.PerformClickAction(state, action);
        }

        private unsafe void PerformClickAction(RelayState state, ClickAction action)
        {
            var mapAgent = AgentMap.Instance();

            var relay = state.Relay;
            var zone = relay.GetZone();
            var zoneId = zone?.Id ?? 0;
            var mapId = zone?.MapId ?? 0;
            var coords = relay.Coords.SwapYZ();

            switch (action)
            {
                case ClickAction.Chat:
                    if (state is RelayState<HuntRelay> huntState) this.HuntNotifier.SendToChat(huntState);
                    else if (state is RelayState<FateRelay> fateState) this.FateNotifier.SendToChat(fateState);
                    break;

                case ClickAction.Map:
                    if (mapAgent is not null)
                    {
                        mapAgent->SetFlagMapMarker(zoneId, mapId, coords);
                        mapAgent->OpenMap(mapId, zoneId);
                    }
                    break;

                case ClickAction.Teleport:
                    if (mapAgent is not null) mapAgent->SetFlagMapMarker(zoneId, mapId, coords);
                    this.Aetherytes.TeleportToClosest(state.Relay, true);
                    break;
            }
        }

        private void DrawHunt(RelayState<HuntRelay> state)
        {
            //throw new Exception("Some error here");

            var relay = state.Relay;

            Vector4 statusColor;
            string statusText;
            string healthText = $"{100f * relay.CurrentHp / relay.MaxHp:F2}%%";

            if (relay.IsMaxHp)
            {
                statusText = Loc.Localize("HealthyText", $"Healthy ({healthText})");
                statusColor = this.Plugin.Configuration.Colors.HuntHealthy;
            }
            else if (!relay.IsDead())
            {
                statusText = Loc.Localize("PulledText", $"Pulled ({healthText})");
                statusColor = this.Plugin.Configuration.Colors.HuntPulled;
            }
            else
            {
                statusText = Loc.Localize("DeadText", "Dead");
                statusColor = this.Plugin.Configuration.Colors.HuntDead;
            }

            ImGui.PushStyleColor(ImGuiCol.Text, statusColor);
            bool isHeaderOpen = ImGui.TreeNodeEx($"##hunt_{relay.RelayKey}", ImGuiTreeNodeFlags.CollapsingHeader, $"{relay} [{relay.Players}]");
            ImGui.PopStyleColor();

            this.PerformClickAction(state);

            if (isHeaderOpen)
            {
                ImGui.BeginGroup(); // Map Group

                var zone = relay.GetZone();

                var tex = this.MapTextures.GetMapTexture(zone?.MapResourcePath ?? string.Empty);

                var position = ImGui.GetCursorScreenPos();
                if (tex != null) ImGui.Image(tex.ImGuiHandle, mapSize * ImGui.GetIO().FontGlobalScale);
                else ImGui.Dummy(mapSize * ImGui.GetIO().FontGlobalScale);

                var offset = MapFlagUtils.FlagToPixel(zone?.Scale ?? 1.0f, relay.GetFlag()) * (mapSize.X / 2048 * ImGui.GetIO().FontGlobalScale);
                ImGui.GetWindowDrawList().AddImage(_redFlag.ImGuiHandle, position + (Vector2)offset - iconSize * ImGui.GetIO().FontGlobalScale, position + (Vector2)offset + iconSize * ImGui.GetIO().FontGlobalScale);

                ImGui.EndGroup(); // End Map Group
                ImGui.SameLine(0, 25 * ImGui.GetIO().FontGlobalScale);
                ImGui.BeginGroup(); // Detail Group 

                ImGui.Text($"Actor ID");
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.ActorId:X8}");

                ImGui.Text(Loc.Localize("HuntDetailNameText", "Name"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetHunt()} ({relay.RelayKey})");

                ImGui.Text(Loc.Localize("HuntDetailRankText", "Rank"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetRank()}");

                ImGui.Text(Loc.Localize("HuntDetailStatusText", "Status"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.TextColored(statusColor, statusText);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                ImGui.Text(Loc.Localize("HuntDetailLocationText", "Location"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetZone()} {relay.GetFlagString()} i{relay.InstanceId}");

                ImGui.Text(Loc.Localize("HuntDetailWorldText", "World"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetWorld()}");

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                {
                    DateTime dt;

                    ImGui.Text(Loc.Localize("HuntDetailLastFoundText", "Last Found"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastFound();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                    ImGui.Text(Loc.Localize("HuntDetailLastSeenText", "Last Seen"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastSeen();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                    ImGui.Text(Loc.Localize("HuntDetailLastKilledText", "Last Killed"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastKilled();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                    ImGui.Text(Loc.Localize("HuntDetailLastUntouchedText", "Last Untouched"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastUntouched();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                    ImGui.Text(Loc.Localize("HuntDetailsPlayersNearby", "Players Nearby"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    ImGui.Text($"{state.Relay.Players}");
                }

                ImGui.Dummy(new Vector2(0.0f, 10.0f) * ImGui.GetIO().FontGlobalScale);

                // Send hunt to chat with link
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
                var relayKey = relay.RelayKey;
                ImGui.PushID($"SonarHuntFlag_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.MapMarkerAlt), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.PerformClickAction(state, ClickAction.Chat);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Loc.Localize("FlagButtonTooltip", "Show coordinates in chat window"));
                }

                ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

                // Set flag to hunt location and open map
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
                ImGui.PushID($"SonarHuntMap_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.MapMarked), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.PerformClickAction(state, ClickAction.Map);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Loc.Localize("MapButtonTooltip", "Set flag and open map"));
                }

                ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

                // Teleport
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
                ImGui.PushID($"SonarHuntTeleport_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.Diamond), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.PerformClickAction(state, ClickAction.Teleport);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(Loc.Localize("TeleportButtonTooltip", "Teleport to Closest Aetheryte"));

                ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

                // Delete hunt from list until next update
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightRed);
                ImGui.PushID($"SonarHuntRemove_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.Times), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.Client.Trackers.Hunts.Data.Remove(relay);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(Loc.Localize("RemoveHuntButtonTooltip", "Remove hunt from the list until next update"));

                ImGui.EndGroup(); // End Detail Group
            }
        }

        private void DrawFate(RelayState<FateRelay> state)
        {
            var relay = state.Relay;
            var fateStatus = relay.Status;

            Vector4 statusColor;
            string statusText = $"{fateStatus}";

            switch (fateStatus)
            {
                case FateStatus.Running:
                    statusColor = this.Plugin.Configuration.Colors.FateRunning;
                    break;
                case FateStatus.Complete:
                    statusColor = this.Plugin.Configuration.Colors.FateComplete;
                    break;
                case FateStatus.Failed:
                    statusColor = this.Plugin.Configuration.Colors.FateFailed;
                    break;
                case FateStatus.Preparation:
                    statusColor = this.Plugin.Configuration.Colors.FatePreparation;
                    break;
                case FateStatus.Unknown:
                    statusColor = this.Plugin.Configuration.Colors.FateUnknown;
                    break;
                default:
                    statusColor = ColorPalette.Grey; // Hardcoded, should never happen
                    break;
            }
            if (fateStatus == FateStatus.Running && state.Relay.Progress > 0) statusColor = this.Plugin.Configuration.Colors.FateProgress;

            ImGui.PushStyleColor(ImGuiCol.Text, statusColor);
            bool isHeaderOpen = ImGui.TreeNodeEx($"##fate_{relay.RelayKey}", ImGuiTreeNodeFlags.CollapsingHeader, $"{relay}");
            ImGui.PopStyleColor();

            this.PerformClickAction(state);

            if (isHeaderOpen)
            {
                ImGui.BeginGroup(); // Map Group

                var zone = relay.GetZone();

                var tex = this.MapTextures.GetMapTexture(zone?.MapResourcePath ?? string.Empty);

                var position = ImGui.GetCursorScreenPos();
                if (tex != null) ImGui.Image(tex.ImGuiHandle, mapSize * ImGui.GetIO().FontGlobalScale);
                else ImGui.Dummy(mapSize * ImGui.GetIO().FontGlobalScale);

                var offset = MapFlagUtils.FlagToPixel(zone?.Scale ?? 1.0f, relay.GetFlag()) * (mapSize.X / 2048 * ImGui.GetIO().FontGlobalScale);
                ImGui.GetWindowDrawList().AddImage(_redFlag.ImGuiHandle, position + (Vector2)offset - iconSize * ImGui.GetIO().FontGlobalScale, position + (Vector2)offset + iconSize * ImGui.GetIO().FontGlobalScale);

                ImGui.EndGroup(); // End Map Group
                ImGui.SameLine(0, 25 * ImGui.GetIO().FontGlobalScale);
                ImGui.BeginGroup(); // Detail Group 

                ImGui.Text(Loc.Localize("FateDetailNameText", "Name"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetFate()} ({relay.RelayKey})");

                ImGui.Text(Loc.Localize("FateDetailLevelText", "Level"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetFate()?.Level}");

                ImGui.Text(Loc.Localize("FateDetailStatusText", "Status"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.TextColored(statusColor, statusText);

                ImGui.Text(Loc.Localize("FateDetailDurationText", "Duration"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text(relay.GetRemainingTimeString());

                ImGui.Text(Loc.Localize("FateDetailProgressText", "Progress"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                float progress = (float)relay.Progress / 100f;
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetTextLineHeight() / 2 - 2f);
                ImGui.ProgressBar(progress, new Vector2(180, 10), string.Empty);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                ImGui.Text(Loc.Localize("FateDetailLocationText", "Location"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetZone()} {relay.GetFlagString()} i{relay.InstanceId}");

                ImGui.Text(Loc.Localize("FateDetailWorldText", "World"));
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetWorld()}");

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                {
                    DateTime dt;

                    ImGui.Text(Loc.Localize("HuntDetailLastFoundText", "Last Found"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastFound();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                    ImGui.Text(Loc.Localize("HuntDetailLastSeenText", "Last Seen"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastSeen();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                    ImGui.Text(Loc.Localize("HuntDetailLastKilledText", "Last Killed"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastKilled();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                    ImGui.Text(Loc.Localize("HuntDetailLastUntouchedText", "Last Untouched"));
                    ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                    dt = state.GetLastUntouched();
                    ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");
                }

                ImGui.Dummy(new Vector2(0.0f, 10.0f) * ImGui.GetIO().FontGlobalScale);

                // Send Fate to chat with link
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
                var relayKey = relay.RelayKey;
                ImGui.PushID($"SonarFateFlag_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.MapMarkerAlt), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.PerformClickAction(state, ClickAction.Chat);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(Loc.Localize("FlagButtonTooltip", "Show coordinates in chat window"));

                ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

                // Set flag to fate location and open map
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
                ImGui.PushID($"SonarFateMap_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.MapMarked), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.PerformClickAction(state, ClickAction.Map);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(Loc.Localize("MapButtonTooltip", "Set flag and open map"));

                ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

                // Teleport
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
                ImGui.PushID($"SonarFateTeleport_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.Diamond), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.PerformClickAction(state, ClickAction.Teleport);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(Loc.Localize("TeleportButtonTooltip", "Teleport to Closest Aetheryte"));

                ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

                // Remove the fate from the list until it is next updated
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightRed);
                ImGui.PushID($"SonarFateRemove_{relayKey}");
                if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.Times), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
                {
                    this.Client.Trackers.Fates.Data.Remove(relay);
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(Loc.Localize("RemoveFateButtonTooltip", "Remove fate from the list till next update"));

                ImGui.EndGroup(); // End Detail Group
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Client.Tick += this.Client_Tick;
            this.Ui.Draw += this.Draw;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Client.Tick -= this.Client_Tick;
            this.Ui.Draw -= this.Draw;
            return Task.CompletedTask;
        }

        #region IDisposable Support
        public void Dispose()
        {
            this._redFlag?.Dispose();
        }
        #endregion
    }
}
