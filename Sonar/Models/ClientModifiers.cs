using Sonar.Messages;
using MessagePack;
using System.Collections.Generic;

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
        public bool? AllowContribute { get; set; }

        /// <summary>Allow client to send manual relays (TODO)</summary>
        [Key(3)]
        public bool? AllowManualRelays { get; set; }

        /// <summary>Mutates this <see cref="ClientModifiers"/> with another <see cref="ClientModifiers"/></summary>
        public void MutateWith(ClientModifiers source)
        {
            if (source.EnableRelayEventHandlers.HasValue) this.EnableRelayEventHandlers = source.EnableRelayEventHandlers;
            if (source.MinimumTickInterval.HasValue) this.MinimumTickInterval = source.MinimumTickInterval.Value;
            if (source.AllowContribute.HasValue) this.AllowContribute = source.AllowContribute.Value;
            if (source.AllowManualRelays.HasValue) this.AllowManualRelays = source.AllowManualRelays.Value;
        }

        public override string ToString()
        {
            List<string> ret = new(4);
            if (this.EnableRelayEventHandlers.HasValue) ret.Add($"Relay Events: {this.EnableRelayEventHandlers}");
            if (this.MinimumTickInterval.HasValue) ret.Add($"Tick Interval: {this.MinimumTickInterval}");
            if (this.AllowContribute.HasValue) ret.Add($"Allow Contribute: {this.AllowContribute}");
            if (this.AllowContribute.HasValue) ret.Add($"Allow Manual Relays: {this.AllowManualRelays}");
            return string.Join(" | ", ret);
        }
    }
}
