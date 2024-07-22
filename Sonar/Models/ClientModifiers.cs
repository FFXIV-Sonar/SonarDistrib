using Sonar.Messages;
using MessagePack;
using System.Collections.Generic;
using Sonar.Relays;
using System;

namespace Sonar.Models
{
    /// <summary>Manipulate certain <see cref="SonarClient"/> functionalities</summary>
    [MessagePackObject]
    public sealed partial class ClientModifiers : ISonarMessage
    {
        /// <summary>Default set of client modifiers</summary>
        public static ClientModifiers Defaults => new()
        {
            EnableRelayEventHandlers = true,
            MinimumTickInterval = 100, // TODO: Not implemented (due to performance issues)
            GlobalContribute = true,
            AllowContribute = new Dictionary<RelayType, bool>() { { RelayType.Hunt, true }, { RelayType.Fate, true }, { RelayType.Manual, true } },
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
        //[Obsolete("Last version with this: v0.6.2.2")] // Repurposed as GlobalContribute
        public bool? GlobalContribute { get; set; }

        [Key(4)]
        public IDictionary<RelayType, bool>? AllowContribute { get; set; }

        /// <summary>Allow client to send manual relays (TODO)</summary>
        [Key(3)]
        public bool? AllowManualRelays { get; set; }

        /// <summary>Mutates this <see cref="ClientModifiers"/> with another <see cref="ClientModifiers"/></summary>
        public void MutateWith(ClientModifiers source)
        {
            if (source.EnableRelayEventHandlers.HasValue) this.EnableRelayEventHandlers = source.EnableRelayEventHandlers;
            if (source.MinimumTickInterval.HasValue) this.MinimumTickInterval = source.MinimumTickInterval.Value;
            if (source.GlobalContribute.HasValue) this.GlobalContribute = source.GlobalContribute.Value;
            if (source.AllowContribute is not null)
            {
                this.AllowContribute ??= new Dictionary<RelayType, bool>();
                foreach (var (type, value) in source.AllowContribute) this.AllowContribute[type] = value;
            }
            if (source.AllowManualRelays.HasValue) this.AllowManualRelays = source.AllowManualRelays.Value;
        }

        public bool CanContribute(RelayType type)
        {
            if (this.GlobalContribute == false) return false;
            if (this.AllowContribute is null || !this.AllowContribute.TryGetValue(type, out var value)) return true;
            return value;
        }

        public override string ToString()
        {
            List<string> ret = new(8);
            if (this.EnableRelayEventHandlers.HasValue) ret.Add($"Relay Events: {this.EnableRelayEventHandlers}");
            if (this.MinimumTickInterval.HasValue) ret.Add($"Tick Interval: {this.MinimumTickInterval}");
            if (this.GlobalContribute.HasValue) ret.Add($"Allow Contribute: {this.GlobalContribute}");
            if (this.AllowContribute is not null)
            {
                foreach (var (type, value) in this.AllowContribute) ret.Add($"Allow {type} Contribute: {value}");
            }
            if (this.AllowManualRelays.HasValue) ret.Add($"Allow Manual Relays: {this.AllowManualRelays}");
            return string.Join(" | ", ret);
        }
    }
}
