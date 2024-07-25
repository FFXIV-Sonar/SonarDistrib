using MessagePack;
using Newtonsoft.Json;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using SonarUtils;

namespace Sonar.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    public class FateConfig : RelayConfig
    {
        private const SonarJurisdiction _DefaultJurisdiction = SonarJurisdiction.None;

        /// <summary>Default Jurisdiction</summary>
        [JsonProperty]
        [Key("defaultJurisdiction")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public SonarJurisdiction DefaultJurisdiction { get; set; } = _DefaultJurisdiction;

        /// <summary>Jurisdictions</summary>
        [JsonProperty]
        [Key("jurisdiction")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dictionary<uint, SonarJurisdiction> Jurisdiction { get; init; } = [];

        /// <summary>Get report jurisdiction for a specific expansion and rank</summary>
        /// <param name="id">Fate ID</param>
        /// <returns>Report jurisdiction</returns>
        public SonarJurisdiction GetJurisdiction(uint id)
        {
            if (!this.Jurisdiction.TryGetValue(id, out var jurisdiction)) jurisdiction = DefaultJurisdiction;
            if (jurisdiction is SonarJurisdiction.Default) return _DefaultJurisdiction;
            return jurisdiction;
        }

        /// <summary>Get all non-default jurisdictions</summary>
        /// <returns>Jurisdictions</returns>
        public Dictionary<uint, SonarJurisdiction> GetJurisdictions() => this.Jurisdiction.ToDictionary(kv => kv.Key, kv => kv.Value);

        /// <summary>Set all jurisdictions in the provided dictionary</summary>
        /// <returns>Jurisdictions</returns>
        public void SetJurisdictions(Dictionary<uint, SonarJurisdiction> jurisdictions)
        {
            foreach (var (key, value) in jurisdictions) this.SetJurisdiction(key, value);
        }

        /// <summary>Set report jurisdiction for a specific fate</summary>
        /// <param name="id">Fate ID</param>
        /// <param name="jurisdiction">Jurisdiction</param>
        public void SetJurisdiction(uint id, SonarJurisdiction jurisdiction)
        {
            if (!Database.Fates.ContainsKey(id)) return;
            if (jurisdiction == SonarJurisdiction.Default) this.RemoveJurisdiction(id);
            else this.Jurisdiction[id] = jurisdiction;
        }

        public SonarJurisdiction GetDefaultJurisdiction()
        {
            return this.DefaultJurisdiction;
        }

        public void SetDefaultJurisdiction(SonarJurisdiction jurisdiction)
        {
            if (jurisdiction == SonarJurisdiction.Default) jurisdiction = _DefaultJurisdiction;
            this.DefaultJurisdiction = jurisdiction;
        }

        /// <summary>Remove a report jurisdiction for a specific ID</summary>
        /// <param name="id">Fate ID</param>
        public bool RemoveJurisdiction(uint id)
        {
            return this.Jurisdiction.Remove(id);
        }

        /// <summary>Remove all report jurisdiction</summary>
        public void ResetJurisdictions()
        {
            this.Jurisdiction.Clear();
        }

        /// <summary>Main jurisdiction check function</summary>
        protected override SonarJurisdiction GetReportJurisdictionImpl(uint id)
        {
            var jurisdiction = this.GetJurisdiction(id);
            if (jurisdiction == SonarJurisdiction.Default) jurisdiction = this.DefaultJurisdiction;
            if (jurisdiction == SonarJurisdiction.Default) jurisdiction = _DefaultJurisdiction;
            return jurisdiction;
        }

        public override void ReadFrom(RelayConfig config)
        {
            if (config is not FateConfig fateConfig) throw new ArgumentException($"{nameof(config)} must be of type {nameof(FateConfig)}", nameof(config));
            this.ReadFrom(fateConfig);
            base.ReadFrom(fateConfig);
        }

        public void ReadFrom(FateConfig config)
        {
            this.DefaultJurisdiction = config.DefaultJurisdiction;
            this.Jurisdiction.Clear();
            this.Jurisdiction.AddRange(config.Jurisdiction);
            for (var attempt = 0; attempt < 3 && !this.Sanitize(); attempt++) { /* Empty */ }
        }

        /// <summary>Sanitize configuration</summary>
        /// <param name="repair">Allow repairs</param>
        /// <param name="debug">Output debug messages to console</param>
        /// <returns>Sanitized status</returns>
        public bool Sanitize(bool repair = true, bool debug = false)
        {
            var isOkay = true;

            var jurisdictions = Enum.GetValues<SonarJurisdiction>().ToHashSet();
            var fates = Database.Fates;

            if (debug) Console.WriteLine("FateConfig IDs and Jurisdictions (1 of 1)");
            foreach (var (fateId, jurisdiction) in this.Jurisdiction.ToList()) // .ToList to avoid modifying the dictionary during enumeration
            {
                if (!fates.ContainsKey(fateId))
                {
                    if (debug) Console.WriteLine($"Invalid Fate ID detected");
                    isOkay = false;
                    if (repair) this.Jurisdiction.Remove(fateId);
                    continue;
                }

                if (!jurisdictions.Contains(jurisdiction))
                {
                    if (debug) Console.WriteLine($"Invalid jurisdiction fate detected");
                    isOkay = false;
                    if (repair) this.Jurisdiction.Remove(fateId);
                    continue;
                }
            }

            return isOkay;
        }
    }
}
