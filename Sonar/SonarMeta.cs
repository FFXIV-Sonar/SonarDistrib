using Sonar.Models;
using SonarUtils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar
{
    public sealed class SonarMeta
    {
        private SpinLock _lock = new(false);
        private PlayerInfo? _playerInfo;
        private PlayerPlace? _playerPlace;

        public PlayerInfo? PlayerInfo => this._playerInfo;
        public PlayerPlace? PlayerPlace => this._playerPlace;

        internal SonarMeta() { /* Empty */ }

        /// <summary>Update player information</summary>
        /// <param name="playerInfo">Player information</param>
        /// <returns>Succeeded</returns>
        /// <remarks>
        /// This method will always succeed unless one of the following happens:
        /// <list type="bullet">
        /// <item><see cref="PlayerInfo"/> is the same (unchanged)</item>
        /// <item><see cref="PlayerInfo"/> is <c>null</c></item>
        /// </list>
        /// </remarks>
        public bool UpdatePlayerInfo(PlayerInfo playerInfo)
        {
            if (playerInfo is null || playerInfo.Equals(this._playerInfo)) return false;
            var lockTaken = false;
            this._lock.Enter(ref lockTaken);
            try
            {
                this._playerInfo = playerInfo;
                this.PlayerInfoChanged?.Invoke(playerInfo);
            }
            finally
            {
                this._lock.Exit();
            }
            return true;
        }

        public bool UpdatePlayerPlace(PlayerPlace playerPlace)
        {
            if (playerPlace is null || playerPlace.Equals(this._playerPlace)) return false;
            var lockTaken = false;
            this._lock.Enter(ref lockTaken);
            try
            {
                this._playerPlace = playerPlace;
                this.PlayerPlaceChanged?.Invoke(playerPlace);
            }
            finally
            {
                this._lock.Exit();
            }
            return true;
        }

        internal event Action<PlayerInfo>? PlayerInfoChanged;
        internal event Action<PlayerPlace>? PlayerPlaceChanged;
    }
}
