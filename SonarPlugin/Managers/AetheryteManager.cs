using Dalamud.Plugin.Services;
using DryIocAttributes;
using Sonar.Data;
using Sonar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Sonar.Data.Rows;
using SonarPlugin.Config;

namespace SonarPlugin.Managers
{
    // Attunement check and teleport code based on: https://github.com/Ottermandias/GatherBuddy/blob/main/GatherBuddy/SeFunctions/Teleporter.cs
    // NOTE: Under no circumstances world IDs are checked. It is the task of the player to ensure they're on the correct world first.
    [SingletonService]
    public sealed class AetheryteManager
    {
        private IClientState ClientState { get; }
        private SonarPlugin Plugin { get; }
        private SonarConfiguration Configuration => this.Plugin.Configuration;
        private IPluginLog Logger { get; }

        public AetheryteManager(IClientState clientState, SonarPlugin plugin, IPluginLog logger)
        {
            this.ClientState = clientState;
            this.Plugin = plugin;
            this.Logger = logger;

            this.Logger.Info("Aetherytes Manager initialized");
        }

        public unsafe bool IsAttuned(uint aetheryteId)
        {
            if (aetheryteId is 0) return false;
            if (!this.ClientState.IsLoggedIn || this.ClientState.LocalPlayer is null) return false; // I rather not return true

            var telepo = Telepo.Instance();
            if (telepo is null) return false;
            telepo->UpdateAetheryteList();

            var endPtr = telepo->TeleportList.Last;
            for (var it = telepo->TeleportList.First; it != endPtr; ++it)
            {
                if (it->AetheryteId == aetheryteId) return true;
            }
            return false;
        }

        public bool IsAttuned(AetheryteRow aetheryte)
        {
            return this.IsAttuned(aetheryte.Id);
        }

        public unsafe bool Teleport(uint aetheryteId)
        {
            if (aetheryteId is 0) return false;
            if (!this.IsAttuned(aetheryteId)) return false;
            var telepo = Telepo.Instance();
            if (telepo is null) return false;
            return telepo->Teleport(aetheryteId, 0);
        }

        public bool Teleport(AetheryteRow aetheryte)
        {
            return this.Teleport(aetheryte.Id);
        }

        public uint FindFirstAttuned(params uint[] aetheryteIds)
        {
            return aetheryteIds.FirstOrDefault(this.IsAttuned, 0u);
        }

        public AetheryteRow? FindFirstAttuned(params AetheryteRow[] aetherytes)
        {
            return aetherytes.FirstOrDefault(this.IsAttuned);
        }

        public AetheryteRow? FindClosest(GamePosition position)
        {
            // The Dravanian Hinterlands
            if (position.ZoneId == 399) return Database.Aetherytes.GetValueOrDefault(this.FindFirstAttuned(75, 77));// Idyllshire or Anyx Trine

            return Database.Aetherytes.Values
                .Where(aetheryte => aetheryte.Teleportable)
                .Where(aetheryte => aetheryte.ZoneId == position.ZoneId)
                .OrderBy(aetheryte => aetheryte.Coords.Delta(position.Coords).Length() + aetheryte.DistanceCostModifier)
                .FirstOrDefault();
        }

        public uint FindClosestId(GamePosition position)
        {
            return this.FindClosest(position)?.Id ?? 0;
        }

        public bool TeleportToClosest(GamePosition position)
        {
            return this.Teleport(this.FindClosestId(position));
        }

        public bool TeleportToClosest(GamePosition position, bool checkWorld)
        {
            if (checkWorld && this.ClientState.LocalPlayer?.CurrentWorld.Id != position.WorldId)
            {
                var cityStateMeta = this.Configuration.PreferredCityState.GetMeta();
                return this.Teleport(this.FindFirstAttuned(cityStateMeta?.AetheryteId ?? 0, 8, 2, 9)); // Limsa, Gridania, Uldah
            }
            return this.TeleportToClosest(position);
        }
    }
}
