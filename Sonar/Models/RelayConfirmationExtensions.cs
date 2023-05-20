using Sonar.Relays;
using Sonar.Trackers;

namespace Sonar.Models
{
    internal static class RelayConfirmationExtensions
    {
        public static RelayConfirmationBase<HuntRelay> AsConfirmation(this HuntRelay relay) => new(relay);
        public static RelayConfirmationBase<HuntRelay> AsConfirmation(this RelayState<HuntRelay> state) => state.Relay.AsConfirmation();
        public static RelayConfirmationBase<FateRelay> AsConfirmation(this FateRelay relay) => new(relay);
        public static RelayConfirmationBase<FateRelay> AsConfirmation(this RelayState<FateRelay> state) => state.Relay.AsConfirmation();

        public static GamePlace AsGamePlace<T>(this RelayConfirmationBase<T> obj) where T : Relay => new() { WorldId = obj.WorldId, ZoneId = obj.ZoneId, InstanceId = obj.InstanceId };
    }
}
