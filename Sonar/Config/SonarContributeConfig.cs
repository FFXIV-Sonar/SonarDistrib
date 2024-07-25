using MessagePack;
using Newtonsoft.Json;
using Sonar.Enums;
using Sonar.Messages;
using Sonar.Relays;
using SonarUtils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sonar.Config
{
    [MessagePackObject]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SonarContributeConfig : ISonarMessage
    {
        private SonarClient? _client;
        private bool _global = true;
        private SonarJurisdiction _jurisdiction = SonarJurisdiction.Datacenter;

        /// <summary>Contains contribution config for individual <see cref="RelayType"/>s.</summary>
        [Key(0)]
        [JsonProperty]
        public HashSet<RelayType> Disabled { get; init; } = [];

        /// <summary>Set to <see langword="false"/> to disable all contribution.</summary>
        [Key(1)]
        [JsonProperty]
        public bool Global
        {
            get => this._global;
            set
            {
                if (this._global == value) return;
                this._global = value;
                this._client?.Connection.SendIfConnected(this);
            }
        }

        /// <summary>Jurisdiction to receive from Server.</summary>
        [Key(2)]
        [JsonProperty]
        public SonarJurisdiction ReceiveJurisdiction
        {
            get => this._jurisdiction;
            set
            {
                if (this._jurisdiction == value) return;
                this._jurisdiction = value;
                this._client?.Connection.SendIfConnected(this);
            }
        }

        /// <summary>Get a compute contribute setting for a specific <see cref="RelayType"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compute(RelayType type)
        {
            if (!this.Global) return false;
            return !this.Disabled.Contains(type);
        }

        [IgnoreMember]
        public bool this[RelayType type]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !this.Disabled.Contains(type);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (this.SetInternal(type, value)) this._client?.Connection.SendIfConnected(this);
            }
        }

        private bool SetInternal(RelayType type, bool value) => value ? this.Disabled.Remove(type) : this.Disabled.Add(type);

        internal void BindClient(SonarClient? client)
        {
            this._client = client;
        }

        public void ReadFrom(SonarContributeConfig config)
        {
            this.Disabled.Clear();
            this.Disabled.AddRange(config.Disabled);
            this.Global = config.Global;
            this.ReceiveJurisdiction = config.ReceiveJurisdiction;
        }

        public void Reset()
        {
            this.Disabled.Clear();
            this.Global = true;
        }
    }
}
