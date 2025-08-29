using MessagePack;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Enums;
using System;
using Sonar.Data.Rows;
using static Sonar.SonarConstants;
using Sonar.Numerics;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using SonarUtils;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Sonar.Localization;
using AG.EnumLocalization;

namespace Sonar.Relays
{
    /// <summary>
    /// Represent a Hunt relay
    /// </summary>
    [MessagePackObject]
    [Serializable]
    [RelayType(RelayType.Hunt)]
    public sealed class HuntRelay : Relay
    {
        protected override string GetRelayKeyImpl()
        {
            return this.GetRank() switch
            {
                HuntRank.SSMinion => ZString.Format("{0}_{1}_{2:X8}", base.GetRelayKeyImpl(), this.ZoneId, this.ActorId),
                HuntRank.SS => ZString.Format("{0}_{1}", base.GetRelayKeyImpl(), this.ZoneId),
                _ => base.GetRelayKeyImpl()
            };
        }

        protected override string GetSortKeyImpl() => this.GetHunt()?.Name.ToString().ToLowerInvariant() ?? base.GetSortKeyImpl();
        protected override IRelayDataRow? GetRelayInfoImpl() => this.GetHunt();

        /// <summary>
        /// Actor ID
        /// </summary>
        [Key(6)]
        public uint ActorId { get; set; }

        /// <summary>
        /// Current HP
        /// </summary>
        [Key(7)]
        public uint CurrentHp { get; set; } = 1;

        /// <summary>
        /// Max Hp
        /// </summary>
        [Key(8)]
        public uint MaxHp { get; set; } = 1;

        /// <summary>
        /// HP Percentage
        /// </summary>
        [IgnoreMember]
        [JsonIgnore]
        public float HpPercent => 100f * ((float)this.CurrentHp / this.MaxHp);

        /// <summary>
        /// Kill Progress
        /// </summary>
        [IgnoreMember]
        [JsonIgnore]
        public float Progress => 100f - this.HpPercent;

        #region Status functions
        /// <summary>
        /// Check if hunt is unharmed
        /// </summary>
        [IgnoreMember]
        [JsonIgnore]
        public bool IsMaxHp => this.CurrentHp == this.MaxHp;

        /// <summary>
        /// Check if hunt is pulled (or harmed in this case)
        /// </summary>
        [IgnoreMember]
        [JsonIgnore]
        public bool IsPulled => this.CurrentHp != this.MaxHp;

        /// <summary>
        /// Players Nearby Count
        /// </summary>
        [Key(9)]
        public int Players { get; set; }

        /// <summary>Client provided, server-side validation only</summary>
        /// <remarks>Please fill this with <see cref="SyncedUnixNow"/>. Server will zero out this field.</remarks>
        [Key(11)]
        [JsonIgnore]
        public double CheckTimestamp { get; set; }

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
        public bool IsSimilarData(HuntRelay relay) => this.HpPercentRoughlyEquals(relay.HpPercent) && this.Coords.Delta(relay.Coords).LengthSquared() < RoughDistanceSquared && this.Players >= relay.Players;

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
        [SuppressMessage("Major Bug", "S1244", Justification = "Intended")]
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
        public override string Type => "Hunt";

        public override bool TryGetValue(ReadOnlySpan<char> name, [MaybeNullWhen(false)] out ReadOnlySpan<char> value)
        {
            var result = name switch
            {
                "status" => this.CurrentHp == this.MaxHp ? RelayStatus.Healthy.GetLocString() : this.CurrentHp > 0 ? RelayStatus.Pulled.GetLocString() : this.CurrentHp == 0 ? RelayStatus.Dead.GetLocString() : null,
                "hpp" => $"{this.HpPercent:F2}%",

                "players" => StringUtils.GetNumber(this.Players),

                "curhp" => this.CurrentHp.ToString(),
                "maxhp" => this.MaxHp.ToString(),

                "actorid" or "objectid" => $"{this.ActorId:X8}",

                _ => null
            };

            if (result is not null)
            {
                value = result;
                return true;
            }
            return base.TryGetValue(name, out value);
        }

        protected override bool IsValidImpl(WorldRow world, ZoneRow zone)
        {
            if (this.ActorId == InvalidActorId) return false;
            return Database.Hunts.TryGetValue(this.Id, out var hunt) &&
                hunt.ZoneIds.Contains(this.ZoneId);
        }

        public override string ToString() => $"Rank {this.GetRank()}: {this.GetHunt()} {this.HpPercent:F2}% {base.ToString()}{(this.IsDead() ? " DEAD" : "")}";

        public new HuntRelay Clone() => Unsafe.As<HuntRelay>(this.MemberwiseClone());
    }
}
