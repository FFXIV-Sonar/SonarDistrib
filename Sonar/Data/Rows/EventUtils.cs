using System;

namespace Sonar.Data.Rows
{
    public static class EventUtils
    {
        public static uint ToId(EventType type, uint id)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(id, 1u << 24);
            return id + (((uint)type) << 24);
        }
    }
}
