using Sonar.Messages;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Sonar.Models
{
    /// <summary>
    /// Client Modifiers
    /// </summary>
    // [MessagePackObject] <-- forgot
    [Obsolete("Replace with ClientModifiers", true)]
    public sealed class ClientModifiersOld : ISonarMessage
    {
        public static ClientModifiersOld Defaults => new()
        {
            EnableRelayEventHandlers = true,
            MinimumTickInterval = 100,
            AllowContribute = true,
        };

        // [Key(0)]
        public bool? EnableRelayEventHandlers { get; set; }
        // [Key(1)]
        public double? MinimumTickInterval { get; set; }
        // [Key(2)]
        public bool? AllowContribute { get; set; }
        // [Key(3)]
        public bool? AllowManualRelays { get; set; }

        public void MutateWith(ClientModifiersOld source)
        {
            if (source.EnableRelayEventHandlers.HasValue) this.EnableRelayEventHandlers = source.EnableRelayEventHandlers;
            if (source.MinimumTickInterval.HasValue) this.MinimumTickInterval = source.MinimumTickInterval.Value;
            if (source.AllowContribute.HasValue) this.AllowContribute = source.AllowContribute.Value;
            if (source.AllowManualRelays.HasValue) this.AllowManualRelays = source.AllowManualRelays.Value;
        }

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
