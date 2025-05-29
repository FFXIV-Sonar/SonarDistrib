namespace Sonar.Relays
{
    public static class RelayExtensions
    {
        public static int GetPlayers(this Relay relay)
        {
            if (relay is HuntRelay huntRelay) return huntRelay.Players;
            if (relay is FateRelay fateRelay) return fateRelay.Players;
            return 0;
        }
    }
}
