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

        /// <summary>Get tracker for <typeparamref name="T"/></summary>
        public RelayTracker<T>? GetTracker<T>() where T : Relay
        {
            var type = typeof(T);
            if (type == typeof(HuntRelay)) return Unsafe.As<RelayTracker<T>>(this.Hunts);
            if (type == typeof(FateRelay)) return Unsafe.As<RelayTracker<T>>(this.Fates);
            return default;
        }
    }
}
