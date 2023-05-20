using MessagePack;
using Newtonsoft.Json;
using Sonar.Enums;
using Sonar.Logging;
using Sonar.Messages;
using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sonar.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject(true)] // WARNING: Obscure bug if (true) is removed because of previous releases having it from SonarConfigMessage (meaning all Key attributes are ignored and names are simply the property name as is).
    public sealed class SonarConfig : ISonarMessage
    {
        public SonarConfig() { }
        public SonarConfig(SonarConfig c)
        {
            this.LogLevel = c.LogLevel;
            this.HuntConfig = c.HuntConfig;
            this.FateConfig = c.FateConfig;
        }

        // TODO: Future implementation (to avoid the unusual API of reassigning to self)
        private SonarClient? Client { get; set; }
        internal void SendToServer() => this.Client?.Connection.SendIfConnected(this);
        internal void BindClient(SonarClient? client) => this.Client = client;

        [JsonProperty]
        [Key("version")]
        public int Version { get; set; }

        /// <summary>
        /// Server side log level
        /// </summary>
        [JsonProperty]
        [Key("logLevel")]
#if DEBUG
        public SonarLogLevel LogLevel { get; set; } = SonarLogLevel.Information; //Info
#else
        public SonarLogLevel LogLevel { get; set; } = SonarLogLevel.Warning; //Warn
#endif

        private HuntConfig huntConfig = new();
        /// <summary>
        /// Hunt reporting configuration
        /// </summary>
        [JsonProperty]
        [Key("huntConfig")]
        public HuntConfig HuntConfig
        {
            get => this.huntConfig;
            set => this.huntConfig = new HuntConfig(value);
        }

        private FateConfig fateConfig = new();
        /// <summary>
        /// Faate reporting configuration
        /// </summary>
        [JsonProperty]
        [Key("fateConfig")]
        public FateConfig FateConfig
        {
            get => this.fateConfig;
            set => this.fateConfig = new FateConfig(value);
        }

        /// <summary>
        /// Jurisdiction to receive from Server
        /// </summary>
        [JsonProperty]
        [Key("receiveJurisdiction")]
        public SonarJurisdiction ReceiveJurisdiction { get; set; } = SonarJurisdiction.Datacenter;

        /// <summary>
        /// Sanitize all configuration
        /// </summary>
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
            return ret.All(r => r);
        }

        /// <summary>
        /// Get a specific Relay Tracker Configuration
        /// </summary>
        public RelayConfig? GetRelayTrackerConfig<T>() where T : Relay
        {
            var type = typeof(T);
            if (type == typeof(HuntRelay)) return this.HuntConfig;
            if (type == typeof(FateRelay)) return this.FateConfig;
            return null;
        }

        internal void VersionUpdate()
        {
            if (this.Version == 0)
            {
                if (this.ReceiveJurisdiction == SonarJurisdiction.Audience) this.ReceiveJurisdiction = SonarJurisdiction.Datacenter;
                this.Version = 1;
            }

            if (this.Version > 1) // Welcome to the future, now I send you to the past
            {
                this.Version = 0;
                this.VersionUpdate();
            }
        }
    }
}
