using MessagePack;
using Newtonsoft.Json;
using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    public abstract class RelayConfig
    {
        private bool _contribute = true;

        protected RelayConfig() { }
        protected RelayConfig(RelayConfig c)
        {
            if (c == null) return;
            this._contribute = c._contribute;
            this.TrackAll = c.TrackAll;
        }

        /// <summary>
        /// Get Report Jurisdiction Implementation
        /// </summary>
        protected virtual SonarJurisdiction GetReportJurisdictionImpl(uint id) => SonarJurisdiction.None;

        /// <summary>
        /// Contribute reports
        /// </summary>
        [JsonProperty]
        [Key("contribute")]
        [Obsolete("Use RelayConfig.Contribute instead", true)]
        public bool Contribute { get => this._contribute; set => this._contribute = value; }

        /// <summary>
        /// True: Track all hunts including those outside of the configured jurisdictions
        /// False: Track hunts within configured jurisdictions only
        /// </summary>
        [JsonProperty]
        [Key("trackAll")]
        public bool TrackAll { get; set; } = true;

        /// <summary>
        /// Main jurisdiction check function
        /// </summary>
        public SonarJurisdiction GetReportJurisdiction(uint id)
        {
            return this.GetReportJurisdictionImpl(id);
        }
    }
}
