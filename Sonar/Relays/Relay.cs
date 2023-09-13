using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonar.Data.Extensions;
using Sonar.Data;
using Newtonsoft.Json;
using MessagePack;
using Sonar.Messages;
using Sonar.Data.Rows;
using Sonar.Utilities;
using static Sonar.Utilities.UnixTimeHelper;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using Sonar.Models;

namespace Sonar.Relays
{
    /// <summary>
    /// Represents a relay
    /// </summary>
    [Serializable]
    [MessagePackObject]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Relay : GamePosition, IRelay, ISonarMessage, IEquatable<Relay>
    {
        private string? _relayKey;
        private string? _sortKey;
        private uint _id;
        private int? _hash;

        protected override void ResetKeyCache()
        {
            this._relayKey = null;
            this._sortKey = null;
            base.ResetKeyCache();
        }

        /// <summary>
        /// Relay Key
        /// </summary>
        [JsonProperty("Key")]
        [IgnoreMember]
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

        /// <summary>
        /// Relay ID (Hunt ID, Fate ID, Player ID)
        /// </summary>
        [JsonProperty]
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
        [JsonProperty]
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

        [JsonIgnore]
        [Key(5)]
        public ReleaseMode Release { get; set; } = ReleaseMode.Normal;
        #endregion

        public override int GetHashCode() => this._hash ??= FarmHashStringComparer.GetHashCodeStatic(this.RelayKey);
        public bool Equals(Relay? relay) => relay is not null && (ReferenceEquals(this, relay) || this.RelayKey.Equals(relay.RelayKey));
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is Relay relay && this.RelayKey.Equals(relay.RelayKey));

        public static bool operator ==(Relay left, Relay right) => left.Equals(right);
        public static bool operator !=(Relay left, Relay right) => !left.Equals(right);
    }
}
