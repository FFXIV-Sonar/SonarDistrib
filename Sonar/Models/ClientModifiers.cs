using Sonar.Messages;
using MessagePack;
using System.Collections.Generic;
using Sonar.Relays;
using System;

namespace Sonar.Models
{
    /// <summary>Manipulate certain <see cref="SonarClient"/> functionalities</summary>
    [MessagePackObject]
    public sealed class ClientModifiers : ISonarMessage
    {
        /// <summary>Default set of client modifiers</summary>
        public static ClientModifiers Defaults => new()
        {
            EnableRelayEventHandlers = true,
            MinimumTickInterval = 100, // TODO: Not implemented (due to performance issues)
            AllowContribute = true,
            AllowContribute2 = new Dictionary<RelayType, bool>() { { RelayType.Hunt, true }, { RelayType.Fate, true }, { RelayType.Manual, true } },
            AllowManualRelays = true, // TODO: Not implemented
        };

        /// <summary>Enable event triggers</summary>
        [Key(0)]
        public bool? EnableRelayEventHandlers { get; set; }

        /// <summary>Minimum tick interval (TODO)</summary>
        [Key(1)]
        public double? MinimumTickInterval { get; set; }

        /// <summary>Allow contributions to happen</summary>
        [Key(2)]
        [Obsolete("Last version with this: v0.6.2.2")]
        public bool? AllowContribute { get; set; }

        [Key(4)]
        public IDictionary<RelayType, bool>? AllowContribute2 { get; set; }

        /// <summary>Allow client to send manual relays (TODO)</summary>
        [Key(3)]
        public bool? AllowManualRelays { get; set; }

        /// <summary>Mutates this <see cref="ClientModifiers"/> with another <see cref="ClientModifiers"/></summary>
        public void MutateWith(ClientModifiers source)
        {
            if (source.EnableRelayEventHandlers.HasValue) this.EnableRelayEventHandlers = source.EnableRelayEventHandlers;
            if (source.MinimumTickInterval.HasValue) this.MinimumTickInterval = source.MinimumTickInterval.Value;
            if (source.AllowContribute.HasValue) this.AllowContribute = source.AllowContribute.Value;
            if (source.AllowContribute2 is not null)
            {
                this.AllowContribute2 ??= new Dictionary<RelayType, bool>();
                foreach (var (type, value) in source.AllowContribute2) this.AllowContribute2[type] = value;
            }
            if (source.AllowManualRelays.HasValue) this.AllowManualRelays = source.AllowManualRelays.Value;
        }

        public override string ToString()
        {
            List<string> ret = new(8);
            if (this.EnableRelayEventHandlers.HasValue) ret.Add($"Relay Events: {this.EnableRelayEventHandlers}");
            if (this.MinimumTickInterval.HasValue) ret.Add($"Tick Interval: {this.MinimumTickInterval}");
            if (this.AllowContribute.HasValue) ret.Add($"Allow Contribute: {this.AllowContribute}");
            if (this.AllowContribute2 is not null)
            {
                foreach (var (type, value) in this.AllowContribute2) ret.Add($"Allow {type} Contribute: {value}");
            }
            if (this.AllowManualRelays.HasValue) ret.Add($"Allow Manual Relays: {this.AllowManualRelays}");
            return string.Join(" | ", ret);
        }
    }
}
