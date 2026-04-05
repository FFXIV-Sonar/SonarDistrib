using Dalamud.Plugin.Services;
using DryIocAttributes;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Microsoft.Extensions.Hosting;
using Sonar;
using Sonar.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CSVector3 = FFXIVClientStructs.FFXIV.Common.Math.Vector3;

namespace SonarPlugin.Trackers
{
    /// <summary>Counts nearby players.</summary>
    [ExportMany]
    [SingletonReuse]
    public sealed class PlayerCounterService : IHostedService
    {
        private readonly Dictionary<uint, Vector3> _players = [];

        private SonarPlugin Plugin { get; }
        private SonarMeta Meta { get; }

        public PlayerCounterService(SonarPlugin plugin, SonarMeta meta)
        {
            this.Plugin = plugin;
            this.Meta = meta;
        }

        /// <summary>Gets total player count.</summary>
        /// <remarks><see cref="IObjectTable.LocalPlayer"/> included.</remarks>
        public int Count => this._players.Count;

        /// <summary>Gets player count within a certain <paramref name="distanceSquared"/> from <see cref="IObjectTable.LocalPlayer"/>.</summary>
        /// <remarks><see cref="IObjectTable.LocalPlayer"/> included.</remarks>
        /// <param name="distanceSquared">Squared distance.</param>
        /// <returns>Player count within a certain <paramref name="distanceSquared"/> from <see cref="IObjectTable.LocalPlayer"/>.</returns>
        public unsafe int GetCount(float distanceSquared = 2500f)
        {
            var manager = CharacterManager.Instance();
            if (manager is null) return 0;

            var player = manager->BattleCharas[0].Value;
            return player is not null ? this.GetCount(player->Position, distanceSquared) : 0;
        }

        /// <summary>Gets player count within a certain <paramref name="distanceSquared"/> from <paramref name="refPos"/>.</summary>
        /// <remarks><see cref="IObjectTable.LocalPlayer"/> included if within range.</remarks>
        /// <param name="refPos">Reference position to base the <paramref name="distanceSquared"/> from.</param>
        /// <param name="distanceSquared">Squared distance.</param>
        /// <returns>Player count within a certain <paramref name="distanceSquared"/> from <see cref="IObjectTable.LocalPlayer"/>.</returns>
        [SuppressMessage("csharpsquid", "S3267")]
        public int GetCount(Vector3 refPos, float distanceSquared = 2500f)
        {
            var count = 0;
            foreach (var position in this._players.Values)
            {
                if (Vector3.DistanceSquared(refPos, position) <= distanceSquared) count++;
            }
            return count;
        }

        /// <summary>Reset player counts.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => this._players.Clear();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkTick += this.CountPlayers;
            this.Meta.PlaceChanged += this.ZoneChanged;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Plugin.FrameworkTick -= this.CountPlayers;
            this.Meta.PlaceChanged -= this.ZoneChanged;
            return Task.CompletedTask;
        }

        // Intentionally done this way to avoid IEnumerable and LINQ overhead
        private unsafe void CountPlayers(IFramework framework)
        {
            var manager = CharacterManager.Instance();
            if (manager is null) return;
            var characters = manager->BattleCharas;
            var players = this._players;
            foreach (var characterPtr in characters)
            {
                var character = characterPtr.Value;
                if (character is not null && character->ObjectKind is ObjectKind.Pc) players[character->EntityId] = Unsafe.As<CSVector3, Vector3>(ref character->Position);
            }
        }

        private void ZoneChanged(PlayerPosition obj) => this.Reset();
    }
}
