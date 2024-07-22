using DryIoc;
using DryIocAttributes;
using Sonar.Config;
using Sonar.Relays;
using System;
using System.Runtime.CompilerServices;

namespace Sonar.Trackers
{
    /// <summary>Contains all relay trackers</summary>
    [SingletonReuse]
    [ExportEx]
    public sealed class RelayTrackers : IDisposable
    {
        private readonly Lazy<RelayTrackersUtils> _utils;

        public SonarClient Client { get; }

        /// <summary>Hunt Tracker</summary>
        public IRelayTracker<HuntRelay> Hunts { get; }

        /// <summary>Fate Tracker</summary>
        public IRelayTracker<FateRelay> Fates { get; }

        internal SonarConfig Config { get; }

        /// <summary>Relay trackers utilities</summary>
        public RelayTrackersUtils Utils => this._utils.Value;

        internal RelayTrackers(Container container, SonarClient client)
        {
            this.Client = client;
            this.Config = client.Configuration;
            this.Hunts = new RelayTracker<HuntRelay>(this, this.Config.HuntConfig);
            this.Fates = new RelayTracker<FateRelay>(this, this.Config.FateConfig);

            this._utils = new(() => new(this));
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

        public void Dispose()
        {
            ((RelayTracker<HuntRelay>)this.Hunts).Dispose();
            ((RelayTracker<FateRelay>)this.Fates).Dispose();
            if (this._utils.IsValueCreated) this._utils.Value.Dispose();
        }
    }
}
