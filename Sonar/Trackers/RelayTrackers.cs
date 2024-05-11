using DryIocAttributes;
using Sonar.Relays;
using System;
using System.Runtime.CompilerServices;

namespace Sonar.Trackers
{
    /// <summary>Contains all relay trackers</summary>
    [SingletonReuse]
    [ExportEx]
    public sealed class RelayTrackers
    {
        /// <summary>Hunt Tracker</summary>
        public HuntTracker Hunts { get; }

        /// <summary>Fate Tracker</summary>
        public FateTracker Fates { get; }

        /// <summary>Relay trackers utilities</summary>
        public RelayTrackersUtils Utils { get; }

        internal RelayTrackers(HuntTracker hunts, FateTracker fates, RelayTrackersUtils utils)
        {
            this.Hunts = hunts;
            this.Fates = fates;
            this.Utils = utils;
        }

        /// <summary>Get a relay tracker of a specified <paramref name="type"/></summary>
        public IRelayTracker? GetTracker(RelayType type)
        {
            if (type == RelayType.Hunt) return this.Hunts;
            if (type == RelayType.Fate) return this.Fates;
            return default;
        }

        /// <summary>Get a relay tracker of a specified <paramref name="type"/></summary>
        public IRelayTracker? GetTracker(Type type)
        {
            return this.GetTracker(RelayUtils.GetRelayType(type));
        }

        /// <summary>Get a relay tracker for <typeparamref name="T"/></summary>
        public RelayTracker<T>? GetTracker<T>() where T : Relay
        {
            return Unsafe.As<RelayTracker<T>>(this.GetTracker(typeof(T)));
        }
    }
}
