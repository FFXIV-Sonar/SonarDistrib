using System;
using Sonar.Data.Extensions;
using MessagePack;
using Sonar.Messages;
using Sonar.Data.Rows;
using SonarUtils;
using static Sonar.Utilities.UnixTimeHelper;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using Sonar.Models;
using SonarUtils.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Sonar.Relays
{
    /// <summary>
    /// Represents a relay
    /// </summary>
    [Serializable]
    [MessagePackObject]
    [Union(0, typeof(Relay))]
    [SuppressMessage("Major Code Smell", "S4035")]
    public abstract class Relay : GamePosition, IRelay, ISonarMessage, IEquatable<Relay>
    {
        private string? _relayKey;
        private string? _sortKey;
        private uint _id;
        private int? _hash;
        private IRelayDataRow? _info;

        protected override void ResetKeyCache()
        {
            this._relayKey = null;
            this._sortKey = null;
            base.ResetKeyCache();
        }

        /// <summary>
        /// Relay Key
        /// </summary>
        [IgnoreMember]
        [JsonPropertyName("Key")]
        public string RelayKey => this._relayKey ??= StringUtils.Intern(this.GetRelayKeyImpl());

        protected virtual string GetRelayKeyImpl() => ZString.Format("{0}_{1}_{2}", this.WorldId, this.Id, this.InstanceId);

        /// <summary>
        /// Sort Key
        /// </summary>
        /// <remarks>NOTE/TODO: this is currently not reliable if changing Sonar languages half-way, due to caching. Might need to work around this.</remarks>
        [JsonIgnore]
        [IgnoreMember]
        public string SortKey => this._sortKey ??= StringUtils.Intern(this.GetSortKeyImpl());

        protected virtual string GetSortKeyImpl() => ZString.Format("{0}_{1}_{2}", this.Id, this.WorldId, this.InstanceId);


        /// <summary>Relay Information</summary>
        [JsonIgnore]
        [IgnoreMember]
        public IRelayDataRow Info => this._info ??= (this.GetRelayInfoImpl() ?? EmptyRelayRow.Instance);
        protected abstract IRelayDataRow? GetRelayInfoImpl();

        /// <summary>
        /// Relay ID (Hunt ID, Fate ID, Player ID)
        /// </summary>
        [Key(4)]
        public uint Id
        {
            get => this._id;
            set
            {
                if (this._id == value) return;
                this._id = value;
                this.ResetKeyCache();
            }
        }

        #region Abstract stuff
        /// <summary>
        /// Relay type
        /// </summary>
        [IgnoreMember]
        public virtual string Type => "Relay";

        /// <summary>
        /// Check if another relay regards the same thing
        /// </summary>
        /// <param name="relay">Relay to check</param>
        public abstract bool IsSameEntity(Relay relay);

        /// <summary>
        /// Check if another relay is similar
        /// </summary>
        /// <param name="relay">Relay to check</param>
        public bool IsSimilarData(Relay relay) => this.IsSimilarData(relay, SyncedUnixNow);

        /// <summary>
        /// Check if another relay is similar
        /// </summary>
        /// <param name="relay">Relay to check</param>
        public abstract bool IsSimilarData(Relay relay, double now);

        protected virtual bool IsValidImpl(WorldRow world, ZoneRow zone) => true;

        /// <summary>
        /// Check if this relay is alive
        /// </summary>
        public virtual bool IsAlive() => true;

        /// <summary>
        /// Check if this relay is alive. This virtual method is specifically for fates.
        /// </summary>
        internal virtual bool IsAliveInternal() => this.IsAlive();

        /// <summary>
        /// Check if this relay is touched (pulled, under attack, etc)
        /// </summary>
        public virtual bool IsTouched() => true;

        /// <summary>
        /// Duplicate threshold in milliseconds
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public virtual double DuplicateThreshold => 0;
        #endregion

        #region Internal Implementation
        /// <summary>
        /// Check if this relay is dead
        /// </summary>
        public bool IsDead() => !this.IsAlive();

        /// <summary>
        /// Check if this relay is dead. This function is specifically for fates.
        /// </summary>
        internal bool IsDeadInternal() => !this.IsAliveInternal();

        /// <summary>
        /// Check if this relay is untouched (not pulled yet)
        /// </summary>
        public bool IsUntouched() => !this.IsTouched();

        /// <summary>
        /// Update this relay with another
        /// </summary>
        public virtual bool UpdateWith(Relay relay)
        {
            if (this.IsDeadInternal() || this.Release < relay.Release) this.Release = relay.Release;
            return true;
        }

        /// <summary>
        /// Check if this is a valid relay
        /// </summary>
        public new bool IsValid()
        {
            if (this.WorldId == 0 || this.ZoneId == 0) return false;
            if (!IsValidInstance(this.InstanceId)) return false;

            var world = this.GetWorld();
            if (world is null || !world.IsPublic) return false;

            var zone = this.GetZone();
            if (zone is null || !zone.IsField) return false;

            return this.IsValidImpl(world, zone);
        }

        public new Relay Clone() => Unsafe.As<Relay>(this.MemberwiseClone());

        public override bool TryGetValue(ReadOnlySpan<char> name, [MaybeNullWhen(false)] out ReadOnlySpan<char> value)
        {
            var result = name switch
            {
                "key" or "relaykey" => this.RelayKey,
                "id" or "relayid" => StringUtils.GetNumber(this.Id),

                "name" => this.Info.Name.ToString(),
                "rank" => this.Info.Rank.ToString(),
                "level" => StringUtils.GetNumber(this.Info.Level),

                "status" => this.IsAliveInternal() ? "Alive" : this.IsDeadInternal() ? "Dead" : "Unknown",

                _ => null
            };

            if (result is not null)
            {
                value = result;
                return true;
            }
            return base.TryGetValue(name, out value);
        }

        [Key(5)]
        public ReleaseMode Release { get; set; } = ReleaseMode.Normal;
        #endregion

        [SuppressMessage("Minor Bug", "S2328", Justification = "Not mutable")]
        public override int GetHashCode() => this._hash ??= FarmHashStringComparer.GetHashCodeStatic(this.RelayKey);
        public bool Equals(Relay? relay) => Equals(this, relay);

        public static new bool Equals(object? left, object? right)
        {
            if (left is not Relay leftRelay || right is not Relay rightRelay) return object.Equals(left, right);
            return Equals(leftRelay, rightRelay);
        }
        public static bool Equals(Relay? left, Relay? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            if (left.GetType() != right.GetType()) return false;
            return string.Equals(left.RelayKey, right.RelayKey);
        }
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is Relay relay && Equals(this, relay));

        public static bool operator ==(Relay left, Relay right) => Equals(left, right);
        public static bool operator !=(Relay left, Relay right) => !Equals(left, right);
    }
}
