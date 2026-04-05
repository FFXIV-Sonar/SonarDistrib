using MessagePack;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Numerics;
using System.Collections.Generic;
using static Sonar.SonarConstants;

namespace Sonar.Relays
{
    [MessagePackObject]
    public sealed class EventRelay : Relay
    {
        public override string Type => "Event";

        [Key(6)]
        public RelayStatus Status { get; set; }

        [Key(7)]
        public float Progress { get; set; }

        [Key(8)]
        public long StartTime { get; set; }

        [Key(9)]
        public long EndTime { get; set; }

        [Key(10)]
        public int Players { get; set; }

        public override bool IsAlive() => this.Status.IsAlive();

        internal override bool IsAliveInternal() => this.Status.IsAlive();

        protected override bool IsValidImpl(WorldRow world, ZoneRow zone) => this.Progress >= 0 && this.Progress <= 100 && this.Status.IsValidStatus();

        public override double DuplicateThreshold => EarthSecond * 15;

        public override bool IsTouched() => this.Status.IsPulled();

        protected override string GetSortKeyImpl() => Database.Events.GetValueOrDefault(this.Id)?.Name.ToString() ?? $"{this.RelayKey}";

        public bool UpdateWith(EventRelay relay)
        {
            base.UpdateWith(relay);
            if (this.IsDead() || this.Players < relay.Players) this.Players = relay.Players;
            this.Status = relay.Status;
            this.Progress = relay.Progress;
            this.StartTime = relay.StartTime;
            this.EndTime = relay.EndTime;
            return true;
        }

        public bool IsSameEntity(EventRelay relay)
        {
            return this.Id == relay.Id && this.WorldId == relay.WorldId && this.ZoneId == relay.ZoneId && this.InstanceId == relay.InstanceId && this.StartTime == relay.StartTime;
        }

        public override bool IsSameEntity(Relay relay) => relay is EventRelay eventRelay && this.IsSameEntity(eventRelay);

        public bool IsSimilarData(EventRelay eventRelay, double now)
        {
            return this.Progress.RoughlyEquals(eventRelay.Progress);
        }
        public override bool IsSimilarData(Relay relay, double now) => relay is EventRelay eventRelay && this.IsSimilarData(eventRelay);

        protected override IRelayDataRow? GetRelayInfoImpl() => Database.Events.GetValueOrDefault(this.Id);
    }
}
