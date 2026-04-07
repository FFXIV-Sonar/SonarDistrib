using DryIocAttributes;
using Sonar.Models;
using System;
using System.Threading;
using static Sonar.SonarConstants;
using Sonar.Extensions;
using System.Collections.Immutable;

namespace Sonar
{
    [ExportEx]
    [SingletonReuse]
    public sealed class SonarMeta
    {
        internal SpinLock _lock = new(false);
        private ImmutableArray<Action<PlayerInfo>> _infoHandlers = [];
        private ImmutableArray<Action<PlayerPosition>> _placeHandlers = [];
        private ImmutableArray<Action<PlayerPosition>> _positionHandlers = [];

        private SonarClient Client { get; }

        public PlayerInfo? PlayerInfo { get; private set; }
        public PlayerPosition? PlayerPosition { get; private set; }

        internal SonarMeta(SonarClient client)
        {
            this.Client = client;
            this.Client.Connection.MessageReceived += this.MessageHandler;
        }

        private void MessageHandler(Connections.SonarConnectionManager _, Messages.ISonarMessage message)
        {
            switch (message)
            {
                case LodestoneVerificationNeeded need:
                    this.VerificationNeeded?.SafeInvoke(this, need);
                    break;

                case LodestoneVerificationResult result:
                    this.VerificationResult?.SafeInvoke(this, result);
                    break;
            }
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

            this.PlayerInfo = playerInfo;
            this.Client.Connection.SendIfConnected(playerInfo);

            foreach (var handler in this._infoHandlers)
            {
                try
                {
                    handler(playerInfo);
                }
                catch (Exception ex)
                {
                    this.Client.LogError(ex, "Exception occured at PlayerInfoChanged handler");
                }
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

            this.PlayerPosition = playerPosition;
            this.Client.Connection.SendIfConnected(playerPosition);

            if (positionUpdated)
            {
                foreach (var handler in this._positionHandlers)
                {
                    try
                    {
                        handler(playerPosition);
                    }
                    catch (Exception ex)
                    {
                        this.Client.LogError(ex, "Exception occured at PlayerInfoChanged handler");
                    }
                }
            }

            if (placeUpdated)
            {
                foreach (var handler in this._placeHandlers)
                {
                    try
                    {
                        handler(playerPosition);
                    }
                    catch (Exception ex)
                    {
                        this.Client.LogError(ex, "Exception occured at PlayerPlaceChanged handler");
                    }
                }
            }

            return (placeUpdated, positionUpdated);
        }

        public void RequestVerification()
        {
            this.Client.Connection.SendIfConnected(new LodestoneVerificationRequest());
        }

        public event Action<PlayerInfo>? InfoChanged
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._infoHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._infoHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }

        public event Action<PlayerPosition>? PlaceChanged
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._placeHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._placeHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }

        public event Action<PlayerPosition>? PositionChanged
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._positionHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._positionHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }

        public event Action<SonarMeta, LodestoneVerificationNeeded>? VerificationNeeded;
        public event Action<SonarMeta, LodestoneVerificationResult>? VerificationResult;
    }
}
