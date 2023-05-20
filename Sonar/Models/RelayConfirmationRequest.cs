using MessagePack;
using Sonar.Messages;
using System.Runtime.CompilerServices;
using Sonar.Trackers;
using Sonar.Relays;

namespace Sonar.Models
{
    [MessagePackObject]
    public sealed class RelayConfirmationRequest<T> : RelayConfirmationBase<T>, ISonarMessage where T : Relay
    {
        public RelayConfirmationRequest() : base() { }
        public RelayConfirmationRequest(uint worldId, uint zoneId, uint instanceId, uint relayId) : base(worldId, zoneId, instanceId, relayId) { }
        public RelayConfirmationRequest(T relay) : base(relay) { }
        public RelayConfirmationRequest(RelayState<T> state) : base(state) { }
        public RelayConfirmationRequest(RelayConfirmationBase confirmation) : base(confirmation) { }
    }
}
