using DryIocAttributes;
using Sonar.Models;
using SonarUtils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sonar.Numerics;
using static Sonar.SonarConstants;

namespace Sonar
{
    [ExportEx]
    [SingletonReuse]
    public sealed class SonarMeta
    {
        internal SpinLock _lock = new(false);

        private SonarClient Client { get; }

        public PlayerInfo? PlayerInfo { get; private set; }
        public PlayerPosition? PlayerPosition { get; private set; }

        internal SonarMeta(SonarClient client)
        {
            this.Client = client;
        }

        /// <summary>Update player information</summary>
        /// <param name="playerInfo">Player information</param>
        /// <returns>Succeeded</returns>
        /// <remarks>
        /// This method will always succeed unless one of the following happens:
        /// <list type="bullet">
        /// <item><see cref="Models.PlayerInfo"/> is the same (unchanged)</item>
        /// <item><see cref="Models.PlayerInfo"/> is <c>null</c></item>
        /// </list>
        /// </remarks>
        public bool UpdatePlayerInfo(PlayerInfo? playerInfo)
        {
            if (playerInfo is null || playerInfo.Equals(this.PlayerInfo)) return false;
            var lockTaken = false;
            this._lock.Enter(ref lockTaken);
            try
            {
                if (playerInfo is null || playerInfo.Equals(this.PlayerInfo)) return false;
                this.PlayerInfo = playerInfo;
                this.PlayerInfoChanged?.Invoke(this.PlayerInfo);
                this.Client.Connection.SendIfConnected(playerInfo);
            }
            finally
            {
                this._lock.Exit();
            }
            return true;
        }

        /// <summary>Update player position</summary>
        /// <param name="playerPosition">Player position</param>
        /// <returns>Succeeded</returns>
        /// <remarks>
        /// This method will always succeed unless one of the following happens:
        /// <list type="bullet">
        /// <item><see cref="Models.PlayerPosition"/> is the same (unchanged)</item>
        /// <item><see cref="Models.PlayerPosition"/> is <c>null</c></item>
        /// </list>
        /// </remarks>
        public (bool PlaceUpdated, bool PositionUpdated) UpdatePlayerPosition(PlayerPosition? playerPosition)
        {
            if (playerPosition is null) return (false, false);
            var placeUpdated = this.PlayerPosition is null || !playerPosition.WorldZoneInstanceEquals(this.PlayerPosition);
            var positionUpdated = placeUpdated || playerPosition.Coords.Delta(this.PlayerPosition!.Coords).LengthSquared() >= RoughDistanceSquared;
            if (!placeUpdated && !positionUpdated) return (false, false);

            var lockTaken = false;
            this._lock.Enter(ref lockTaken);
            try
            {
                placeUpdated = this.PlayerPosition is null || !playerPosition.WorldZoneInstanceEquals(this.PlayerPosition);
                positionUpdated = placeUpdated || playerPosition.Coords.Delta(this.PlayerPosition!.Coords).LengthSquared() >= RoughDistanceSquared;
                if (!placeUpdated && !positionUpdated) return (false, false);

                this.PlayerPosition = playerPosition;
                if (placeUpdated) this.PlayerPlaceChanged?.Invoke(playerPosition);
                if (positionUpdated) this.PlayerPlaceChanged?.Invoke(playerPosition);
                this.Client.Connection.SendIfConnected(playerPosition);
            }
            finally
            {
                this._lock.Exit();
            }
            return (placeUpdated, positionUpdated);
        }

        internal event Action<PlayerInfo>? PlayerInfoChanged;
        internal event Action<PlayerPosition>? PlayerPlaceChanged;
        internal event Action<PlayerPosition>? PlayerPositionChanged;
    }
}
