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

            var addon = rapture is not null ? rapture->GetAddonByName("WKSAnnounce") : null;
            using (var node = ImRaii.TreeNode("WKSAnnounce Addon"))
            {
                if (node.Success)
                {
                    if (addon is not null)
                    {
                        var values = addon->AtkValuesSpan;
                        for (var index = 0; index < values.Length; index++)
                        {
                            var value = values[index];
                            ImGui.TextUnformatted($"Value {index}: {value}");
                        }
                    }
                    else
                    {
                        ImGui.TextUnformatted("WKSAnnounce is null");
                    }
                }
            }

            var agent = (AgentWKSMission*)(addon is not null ? this.GameGui.FindAgentInterface(addon).Address : 0); // TODO: Fix once available
            using (var node = ImRaii.TreeNode("AgentWKSAnnounce"))
            {
                if (node.Success)
                {
                    if (agent is not null)
                    {
                        if (agent->IsAgentActive())
                        {
                            ImGui.TextUnformatted("WKS Announce Agent is active but uh... wait for Dalamud update T_T");
                        }
                        else
                        {
                            ImGui.TextUnformatted("WKS Announce Agent is not active");
                        }
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
                        ImGui.TextUnformatted($"Flags: {currentEvent->Flags:F}");
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
