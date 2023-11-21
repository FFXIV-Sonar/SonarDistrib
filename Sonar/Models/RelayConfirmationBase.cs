using MessagePack;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Data.Rows;
using Sonar.Utilities;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sonar.Trackers;
using Sonar.Relays;
using SonarUtils;

namespace Sonar.Models
{
    [MessagePackObject]
    public class RelayConfirmationBase<T> : RelayConfirmationBase where T : Relay
    {
        public RelayConfirmationBase() { }
        public RelayConfirmationBase(uint worldId, uint zoneId, uint instanceId, uint relayId) : base(worldId, zoneId, instanceId, relayId) { }
        public RelayConfirmationBase(T relay) : base(relay.WorldId, relay.ZoneId, relay.InstanceId, relay.Id) { }
        public RelayConfirmationBase(RelayState<T> state) : this(state.Relay) { }
        public RelayConfirmationBase(RelayConfirmationBase confirmation) : base(confirmation) { }

        public override string ToString()
        {
            string? name = null;
            if (typeof(T) == typeof(HuntRelay))
            {
                var info = Database.Hunts.GetValueOrDefault(this.RelayId);
                if (info is not null) name = $"Rank {info.Rank}: {info.Name}";
            }
            else if (typeof(T) == typeof(FateRelay))
            {
                var info = Database.Fates.GetValueOrDefault(this.RelayId);
                if (info is not null) name = $"FATE: {info.Name}";
            }
            if (name is null) name = $"Unknown Entity ({this.RelayId})";
            return $"{name} {this.GetZone()?.Name.ToString() ?? $"Unknown Zone ({this.ZoneId})"} <{this.GetWorld()?.Name ?? $"{this.WorldId}"}> i{this.InstanceId}";
        }
    }

    [SuppressMessage("Code Smell", "S4035", Justification = "Intentional")]
    [MessagePackObject]
    public class RelayConfirmationBase : GamePlace, IEquatable<RelayConfirmationBase>
    {
        private string? _key;

        [IgnoreMember]
        public string Key => this._key ??= StringUtils.Intern($"{this.WorldId}_{this.ZoneId}_{this.InstanceId}_{this.RelayId}");

        [Key(3)]
        public uint RelayId { get; set; }

        // Implementation detail: Actor ID won't be included for SS Minions should we decide to confirm those.

        public RelayConfirmationBase() { }
        public RelayConfirmationBase(uint worldId, uint zoneId, uint instanceId, uint relayId) : base(worldId, zoneId, instanceId)
        {
            this.RelayId = relayId;
        }

        public RelayConfirmationBase(RelayConfirmationBase confirmation) : this(confirmation.WorldId, confirmation.ZoneId, confirmation.InstanceId, confirmation.RelayId) { }

        public static bool Equals(RelayConfirmationBase? left, RelayConfirmationBase? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.WorldId == right.WorldId && left.ZoneId == right.ZoneId && left.InstanceId == right.InstanceId && left.RelayId == right.RelayId;
        }
        public bool Equals(RelayConfirmationBase? other) => Equals(this, other);
        public override bool Equals(object? obj) => obj is RelayConfirmationBase other && Equals(this, other);
        public override int GetHashCode() => this.WorldId.GetHashCode() ^ this.ZoneId.GetHashCode() ^ this.InstanceId.GetHashCode() ^ this.RelayId.GetHashCode();
        public static bool operator ==(RelayConfirmationBase? left, RelayConfirmationBase? right) => Equals(left, right);
        public static bool operator !=(RelayConfirmationBase? left, RelayConfirmationBase? right) => !Equals(left, right);

        public bool RelayEquals(Relay relay)
        {
            return this.WorldId == relay.WorldId && this.ZoneId == relay.ZoneId && this.InstanceId == relay.InstanceId && this.RelayId == relay.Id;
        }

        public bool PlaceEquals(GamePlace place)
        {
            return this.WorldId == place.WorldId && this.ZoneId == place.ZoneId && this.InstanceId == place.InstanceId;
        }
    }
}
