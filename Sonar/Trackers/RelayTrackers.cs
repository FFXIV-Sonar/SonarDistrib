using DryIocAttributes;
using Sonar.Relays;
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

        internal RelayTrackers(HuntTracker hunts, FateTracker fates)
        {
            this.Hunts = hunts;
            this.Fates = fates;
        }

        /// <summary>Get tracker for <typeparamref name="T"/></summary>
        public RelayTracker<T>? GetTracker<T>() where T : Relay
        {
            var type = typeof(T);
            if (type == typeof(HuntRelay)) return Unsafe.As<RelayTracker<T>>(this.Hunts);
            if (type == typeof(FateRelay)) return Unsafe.As<RelayTracker<T>>(this.Fates);
            //if (type == typeof(HuntRelay)) return (RelayTracker<T>?)(IRelayTracker)this.Hunts;
            //if (type == typeof(FateRelay)) return (RelayTracker<T>?)(IRelayTracker)this.Fates;
            return default;
        }
    }
}
