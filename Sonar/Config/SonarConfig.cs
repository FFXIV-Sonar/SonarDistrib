using MessagePack;
using Newtonsoft.Json;
using Sonar.Enums;
using Sonar.Logging;
using Sonar.Messages;
using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.ComponentModel;

namespace Sonar.Config
{
    public sealed class SonarConfig
    {
        public SonarConfig() { }
        public SonarConfig(SonarConfig c)
        {
            this.LogLevel = c.LogLevel;
            this.HuntConfig = c.HuntConfig;
            this.FateConfig = c.FateConfig;
        }

        internal void BindClient(SonarClient? client)
        {
            this.Contribute.BindClient(client);
        }


        [JsonProperty]
        [Key("version")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        /// <summary>Configuration version. Do not change.</summary>
        public int Version { get; set; }

        /// <summary>Sonar client log level</summary>
        [JsonProperty]
        [Key("logLevel")]
#if DEBUG
        public SonarLogLevel LogLevel { get; set; } = SonarLogLevel.Information; //Info
#else
        public SonarLogLevel LogLevel { get; set; } = SonarLogLevel.Warning; //Warn
#endif

        /// <summary>Hunt reporting configuration</summary>
        [JsonProperty]
        [Key("huntConfig")]
        public HuntConfig HuntConfig { get; init; } = new();

        /// <summary>Fate reporting configuration</summary>
        [JsonProperty]
        [Key("fateConfig")]
        public FateConfig FateConfig { get; init; } = new();

        /// <summary>Contributing configuration</summary>
        [JsonProperty]
        [Key("server")] // "contribute" but too late to change
        public SonarContributeConfig Contribute { get; init; } = new();

        /// <summary>Sanitize all configuration</summary>
        /// <param name="repair">Allow repairs</param>
        /// <param name="debug">Output debug messages to console</param>
        /// <returns>Sanitized status</returns>
        public bool Sanitize(bool repair = true, bool debug = false)
        {
            var ret = new List<bool>
            {
                this.HuntConfig.Sanitize(repair, debug),
                this.FateConfig.Sanitize(repair, debug)
            };
            return ret.TrueForAll(r => r);
        }

        /// <summary>Get a specific Relay Tracker Configuration</summary>
        public RelayConfig? GetRelayTrackerConfig(RelayType type)
        {
            if (type == RelayType.Hunt) return this.HuntConfig;
            if (type == RelayType.Fate) return this.FateConfig;
            return null;
        }

        /// <summary>Get a specific Relay Tracker Configuration</summary>
        public RelayConfig? GetRelayTrackerConfig(Type type) => this.GetRelayTrackerConfig(RelayUtils.GetRelayType(type));

        /// <summary>Get a specific Relay Tracker Configuration</summary>
        public RelayConfig? GetRelayTrackerConfig<T>() where T : Relay => this.GetRelayTrackerConfig(typeof(T));

        /// <summary>Reads another configuration into this configuration.</summary>
        /// <remarks><see cref="VersionUpdate"/> is run on <paramref name="config"/> before reading.</remarks>
        public void ReadFrom(SonarConfig config)
        {
            config.VersionUpdate();
            this.Version = config.Version;
            this.LogLevel = config.LogLevel;
            this.HuntConfig.ReadFrom(config.HuntConfig);
            this.FateConfig.ReadFrom(config.FateConfig);
            this.Contribute.ReadFrom(config.Contribute);
        }

        /// <summary>Update configuration version.</summary>
        /// <exception cref="InvalidOperationException">Accessor failure.</exception>
        public void VersionUpdate()
        {
            // I cannot bypass an [Obsolete] as error, but that doesn't stop me from using reflection based property accesors
            //var receiveAccessor = typeof(SonarConfig).GetProperty("ReceiveJurisdiction", BindingFlags.Instance | BindingFlags.Public)
            //    ?? throw new InvalidOperationException("Unexpected error while getting contribute accessor");
            //var contributeAccessor = typeof(RelayConfig).GetProperty("Contribute", BindingFlags.Instance | BindingFlags.Public)
            //    ?? throw new InvalidOperationException("Unexpected error while getting contribute accessor");

            if (this.Version <= 0)
            {
                // Initial default was intended to be Data Center, however I accidentally left it at Audience during first release of this setting.
                // This mistaken default is only changed once as part of a configuration version update. Users can set it back to Audience as they wish.
                //if ((SonarJurisdiction)receiveAccessor.GetValue(this)! == SonarJurisdiction.Audience) receiveAccessor.SetValue(this, SonarJurisdiction.Datacenter);

                // Update configuration version
                this.Version = 1;
            }

            if (this.Version == 1)
            {
                // Contribute settings got moved into SonarConfig.Server. This moves the config previously saved at
                // each respective hunt and fates config into their new location.
                //this.Contribute[RelayType.Hunt] = (bool)contributeAccessor.GetValue(this.HuntConfig)!;
                //this.Contribute[RelayType.Fate] = (bool)contributeAccessor.GetValue(this.FateConfig)!;

                // Global contribute default to true. Not needed since that's the default but placed here for verbosity
                this.Contribute.Global = true;

                // Receive jurisdiction got moved into SonarConfig.Server as well. This moves the config previously
                // saved here into the server configuration.
                //this.Contribute.ReceiveJurisdiction = (SonarJurisdiction)receiveAccessor.GetValue(this)!;

                // Update configuration version
                this.Version = 2;
            }

            if (this.Version > 2) // Welcome to the future, now I send you to the past
            {
                // We don't know from how far in the future this user came from. We have no way of knowing as we really
                // cannot see nor predict the future and therefore we don't know how much the configuration must have
                // changed by then. We therefore send this user to the beginning of Sonar's time and rerun VersionUpdate()
                // recursively, effectively placing them on the right timeline. (Why am I even commenting this?)
                this.Version = 0;
                this.VersionUpdate();
            }
        }
    }
}
