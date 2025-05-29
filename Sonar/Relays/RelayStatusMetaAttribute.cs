using System;
using System.Collections.Frozen;

namespace Sonar.Relays
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class RelayStatusMetaAttribute : Attribute
    {
        public RelayStatusMetaAttribute(bool isAlive, bool isHealthy, bool isPulled, bool isDead, bool isStale, params RelayType[] types)
        {
            this.IsAlive = isAlive;
            this.IsHealthy = isHealthy;
            this.IsPulled = isPulled;
            this.IsDead = isDead;
            this.IsStale = isStale;
            this.Types = types.ToFrozenSet();
        }

        // Make it easier for me (1, 0, 0, 0) instead of (true, false, false, false)
        public RelayStatusMetaAttribute(int isAlive, int isHealthy, int isPulled, int isDead, int isStale, params RelayType[] types) : this(isAlive != 0, isHealthy != 0, isPulled != 0, isDead != 0, isStale != 0, types) { /* Empty */ }

        /// <summary>Return a value indicating whether this <see cref="RelayStatusMetaAttribute"/> represents an alive status.</summary>
        public bool IsAlive { get; }

        /// <summary>Returns a value indicating whether this <see cref="RelayStatusMetaAttribute"/> represents a healthy status.</summary>
        public bool IsHealthy { get; }

        /// <summary>Return a value indicating whether this <see cref="RelayStatusMetaAttribute"/> represents a pulled status.</summary>
        public bool IsPulled { get; }

        /// <summary>Return a value indicating whether this <see cref="RelayStatusMetaAttribute"/> represents a dead status.</summary>
        public bool IsDead { get; }

        /// <summary>Return a value indicating whether this <see cref="RelayStatusMetaAttribute"/> represents a stale status.</summary>
        public bool IsStale { get; }

        /// <summary><see cref="RelayType"/>s this <see cref="RelayStatusMetaAttribute"/> is valid for.</summary>
        public FrozenSet<RelayType> Types { get; }
    }
}
