using System.Diagnostics;
using CheapLoc;
using SonarPlugin.Config;
using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
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
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;
using SonarPlugin.Localization;
using AG.EnumLocalization;
using Sonar.Localization;
using SonarUtils.Text.Placeholders;
using DryIoc.Messages;

namespace SonarPlugin.GUI
{
    public sealed class SonarMainWindow : IHostedService, IDisposable
    {
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
        private PlaceholderFormatter Formatter { get; }
        private IUiBuilder Ui { get; }
        private IGameGui GameGui { get; }
        private IFramework Framework { get; }
        private IDalamudPluginInterface PluginInterface { get; }
        private IPluginLog Logger { get; }

        private static Vector2 minimumWindowSize = new(300, 100);
        private static Vector2 maximumWindowSize = new(float.MaxValue, float.MaxValue);
        private static Vector2 mapSize = new(300, 300);
        private static Vector2 iconSize = new(16, 16);
        private static readonly float detailLabelOffset = 100.0f;

        private readonly IDalamudTextureWrap _redFlag;

        public SonarMainWindow(SonarPlugin plugin, SonarClient client, RelayTrackerViews views, HuntNotifier huntsNotifier, FateNotifier fateNotifier, AetheryteManager aetherytes, MapTextureProvider mapTextures, ResourceHelper resources, PlaceholderFormatter formatter, IUiBuilder ui, IGameGui gameGui, IFramework framework, IDalamudPluginInterface pluginInterface, IPluginLog logger)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.Views = views;
            this.HuntNotifier = huntsNotifier;
            this.FateNotifier = fateNotifier;
            this.Aetherytes = aetherytes;
            this.MapTextures = mapTextures;
            this.Resources = resources;
            this.Formatter = formatter;
            this.Ui = ui;
            this.GameGui = gameGui;
            this.Framework = framework;
            this.PluginInterface = pluginInterface;
            this.Logger = logger;

            this._visible = this.Plugin.Configuration.OverlayVisibleByDefault;
            this._redFlag = this.Resources.LoadIcon("redflag.png");

            this.Logger.Information("Sonar Main Overlay Initialized");

            this.PluginInterface.UiBuilder.OpenMainUi += this.OpenWindow;
        }

        private void OpenWindow()
        {
            this.IsVisible = !this.IsVisible;
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

            // RunSetup flags for window based on config settings
            var windowFlags = ImGuiWindowFlags.HorizontalScrollbar | this.WindowFlags;

            if (ImGui.Begin($"{MainWindowLoc.WindowTitle.GetLocString()}###SonarMainWindow", ref this._visible, windowFlags))
            {
                ImGui.BeginChild("###SonarTabBarWindow", (new Vector2(0, 24)) * ImGui.GetIO().FontGlobalScale, false, ImGuiWindowFlags.AlwaysAutoResize | this.TabBarFlags);
                if (ImGui.BeginTabBar("###SonarTabBar", ImGuiTabBarFlags.None))
                {
                    var ranks = Enum.GetValues<HuntRank>();
                    foreach (var rank in ranks.Where(rank => rank != HuntRank.None && ((int)rank & 0x80) != 0x80).Reverse().Prepend(HuntRank.None))
                    {
                        if (!this.Plugin.Configuration.AllSRankSettings && (rank == HuntRank.SS || rank == HuntRank.SSMinion)) continue;

                        // TODO: Eventually localize these enum strings
                        var tabText = rank == HuntRank.None ? $" {MainWindowLoc.AllTab.GetLocString()} ###SonarAllTab" : $" {rank.GetLocString()} ###Sonar{RelayType.Hunt}{rank}Tab";
                        if (ImGui.BeginTabItem(tabText))
                        {
                            ImGui.EndTabItem();
                            this._huntRank = rank;
                            this._huntsVisible = true;
                            this._fatesVisible = rank == HuntRank.None;
                        }
                    }

                    if (ImGui.BeginTabItem($" {HuntRank.Fate.GetLocString()} ###Sonar{RelayType.Fate}{HuntRank.Fate}Tab"))
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
                if (place is null) return false;
                var rank = this._huntRank;

                HuntRank[]? searchRanks = null;
                if (rank == HuntRank.S && !this.Plugin.Configuration.AllSRankSettings)
                {
                    searchRanks = [HuntRank.S, HuntRank.SSMinion, HuntRank.SS];
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

            ImGui.BeginChild($"###{this._huntRank}TabScrollRegion", new Vector2(0, 0), false, this.ListFlags);

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

            foreach (var state in states.Where(s => s.IsDead()))
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

        [SuppressMessage("Major Code Smell", "S3358", Justification = "Reviewed.")]
        private unsafe void PerformClickAction(RelayState state)
        {
            var ctrl = ImGui.IsKeyDown(ImGuiKey.ModCtrl);
            var shift = ImGui.IsKeyDown(ImGuiKey.ModShift);
            var alt = ImGui.IsKeyDown(ImGuiKey.ModAlt);

            var modifier =
                ctrl ? KeyModifier.Ctrl :
                shift ? KeyModifier.Shift :
                alt ? KeyModifier.Alt :
                KeyModifier.None;

            var middle = ImGui.IsItemClicked(ImGuiMouseButton.Middle);
            var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);

            if ((ctrl || shift || alt) && (middle || right)) this.Logger.Debug("Detected click: [ctrl={ctrl} | shift={shift} | alt={alt}] [middle={middle} | right={right}]", ctrl, shift, alt, middle, right);

            var action = ClickAction.None;
            if (false) { /* Empty */ } // easy to copy and paste below

            // No modifier
            else if (modifier == KeyModifier.None && middle) action = this.Plugin.Configuration.MiddleClick;
            else if (modifier == KeyModifier.None && right) action = this.Plugin.Configuration.RightClick;
            
            // Ctrl
            else if (modifier == KeyModifier.Ctrl && middle) action = this.Plugin.Configuration.CtrlMiddleClick;
            else if (modifier == KeyModifier.Ctrl && right) action = this.Plugin.Configuration.CtrlRightClick;
            
            // Shift
            else if (modifier == KeyModifier.Shift && middle) action = this.Plugin.Configuration.ShiftMiddleClick;
            else if (modifier == KeyModifier.Shift && right) action = this.Plugin.Configuration.ShiftRightClick;
            
            // Alt
            else if (modifier == KeyModifier.Alt && middle) action = this.Plugin.Configuration.AltMiddleClick;
            else if (modifier == KeyModifier.Alt && right) action = this.Plugin.Configuration.AltRightClick;

            this.PerformClickAction(state, action);
        }

        private unsafe void PerformClickAction(RelayState state, ClickAction action)
        {
            var mapAgent = AgentMap.Instance();
            if (mapAgent is null) this.Logger.Error("Map Agent not found for click actions"); // NOTE: Do not return as some actions still work

            var relay = state.Relay;
            var zone = relay.GetZone();
            var zoneId = zone?.Id ?? 0;
            var mapId = zone?.MapId ?? 0;
            var coords = relay.Coords.SwapYZ();

            switch (action)
            {
                case ClickAction.None:
                    break;

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

                case ClickAction.Remove:
                    this.Client.Trackers.GetTracker(relay.GetRelayType())?.Data.Remove(relay);
                    break;

                default:
                    this.Logger.Error($"Click Action not defined: {action}");
                    break;
            }
        }

        private void DrawHunt(RelayState<HuntRelay> state)
        {
            var relay = state.Relay;

            Vector4 statusColor;
            string statusText;
            string healthText = $"{100f * relay.CurrentHp / relay.MaxHp:F2}%";

            if (relay.IsMaxHp)
            {
                statusText = RelayStatus.Healthy.GetLocString();
                statusColor = this.Plugin.Configuration.Colors.HuntHealthy;
            }
            else if (!relay.IsDead())
            {
                statusText = RelayStatus.Pulled.GetLocString();
                statusColor = this.Plugin.Configuration.Colors.HuntPulled;
            }
            else
            {
                statusText = RelayStatus.Dead.GetLocString();
                statusColor = this.Plugin.Configuration.Colors.HuntDead;
            }
            statusText = $"{statusText} ({healthText})";

            ImGui.PushStyleColor(ImGuiCol.Text, statusColor);
            var relayText = this.Formatter.Format("<rank>: <name> <hpp> <flag> <<world>> <instance> [<players>]", relay);
            var isHeaderOpen = ImGui.TreeNodeEx($"###{RelayType.Hunt}{relay.RelayKey}", ImGuiTreeNodeFlags.CollapsingHeader, relayText);
            ImGui.PopStyleColor();

            this.PerformClickAction(state);

            if (isHeaderOpen)
            {
                // Map Group
                ImGui.BeginGroup();

                var zone = relay.GetZone();

                var tex = this.MapTextures.GetMapTexture(zone?.MapResourcePath ?? string.Empty);

                var position = ImGui.GetCursorScreenPos();
                if (tex != null) ImGui.Image(tex.Handle, mapSize * ImGui.GetIO().FontGlobalScale);
                else ImGui.Dummy(mapSize * ImGui.GetIO().FontGlobalScale);

                var offset = MapFlagUtils.FlagToPixel(zone?.Scale ?? 1.0f, relay.GetFlag()) * (mapSize.X / 2048 * ImGui.GetIO().FontGlobalScale);
                ImGui.GetWindowDrawList().AddImage(_redFlag.Handle, position + (Vector2)offset - iconSize * ImGui.GetIO().FontGlobalScale, position + (Vector2)offset + iconSize * ImGui.GetIO().FontGlobalScale);

                ImGui.EndGroup(); // End Map Group

                ImGui.SameLine(0, 25 * ImGui.GetIO().FontGlobalScale);

                // Details Group
                ImGui.BeginGroup(); // Detail Group 

                ImGui.Text(RelayDetailsLoc.ActorId.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.ActorId:X8}");

                DrawDetailsGroup1(state);

                ImGui.Text(RelayDetailsLoc.Rank.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{relay.GetRank().GetLocString()}");

                ImGui.Text(RelayDetailsLoc.Status.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.TextColored(statusColor, statusText);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                DrawDetailsGroup2(state);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                this.DrawDetailsGroup3(state);

                ImGui.EndGroup(); // End Detail Group
            }
        }

        private void DrawFate(RelayState<FateRelay> state)
        {
            var relay = state.Relay;
            var fateStatus = relay.Status;

            Vector4 statusColor;
            string statusText = fateStatus.GetLocString();

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
            var relayText = relay.Bonus ? this.Formatter.Format("[B] <name>: <flag> <<world>> <instance> <tap> [<players>]", relay) : this.Formatter.Format("<name>: <flag> <<world>> <instance> <tap> [<players>]", relay); // TODO: Better way to expose Bonus status
            var isHeaderOpen = ImGui.TreeNodeEx($"###{RelayType.Fate}{relay.RelayKey}", ImGuiTreeNodeFlags.CollapsingHeader, relayText);
            ImGui.PopStyleColor();

            this.PerformClickAction(state);

            if (isHeaderOpen)
            {
                ImGui.BeginGroup(); // Map Group

                var zone = relay.GetZone();

                var tex = this.MapTextures.GetMapTexture(zone?.MapResourcePath ?? string.Empty);

                var position = ImGui.GetCursorScreenPos();
                if (tex != null) ImGui.Image(tex.Handle, mapSize * ImGui.GetIO().FontGlobalScale);
                else ImGui.Dummy(mapSize * ImGui.GetIO().FontGlobalScale);

                var offset = MapFlagUtils.FlagToPixel(zone?.Scale ?? 1.0f, relay.GetFlag()) * (mapSize.X / 2048 * ImGui.GetIO().FontGlobalScale);
                ImGui.GetWindowDrawList().AddImage(_redFlag.Handle, position + (Vector2)offset - iconSize * ImGui.GetIO().FontGlobalScale, position + (Vector2)offset + iconSize * ImGui.GetIO().FontGlobalScale);

                ImGui.EndGroup(); // End Map Group
                ImGui.SameLine(0, 25 * ImGui.GetIO().FontGlobalScale);
                ImGui.BeginGroup(); // Detail Group

                DrawDetailsGroup1(state);

                ImGui.Text(RelayDetailsLoc.Status.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.TextColored(statusColor, statusText);

                ImGui.Text(RelayDetailsLoc.Duration.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text(relay.GetRemainingTimeString());

                ImGui.Text(RelayDetailsLoc.Progress.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                float progress = (float)relay.Progress / 100f;
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetTextLineHeight() / 2 - 2f);
                ImGui.ProgressBar(progress, new Vector2(180, 10), string.Empty);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                DrawDetailsGroup2(state);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();

                this.DrawDetailsGroup3(state);

                ImGui.EndGroup(); // End Detail Group
            }
        }

        private static void DrawDetailsGroup1(RelayState state)
        {
            var relay = state.Relay;

            ImGui.Text(RelayDetailsLoc.Name.GetLocString());
            ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
            ImGui.Text($"{relay.Info} ({relay.RelayKey})");

            ImGui.Text(RelayDetailsLoc.Level.GetLocString());
            ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
            ImGui.Text($"{relay.Info.Level}");
        }

        private static void DrawDetailsGroup2(RelayState state)
        {
            var relay = state.Relay;

            ImGui.Text(MainWindowLoc.LocationDetail.GetLocString());
            ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
            ImGui.Text($"{relay.GetFlagString()} i{relay.InstanceId}");

            ImGui.Text(RelayDetailsLoc.World.GetLocString());
            ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
            ImGui.Text($"{relay.GetWorld()}");
        }

        private void DrawDetailsGroup3(RelayState state)
        {
            var relay = state.Relay;
            {
                DateTime dt;

                ImGui.Text(RelayDetailsLoc.LastFound.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                dt = state.GetLastFound();
                ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                ImGui.Text(RelayDetailsLoc.LastSeen.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                dt = state.GetLastSeen();
                ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                ImGui.Text(RelayDetailsLoc.LastKilled.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                dt = state.GetLastKilled();
                ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                ImGui.Text(RelayDetailsLoc.LastHealthy.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                dt = state.GetLastUntouched();
                ImGui.Text($"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToShortTimeString()}");

                ImGui.Text(RelayDetailsLoc.Players.GetLocString());
                ImGui.SameLine(detailLabelOffset * ImGui.GetIO().FontGlobalScale);
                ImGui.Text($"{state.Relay.GetPlayers()}");
            }

            ImGui.Dummy(new Vector2(0.0f, 10.0f) * ImGui.GetIO().FontGlobalScale);

            // Send hunt to chat with link
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
            ImGui.PushID($"SonarFlag{relay.GetRelayType()}{relay.RelayKey}");
            if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.MapMarkerAlt), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
            {
                this.PerformClickAction(state, ClickAction.Chat);
            }
            ImGui.PopID();
            ImGui.PopStyleColor();
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(MainWindowLoc.DetailFlagToolTip.GetLocString());
            }

            ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

            // Set flag to hunt location and open map
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
            ImGui.PushID($"SonarMap{relay.GetRelayType()}{relay.RelayKey}");
            if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.MapMarked), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
            {
                this.PerformClickAction(state, ClickAction.Map);
            }
            ImGui.PopID();
            ImGui.PopStyleColor();
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(MainWindowLoc.DetailMapToolTip.GetLocString());
            }

            ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

            // Teleport
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightOrange);
            ImGui.PushID($"SonarTeleport{relay.GetRelayType()}{relay.RelayKey}");
            if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.Diamond), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
            {
                this.PerformClickAction(state, ClickAction.Teleport);
            }
            ImGui.PopID();
            ImGui.PopStyleColor();
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(MainWindowLoc.DetailTeleportToolTip.GetLocString());
            }

            ImGui.SameLine(0, 10 * ImGui.GetIO().FontGlobalScale);

            // Delete hunt from list until next update
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.LightRed);
            ImGui.PushID($"SonarRemove{relay.GetRelayType()}{relay.RelayKey}");
            if (ImGui.Button(FontAwesomeExtensions.ToIconString(FontAwesomeIcon.Times), (new Vector2(40.0f, 0.0f)) * ImGui.GetIO().FontGlobalScale))
            {
                this.PerformClickAction(state, ClickAction.Remove);
            }
            ImGui.PopID();
            ImGui.PopStyleColor();
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(MainWindowLoc.DetailRemoveToolTip.GetLocString());
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
