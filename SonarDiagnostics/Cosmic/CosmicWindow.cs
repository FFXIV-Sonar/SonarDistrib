using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.STD.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarDiagnostics.Cosmic
{
    [Export]
    [SingletonReuse]
    public unsafe sealed class CosmicWindow : Window, IDisposable
    {
        private IGameGui GameGui { get; }
        private WindowSystem Windows { get; }

        public CosmicWindow(IGameGui gameGui, WindowSystem windows) : base("Cosmic Exploration")
        {
            this.GameGui = gameGui;
            this.Windows = windows;

            this.Windows.AddWindow(this);

            this.Size = new(400, 300);
            this.SizeCondition = ImGuiCond.FirstUseEver;
        }
        
        public override void Draw()
        {
            var manager = WKSManager.Instance();
            var rapture = RaptureAtkUnitManager.Instance();

            using (var node = ImRaii.TreeNode("WKSManager"))
            {
                if (node.Success)
                {
                    DrawManager(manager);
                }
            }

            var addon = rapture is not null ? (AgentWKSAnnounce*)rapture->GetAddonByName("WKSAnnounce") : null;
            var agent = (AgentWKSAnnounce*)(addon is not null ? this.GameGui.FindAgentInterface(addon).Address : 0);
            using (var node = ImRaii.TreeNode("WKS Announce Agent"))
            {
                if (node.Success)
                {
                    if (agent is not null && agent->IsAddonReady() && agent->Data is not null)
                    {
                        var data = agent->Data;
                        var stateId = data->State;
                        var stateType = stateId switch
                        {
                            0 => "Mech Ops Commenced", // Seems to be the default state?
                                // While Mech Ops is in progress EndTime points to the start time
                                // Flag: HasCurrentEvent | Event Flag? 1923?
                            1 => "Red Alert Incoming",
                            2 => "Red Alert Progressing",
                            3 => "???",
                            4 => "???",
                            5 => "Mech Ops Issued",
                            6 => "Mech Ops Deploying",
                            7 => "???",
                            8 => "Waiting for dev stage progress",
                            _ => "Unknown",
                        };
                        ImGui.TextUnformatted($"State: {stateId} ({stateType})");
                        ImGui.TextUnformatted($"State Progress: {data->StateProgress}");
                        ImGui.TextUnformatted($"Emergency ID: {data->EmergencyInfoRowId}.{(&data->EmergencyInfoRowId)[1]}"); // TODO: Change this weird pointer operation into data->EmergencyInfoSubRowId once https://github.com/aers/FFXIVClientStructs/pull/1707 makes it into Dalamud.
                        ImGui.TextUnformatted($"End Time: {data->EndTime}");
                        ImGui.TextUnformatted($"Dev Grade: {data->DevGrade}");
                    }
                    else
                    {
                        ImGui.TextUnformatted("WKS Announce Agent is null");
                    }
                }
            }
        }

        private static void DrawManager(WKSManager* manager)
        {
            if (manager is null)
            {
                ImGui.TextUnformatted("WKSManager is null");
                return;
            }
            
            ImGui.TextUnformatted($"DevGrade: {manager->DevGrade}");
            ImGui.TextUnformatted($"CurrentFateId: {manager->CurrentFateId}");
            ImGui.TextUnformatted($"CurrentFateControlRowId: {manager->CurrentFateControlRowId}");
            ImGui.TextUnformatted($"CurrentMissionUnitRowid: {manager->CurrentMissionUnitRowId}");
            ImGui.TextUnformatted($"FishingBait: {manager->FishingBait}");
            ImGui.TextUnformatted($"TerritoryId: {manager->TerritoryId}");
            ImGui.TextUnformatted($"Scores: {string.Join(", ", manager->Scores.ToArray())}");

            var missions = manager->MissionModule;
            using (var node = ImRaii.TreeNode("WKSMissionModule"))
            {
                if (node.Success)
                {
                    DrawManagerMissions(missions);
                }
            }

            var mecha = manager->MechaEventModule;
            using (var node = ImRaii.TreeNode("WKSMechaEventModule"))
            {
                if (node.Success)
                {
                    DrawManagerMecha(mecha);
                }
            }
        }

        //private static void DrawAgent()

        private static void DrawManagerMissions(WKSMissionModule* missions)
        {
            if (missions is null)
            {
                ImGui.TextUnformatted("WKSMissionsModule is null");
                return;
            }

            ImGui.TextUnformatted("TODO");
        }

        private static void DrawManagerMecha(WKSMechaEventModule* mecha)
        {
            if (mecha is null)
            {
                ImGui.TextUnformatted("WKSMechaEventModule is null");
                return;
            }

            ImGui.TextUnformatted($"Flags: {mecha->Flags:F}");

            using (var node2 = ImRaii.TreeNode("CurrentEvent"))
            {
                if (node2.Success)
                {
                    var currentEvent = mecha->CurrentEvent;
                    if (currentEvent is not null)
                    {
                        ImGui.TextUnformatted($"Flags: {currentEvent->Flags:F} (0x{currentEvent->Flags:X})");
                        ImGui.TextUnformatted($"Mecha Event ID: {currentEvent->WKSMechaEventDataRowId}");
                        
                        using (var node3 = ImRaii.TreeNode("Map Markers", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            if (node3.Success)
                            {
                                foreach (var marker in currentEvent->MapMarkers)
                                {
                                    var position = marker.MapMarkerData.Position;
                                    var data = marker.MapMarkerData;
                                    ImGui.TextUnformatted($"{marker.Name} (Flags: {marker.Flags} | Data Flags: {marker.MapMarkerDataFlags} | EndTimestamp: {data.EndTimestamp}) (Coords: {position.X}, {position.Z}, {position.Y})");
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        ImGui.TextUnformatted("CurrentEvent is null");
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Windows.RemoveWindow(this);
        }
    }
}
