using System;
using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;
using MessagePack;
using Sonar.Data.Extensions;
using Sonar.Enums;
using System.Linq;
using static Sonar.Utilities.UnixTimeHelper;
using static Sonar.SonarConstants;
using Sonar.Numerics;
using Sonar.Data.Rows;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using SonarUtils;
using System.Text.Json.Serialization;
using AG.EnumLocalization;

namespace Sonar.Relays
{
    /// <summary>
    /// Represents a Fate relay
    /// </summary>
    [MessagePackObject]
    [Serializable]
    [RelayType(RelayType.Fate)]
    public sealed class FateRelay : Relay
    {
        public const double DefaultGracePeriod = EarthSecond * 15;
        public const int TimeMultiple = (int)EarthSecond;

        #region Validity Functions
        private static readonly IReadOnlySet<FateStatus> s_validStatus = Enum.GetValues<FateStatus>().ToHashSet();
        public static bool IsValidStatus(FateStatus status) => s_validStatus.Contains(status);
        public bool IsValidStatus() => IsValidStatus(this._status);

        public static bool IsValidProgress(byte progress) => progress <= 100;
        public bool IsValidProgress() => IsValidProgress(this._progress);

        public static bool IsValid(FateRelay relay)
        {
            if (!relay.IsValidStatus() || !relay.IsValidProgress()) return false;

            var i = relay.GetFate();
            if (i is null) return false;
            if (relay.ZoneId != i.ZoneId) return false;

            return true;
        }
        protected override bool IsValidImpl(WorldRow world, ZoneRow zone) => IsValid(this);

        #endregion

        protected override string GetSortKeyImpl() => this.GetFate()?.Name.ToString().ToLowerInvariant() ?? base.GetSortKeyImpl();
        protected override IRelayDataRow? GetRelayInfoImpl() => this.GetFate();

        private byte _progress;
        /// <summary>
        /// Fate Progress (0-100)
        /// </summary>
        [Key(6)]
        public byte Progress
        {
            get => this._progress;
            set
            {
                if (!IsValidProgress(value)) throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} must be within 0 - 100");
                this._progress = value;
            }
        }

        private FateStatus _status;
        /// <summary>
        /// Fate status
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public FateStatus Status
        {
            get => this.GetStatus(SyncedUnixNow);
            set
            {
                if (!IsValidStatus(value)) throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} is not a valid status");
                this._status = value;
            }
        }

        /// <summary>
        /// Fate Status (might be outdated if <see cref="FateStatus.Running"/>)
        /// </summary>
        /// <remarks>
        /// Please use <see cref="Status"/> instead for most purposes.
        /// </remarks>
        [JsonPropertyName("Status")]
        [Key(7)]
        [SuppressMessage("Minor Code Smell", "S2292", Justification = "For now I want to leave this here until I change how this works.")]
        public FateStatus StatusDirect
        {
            get => this._status;
            set => this._status = value;
        }

        public FateStatus GetStatus(double now)
        {
            if (this._status is not FateStatus.Running) return this._status;
            if (this.Progress == 100) return this._status = FateStatus.Complete;
            if (this.GetRemainingTimeWithGracePeriod(DefaultGracePeriod, now) is 0) return this._status = FateStatus.Unknown;
            return this._status;
        }

        /// <summary>
        /// Fate start time
        /// </summary>
        [IgnoreMember]
        public double StartTime { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonIgnore]
        [Key(8)]
        public long msgPackStartTime
        {
            get => unchecked((long)this.StartTime / TimeMultiple);
            set => this.StartTime = value * TimeMultiple;
        }

        /// <summary>
        /// Fate duration
        /// </summary>
        [IgnoreMember]
        public double Duration { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonIgnore]
        [Key(9)]
        public int MsgPackDuration
        {
            get => unchecked((int)this.Duration / TimeMultiple);
            set => this.Duration = value * TimeMultiple;
        }

        /// <summary>Players Nearby Count</summary>
        [Key(10)]
        public int Players { get; set; }

        /// <summary>Client provided, server-side validation only</summary>
        /// <remarks>Please fill this with <see cref="SyncedUnixNow"/>. Server will zero out this field.</remarks>
        [Key(11)]
        [JsonIgnore] // Server-only purpose
        public double CheckTimestamp { get; set; }

        /// <inheritdoc/>
        [Key(12)]
        public bool Bonus { get; set; }

        /// <summary>
        /// Fate's ending time
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public double EndTime => this.StartTime + this.Duration;

        /// <summary>
        /// Check if this fate is alive
        /// </summary>
        public override bool IsAlive() => this.Status is FateStatus.Running or FateStatus.Preparation;

        /// <summary>
        /// Check if this fate is alive (might be outdated if <see cref="StatusDirect"/> is <see cref="FateStatus.Running"/>)
        /// </summary>
        /// <remarks>
        /// Please use <see cref="IsAlive"/> instead for most purposes.
        /// </remarks>
        internal override bool IsAliveInternal() => this._status is FateStatus.Running or FateStatus.Preparation;

        /// <summary>
        /// Remaining time in milliseconds
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public double RemainingTime => Math.Max(this.EndTime - SyncedUnixNow, 0);

        private double GetRemainingTimeWithGracePeriod(double gracePeriod = DefaultGracePeriod) => this.GetRemainingTimeWithGracePeriod(gracePeriod, SyncedUnixNow);
        private double GetRemainingTimeWithGracePeriod(double gracePeriod, double now) => Math.Max(this.EndTime - now + gracePeriod, 0);

        /// <summary>
        /// Get remaining time and progress or completed / failed
        /// </summary>
        /// <returns></returns>
        public string GetRemainingTimeAndProgressString() => this.Status == FateStatus.Running ? $"{this.GetRemainingTimeString()} {this.Progress}%" : string.Empty;

        /// <summary>
        /// Get remaining time in MM:SS
        /// </summary>
        /// <returns>MM:SS</returns>
        public string GetRemainingTimeString()
        {
            var seconds = (int)(this.RemainingTime / 1000);

            var minutes = seconds / 60;
            seconds %= 60;

            var hours = minutes / 60;
            minutes %= 60;

            var hoursStr = hours > 0 ? $"{hours}:" : string.Empty;
            var minutesStr = hours > 0 ? $"{minutes:D2}:" : $"{minutes}:";
            var secondsStr = $"{seconds:D2}";

            return $"{hoursStr}{minutesStr}{secondsStr}";
        }

        /// <summary>
        /// Check if another relay regards the same fate (only Start time is checked)
        /// </summary>
        /// <param name="relay">Relay to check</param>
        [SuppressMessage("Major Bug", "S1244", Justification = "Intended")]
        public bool IsSameEntity(FateRelay relay) => this.StatusDirect == FateStatus.Preparation && this.StartTime is 0 || this.StartTime is not 0 && this.StartTime == relay.StartTime;

        /// <summary>
        /// Check if another relay regards the same fate (only Start time is checked)
        /// </summary>
        /// <param name="relay">Relay to check</param>
        public override bool IsSameEntity(Relay relay) => relay is FateRelay fateRelay && this.IsSameEntity(fateRelay);

        /// <summary>
        /// Check if this fate progress roughly equals another's progress
        /// </summary>
        public bool ProgressRoughlyEquals(FateRelay relay) => this.ProgressRoughlyEquals(relay.Progress);

        /// <summary>
        /// Check if this fate progress roughly equals another's progress
        /// </summary>
        public bool ProgressRoughlyEquals(byte otherProgress)
        {
            var thisProgress = this.Progress;
            if (thisProgress is 0 or 100 || otherProgress is 0 or 100) return thisProgress == otherProgress;
            return thisProgress.RoughlyEquals(otherProgress, RoughFateProgress);
        }

        /// <summary>
        /// Check if another relay is similar (only Progress, Coords, Status and Start time are checked)
        /// </summary>
        public bool IsSimilarData(FateRelay relay, double now) => this.ProgressRoughlyEquals(relay.Progress) && this.Coords.Delta(relay.Coords).LengthSquared() < RoughDistanceSquared && this.GetStatus(now) == relay.GetStatus(now) && this.Players >= relay.Players;

        /// <summary>
        /// Check if another relay is similar (only Progress, Coords, Status and Start time are checked)
        /// </summary>
        public bool IsSimilarData(FateRelay relay) => this.IsSimilarData(relay, SyncedUnixNow);

        /// <summary>
        /// Check if another relay is similar (only Progress, Coords, Status and Start time are checked)
        /// </summary>
        public override bool IsSimilarData(Relay relay, double now) => relay is FateRelay fateRelay && this.IsSimilarData(fateRelay);

        /// <summary>
        /// Check if this fate is being progressed
        /// </summary>
        public override bool IsTouched() => this.Progress > 0;

        [JsonIgnore]
        [IgnoreMember]
        public override double DuplicateThreshold => EarthMinute * 15;

        public override bool UpdateWith(Relay relay)
        {
            if (relay is FateRelay fateRelay)
            {
                this.UpdateWith(fateRelay);
                return true;
            }
            return false;
        }

        public void UpdateWith(FateRelay relay)
        {
            base.UpdateWith(relay);
            if (this.IsDead() || this.Players < relay.Players) this.Players = relay.Players;
            this.Coords = relay.Coords;
            this.StartTime = relay.StartTime;
            this.Progress = relay.Progress;
            this.Duration = relay.Duration;
            this.StatusDirect = relay.StatusDirect;
            this.Bonus = relay.Bonus;
        }

        public override bool TryGetValue(ReadOnlySpan<char> name, [MaybeNullWhen(false)] out ReadOnlySpan<char> value)
        {
            var result = name switch
            {
                "status" => this.Status.GetLocString(),
                "progress" => $"{StringUtils.GetNumber(this.Progress)}%",

                "players" => StringUtils.GetNumber(this.Players),

                "time" => this.GetRemainingTimeString(),

                "tap" => this.GetRemainingTimeAndProgressString(), // TAP => Time And Progress

                _ => null
            };

            if (result is not null)
            {
                value = result;
                return true;
            }
            return base.TryGetValue(name, out value);
        }


        [IgnoreMember]
        public override string Type => "Fate";

        public override string ToString() => $"{this.GetFate()}{(this.Bonus ? " [B]" : string.Empty)}: {base.ToString()} {this.GetRemainingTimeAndProgressString()}";
        public new FateRelay Clone() => Unsafe.As<FateRelay>(this.MemberwiseClone());
    }
}