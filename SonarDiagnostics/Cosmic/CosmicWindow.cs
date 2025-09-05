using Dalamud.Interface.Windowing;
using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace SonarDiagnostics.Cosmic
{
    [Export]
    [SingletonReuse]
    public unsafe sealed class CosmicWindow : Window, IDisposable
    {
        private IGameGui GameGui { get; }
        private WindowSystem Windows { get; }

        public CosmicWindow(WindowSystem windows) : base("Cosmic Exploration")
        {
            this.Windows = windows;

            this.Windows.AddWindow(this);

            this.Size = new(400, 300);
            this.SizeCondition = ImGuiCond.FirstUseEver;
        }
        
        public override void Draw()
        {
            var manager = WKSManager.Instance();
            if (manager is null)
            {
                ImGui.TextUnformatted("WKS Manager is not loaded");
                return;
            }

            using (var node = ImRaii.TreeNode("WKS Manager"))
            {
                if (node.Success)
                {
                    ImGui.TextUnformatted($"DevGrade: {manager->DevGrade}");
                    ImGui.TextUnformatted($"CurrentFateId: {manager->CurrentFateId}");
                    ImGui.TextUnformatted($"CurrentFateControlRowId: {manager->CurrentFateControlRowId}");
                    ImGui.TextUnformatted($"CurrentMissionUnitRowid: {manager->CurrentMissionUnitRowId}");
                    ImGui.TextUnformatted($"FishingBait: {manager->FishingBait}");
                    ImGui.TextUnformatted($"TerritoryId: {manager->TerritoryId}");
                    ImGui.TextUnformatted($"Scores: {string.Join(", ", manager->Scores.ToArray())}");
                }
            }

            using (var node = ImRaii.TreeNode("Missions Module"))
            {
                if (node.Success)
                {
                    var missions = manager->MissionModule;
                    if (missions is not null)
                    {
                        /* Empty */
                    }
                    else
                    {
                        ImGui.TextUnformatted("Missions module is null");
                    }
                }
            }

            using (var node = ImRaii.TreeNode("Mecha Event Module"))
            {
                if (node.Success)
                {
                    var mecha = manager->MechaEventModule;
                    if (mecha is not null)
                    {
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
                        ImGui.TextUnformatted($"Flags: {mecha->Flags:F}");
                    }
                    else
                    {
                        ImGui.TextUnformatted("Mecha event module is null");
                    }
                }
            }

            using (var node = ImRaii.TreeNode("WKS Announce Addon"))
            {
                if (node.Success)
                {
                    var rapture = RaptureAtkUnitManager.Instance();
                    if (rapture is not null)
                    {
                        var addon = rapture->GetAddonByName("WKSAnnounce");
                        if (addon is not null)
                        {
                            var values = addon->AtkValuesSpan;
                            for (var index = 0; index < values.Length; index++)
                            {
                                var value = values[index];
                                ImGui.TextUnformatted($"Value {index} ({value.Type}): ");
                                ImGui.SameLine();
                                
                                switch (value.Type)
                                {
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Undefined:
                                        ImGui.TextUnformatted("Undefined");
                                        break;
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Null:
                                        ImGui.TextUnformatted("Null");
                                        break;
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Bool:
                                        ImGui.TextUnformatted($"{value.Bool}");
                                        break;

                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int:
                                        ImGui.TextUnformatted($"{value.Int}");
                                        break;
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int64:
                                        ImGui.TextUnformatted($"{value.Int64}");
                                        break;
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt:
                                        ImGui.TextUnformatted($"{value.UInt}");
                                        break;
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt64:
                                        ImGui.TextUnformatted($"{value.UInt64}");
                                        break;

                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Float:
                                        ImGui.TextUnformatted($"{value.Float}");
                                        break;

                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String:
                                        ImGui.TextUnformatted(value.String.HasValue ? value.String : " -- ");
                                        break;
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.WideString:
                                        ImGui.TextUnformatted(value.WideString is not null ? value.WideString : " -- ");
                                        break;
                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.ManagedString:
                                        ImGui.TextUnformatted("...");
                                        break;

                                    case FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String8:
                                        ImGui.TextUnformatted(value.String.HasValue ? value.String : " -- ");
                                        break;

                                }
                            }
                        }
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
