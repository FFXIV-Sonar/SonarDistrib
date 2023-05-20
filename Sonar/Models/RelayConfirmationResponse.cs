using MessagePack;
using Sonar.Messages;
using System.Runtime.CompilerServices;
using Sonar.Trackers;
using Sonar.Relays;

namespace Sonar.Models
{
    [MessagePackObject]
    public sealed class RelayConfirmationResponse<T> : RelayConfirmationBase<T>, ISonarMessage where T : Relay
    {
        public RelayConfirmationResponse() : base() { }
        public RelayConfirmationResponse(uint worldId, uint zoneId, uint instanceId, uint relayId) : base(worldId, zoneId, instanceId, relayId) { }
        public RelayConfirmationResponse(T relay) : base(relay) { }
        public RelayConfirmationResponse(RelayState<T> state) : base(state) { }
        public RelayConfirmationResponse(RelayConfirmationBase confirmation) : base(confirmation) { }
    }
}
