using Sonar.Enums;

namespace Sonar.Config
{
    public abstract class RelayConfig
    {
        /// <summary>Get Report Jurisdiction Implementation</summary>
        protected virtual SonarJurisdiction GetReportJurisdictionImpl(uint id) => SonarJurisdiction.None;

        /// <summary>
        /// <list type="bullet">
        /// <item><see langword="true"/>: Track all hunts including those outside of the configured jurisdictions</item>
        /// <item><see langword="false"/>: Track hunts within configured jurisdictions only</item>
        /// </list>
        /// </summary>
        public bool TrackAll { get; set; } = true;

        /// <summary>Main jurisdiction check function</summary>
        public SonarJurisdiction GetReportJurisdiction(uint id)
        {
            return this.GetReportJurisdictionImpl(id);
        }

        /// <summary>Reads another configuration into this configuration.</summary>
        public virtual void ReadFrom(RelayConfig config)
        {
            this.TrackAll = config.TrackAll;
        }
    }
}
