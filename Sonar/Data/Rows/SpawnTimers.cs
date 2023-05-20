using MessagePack;
using System;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Hunt respawn timers
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public struct SpawnTimers
    {
        public SpawnTimers(TimerRange normal) : this(normal, new()) { }

        public SpawnTimers(TimerRange normal, TimerRange maintenance)
        {
            this.Normal = normal;
            this.Maintenance = maintenance;
        }

        [Key(0)]
        public TimerRange Normal { get; }
        [Key(1)]
        public TimerRange Maintenance { get; }

        public override string ToString() => $"Normal: {this.Normal} | Maintenance: {this.Maintenance}";
    }
}
