using MessagePack;
using Newtonsoft.Json;
using Sonar.Messages;
using System;
using Sonar.Enums;
using Sonar.Data.Extensions;
using System.Runtime.CompilerServices;
using Sonar.Models;

namespace Sonar.Relays
{
    /// <summary>
    /// Represent a parsed player relay
    /// </summary>
    [JsonObject]
    [MessagePackObject]
    [Serializable]
    public sealed class ManualRelay : Relay
    {
        protected override string GetSortKeyImpl() => $"{this.Player?.Name.ToLowerInvariant() ?? "_"} <{this.Player?.GetWorld()?.Name.ToLowerInvariant() ?? this.Player?.HomeWorldId.ToString().ToLowerInvariant() ?? "_"}>";

        /// <summary>
        /// Player that sent this relay (will be set Server side)
        /// </summary>
        [Key(6)]
        public PlayerInfo? Player { get; set; }

        /// <summary>
        /// Reach jurisdiction
        /// </summary>
        [Key(7)]
        public SonarJurisdiction Jurisdiction { get; set; } = SonarJurisdiction.All;

        /// <summary>
        /// Manual relay kind
        /// </summary>
        [Key(8)]
        public ManualRelayKind Kind { get; set; }

        /// <summary>
        /// Message included in this relay
        /// </summary>
        [Key(9)]
        public string Message { get; set; } = string.Empty;

        public override bool IsSameEntity(Relay relay) => relay is ManualRelay m && this.IsSame(m);

        public bool IsSame(ManualRelay relay) => this.Id == relay.Id && this.IsSimilarData(relay);

        public override bool IsSimilarData(Relay relay, double now) => relay is ManualRelay m && this.IsSimilarData(m);

        public bool IsSimilarData(ManualRelay relay) => this.Id == relay.Id && this.WorldId == relay.WorldId && this.ZoneId == relay.WorldId && this.InstanceId == relay.InstanceId;

        public new ManualRelay Clone() => Unsafe.As<ManualRelay>(this.MemberwiseClone());
    }
}
