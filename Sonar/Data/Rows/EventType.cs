using Sonar.Data.Rows.Internal;

namespace Sonar.Data.Rows
{
    /// <summary>Event types</summary>
    public enum EventType : byte
    {
        Unknown = 0,

        [SubRowBits(8)]
        CosmicEmergency = 1,
    }
}
