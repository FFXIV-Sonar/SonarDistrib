using MessagePack;
using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;
using Newtonsoft.Json;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Messages;
using System;
using Sonar.Data.Rows;
using static Sonar.SonarConstants;
using Sonar.Numerics;
using Sonar.Utilities;
using System.Runtime.CompilerServices;

namespace Sonar.Relays
{
    /// <summary>
    /// Represent a Hunt relay
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    [Serializable]
    public sealed class HuntRelay : Relay
    {
        /// <summary>
        /// Relay Key
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public override string RelayKey
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Bug", "S4275", Justification = "base.RelayKey does this")]
            get
            {
                return this.GetRank() switch
                {
                    HuntRank.SSMinion => $"{base.RelayKey}_{this.ZoneId}_{this.ActorId:X8}",
                    HuntRank.SS => $"{base.RelayKey}_{this.ZoneId}",
                    _ => base.RelayKey
                };
            }
        }

        /// <summary>
        /// Sort Key
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public override string SortKey => this.GetHunt()?.Name.ToString().ToLowerInvariant() ?? base.SortKey;

        /// <summary>
        /// Actor ID
        /// </summary>
        [JsonProperty]
        [Key(6)]
        public uint ActorId { get; set; }

        /// <summary>
        /// Current HP
        /// </summary>
        [JsonProperty]
        [Key(7)]
        public uint CurrentHp { get; set; } = 1;

        /// <summary>
        /// Max Hp
        /// </summary>
        [JsonProperty]
        [Key(8)]
        public uint MaxHp { get; set; } = 1;

        /// <summary>
        /// HP Percentage
        /// </summary>
        [IgnoreMember]
        public float HpPercent => 100f * this.CurrentHp / this.MaxHp;

        /// <summary>
        /// Kill Progress
        /// </summary>
        [IgnoreMember]
        public float Progress => 100f - this.HpPercent;

        #region Status functions
        /// <summary>
        /// Check if hunt is unharmed
        /// </summary>
        [IgnoreMember]
        public bool IsMaxHp => this.CurrentHp == this.MaxHp;

        /// <summary>
        /// Check if hunt is pulled (or harmed in this case)
        /// </summary>
        [IgnoreMember]
        public bool IsPulled => this.CurrentHp != this.MaxHp;

        /// <summary>
        /// Players Nearby Count
        /// </summary>
        [Key(9)]
        [JsonProperty]
        public int Players { get; set; }

        /// <summary>
        /// Check if this hunt is alive
        /// </summary>
        public override bool IsAlive() => this.CurrentHp > 0;
        #endregion


        /// <summary>
        /// Check if another relay regards the same hunt (only Actor ID is checked)
        /// </summary>
        /// <param name="relay">Relay to check</param>
        public bool IsSameEntity(HuntRelay relay) => this.ActorId == relay.ActorId;

        /// <summary>
        /// Check if another relay regards the same hunt (only Actor ID is checked)
        /// </summary>
        /// <param name="relay">Relay to check</param>
        public override bool IsSameEntity(Relay relay) => relay is HuntRelay huntRelay && this.IsSameEntity(huntRelay);

        /// <summary>
        /// Check if another relay is similar (only HP, Coords and Actor ID are checked)
        /// </summary>
        /// <param name="relay">Relay to check</param>
        public bool IsSimilarData(HuntRelay relay) => this.HpPercentRoughlyEquals(relay.HpPercent) && this.Coords.Delta(relay.Coords).LengthSquared() < RoughDistanceSquared;

        /// <summary>
        /// Check if another relay is similar (only HP, Coords and Actor ID are checked)
        /// </summary>
        /// <param name="relay"></param>
        public override bool IsSimilarData(Relay relay, double now) => relay is HuntRelay huntRelay && this.IsSimilarData(huntRelay);

        /// <summary>
        /// Check if this hunt is under attack
        /// </summary>
        public override bool IsTouched() => this.CurrentHp != this.MaxHp;

        /// <summary>
        /// Check if this hunt hp roughly equals another's hp
        /// </summary>
        public bool HpPercentRoughlyEquals(HuntRelay relay) => this.HpPercentRoughlyEquals(relay.Progress);

        /// <summary>
        /// Check if this hunt hp roughly equals another's hp
        /// </summary>
        public bool HpPercentRoughlyEquals(float otherHpPercent)
        {
            var thisHpPercent = this.HpPercent;
            if (thisHpPercent is 0 or 100 || otherHpPercent is 0 or 100) return thisHpPercent == otherHpPercent;
            return thisHpPercent.RoughlyEquals(otherHpPercent, RoughHpPercent);
        }

        [JsonIgnore]
        [IgnoreMember]
        public override double DuplicateThreshold => EarthSecond * 15;

        public override bool UpdateWith(Relay relay)
        {
            if (relay is HuntRelay huntRelay)
            {
                this.UpdateWith(huntRelay);
                return true;
            }
            return false;
        }

        public void UpdateWith(HuntRelay relay)
        {
            base.UpdateWith(relay);
            if (this.IsDead() || this.Players < relay.Players) this.Players = relay.Players;
            this.Coords = relay.Coords;
            this.ActorId = relay.ActorId;
            this.CurrentHp = relay.CurrentHp;
            this.MaxHp = relay.MaxHp;
        }

        [IgnoreMember]
        [JsonProperty]
        public override string Type => "Hunt";

        protected override bool IsValidImpl(WorldRow world, ZoneRow zone)
        {
            if (this.ActorId == InvalidActorId) return false;
            return Database.Hunts.TryGetValue(this.Id, out var hunt) &&
                hunt.SpawnZoneIds.Contains(this.ZoneId);
        }

        public override string ToString() => $"Rank {this.GetRank()}: {this.GetHunt()} {this.HpPercent:F2}%% {base.ToString()}{(this.IsDead() ? " DEAD" : "")}";

        public new HuntRelay Clone() => Unsafe.As<HuntRelay>(this.MemberwiseClone());
    }
}
