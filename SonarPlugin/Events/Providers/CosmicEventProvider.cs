using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sonar;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Relays;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SonarPlugin.Events.Providers
{
    [ExportMany]
    [SingletonReuse]
    public sealed partial class CosmicEventProvider : IHostedService
    {
        private readonly FrozenSet<uint> _cosmicZoneIds;

        private SonarEventManager Events { get; }
        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        private IClientState ClientState { get; }
        private IGameGui GameGui { get; }
        private ILogger Logger { get; }
        
        public CosmicEventProvider(SonarEventManager events, SonarPlugin plugin, SonarClient client, IClientState clientState, IGameGui gameGui, ILogger<CosmicEventProvider> logger)
        {
            this.Events = events;
            this.Plugin = plugin;
            this.Client = client;
            this.ClientState = clientState;
            this.GameGui = gameGui;
            this.Logger = logger;

            this._cosmicZoneIds = Database.Zones.Values
                .Where(zone => zone.IntendedUse is 60)
                .Select(zone => zone.Id)
                .ToFrozenSet();

            this.Logger.LogInformation("Cosmic event provider constructed");
            this.Logger.LogDebug("Detected Cosmic Zones: {zones}", string.Join(", ", this._cosmicZoneIds.Select(zoneId => Database.Zones.TryGetValue(zoneId, out var zone) ? $"{zone.Name} ({zone.Id})" : $"{zoneId}")));
        }

        public void FrameworkHandler(IFramework _)
        {
            if (!this._cosmicZoneIds.Contains(this.ClientState.TerritoryType)) return;
            this.FrameworkHandlerCore();
        }

        private unsafe void FrameworkHandlerCore()
        {
            var rapture = RaptureAtkUnitManager.Instance();
            if (rapture is null) return;

            var addon = rapture->GetAddonByName("WKSAnnounce");
            if (addon is null || !addon->IsReady) return;

            var agent = (AgentWKSAnnounce*)this.GameGui.FindAgentInterface(addon).Address;
            if (agent is null || !agent->IsAddonReady()) return;

            var data = agent->Data;
            if (data is null) return;

            var state = data->State;
            if (state is not 1 and not 2) return; // 1 => Red Alert Incoming | 2 => Red Alert Progressing

            var rowId = data->EmergencyInfoRowId;
            var subRowId = data->EmergencyInfoSubRowId;
            var id = EventUtils.ToId(EventType.CosmicEmergency, rowId, subRowId);

            var playerPosition = this.Client.Meta.PlayerPosition;
            if (playerPosition is null) return;

            var relay = new EventRelay()
            {
                Id = id,

                WorldId = playerPosition.WorldId,
                ZoneId = playerPosition.ZoneId,
                InstanceId = playerPosition.InstanceId,
                Coords = new(0, 0, 0),

                

            };


            if (state is 1)
            {
                // Red Alert Incoming
            }
            else if (state is 2)
            {
                // Red Alert Progressing
                var (progress2, progress1) = Math.DivRem(data->StateProgress, 256);
                var totalProgress = (progress1 + progress2) / 200f;
            }



            /*
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
            */
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkTick += this.FrameworkHandler;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkTick -= this.FrameworkHandler;
            return Task.CompletedTask;
        }
    }
}
