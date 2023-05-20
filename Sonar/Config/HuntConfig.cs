using MessagePack;
using Sonar.Data;
using Newtonsoft.Json;
using Sonar.Data.Rows;
using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Sonar.Config
{
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    public sealed class HuntConfig : RelayConfig
    {
        #region HuntConfig Logic
        public HuntConfig()
        {
            this.Jurisdiction = new Dictionary<ExpansionPack, Dictionary<HuntRank, SonarJurisdiction>>();
            foreach (ExpansionPack expansion in Enum.GetValues(typeof(ExpansionPack)))
            {
                this.Jurisdiction[expansion] = new Dictionary<HuntRank, SonarJurisdiction>();
                foreach (HuntRank rank in Enum.GetValues(typeof(HuntRank)))
                {
                    this.SetJurisdiction(expansion, rank, SonarJurisdiction.Default);
                }
            }
        }

        public HuntConfig(HuntConfig c) : base(c)
        {
            this.Jurisdiction = c.Jurisdiction;
            this.JurisdictionOverride = c.JurisdictionOverride;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API")]
        private static SonarJurisdiction GetDefaultJurisdiction(ExpansionPack expansion, HuntRank rank)
        {
            return rank switch
            {
                HuntRank.B => SonarJurisdiction.None,
                HuntRank.A => SonarJurisdiction.World,
                _ => SonarJurisdiction.Datacenter
            };
        }

        /// <summary>
        /// Jurisdictions
        /// </summary>
        [JsonProperty]
        [Key("jurisdiction")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dictionary<ExpansionPack, Dictionary<HuntRank, SonarJurisdiction>> Jurisdiction { get; set; }

        /// <summary>
        /// Jurisdiction Overrides
        /// </summary>
        [JsonProperty]
        [Key("jurisdictionOverride")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Dictionary<uint, SonarJurisdiction> JurisdictionOverride { get; set; } = new Dictionary<uint, SonarJurisdiction>();
        #endregion

        /// <summary>
        /// Get report jurisdiction for a specific expansion and rank
        /// </summary>
        /// <param name="expansion"></param>
        /// <param name="rank"></param>
        /// <returns>Report jurisdiction</returns>
        public SonarJurisdiction GetJurisdiction(ExpansionPack expansion, HuntRank rank)
        {
            return this.Jurisdiction[expansion][rank];
        }

        /// <summary>
        /// Set report jurisdiction for a specific expansion and rank
        /// </summary>
        /// <param name="expansion">Expansion Pack</param>
        /// <param name="rank">Hunt Rank</param>
        /// <param name="jurisdiction">Jurisdiction to receive reports from</param>
        public void SetJurisdiction(ExpansionPack expansion, HuntRank rank, SonarJurisdiction jurisdiction)
        {
            this.Jurisdiction[expansion][rank] = (jurisdiction != SonarJurisdiction.Default)? jurisdiction : GetDefaultJurisdiction(expansion, rank);
        }

        /// <summary>
        /// Get jurisdiction override for a specific ID
        /// </summary>
        /// <param name="id">ID to set the report jurisdiction for</param>
        public SonarJurisdiction GetJurisdictionOverride(uint id)
        {
            return this.JurisdictionOverride.GetValueOrDefault(id, SonarJurisdiction.Default);
        }

        /// <summary>
        /// Override report jurisdiction for a specific ID
        /// </summary>
        /// <param name="id">ID to set the report jurisdiction for</param>
        /// <param name="jurisdiction">Jurisdiction to receive reports from</param>
        public void SetJurisdictionOverride(uint id, SonarJurisdiction jurisdiction)
        {
            if (!Database.Hunts.ContainsKey(id)) throw new KeyNotFoundException($"Invalid ID ({id})");
            if (jurisdiction == SonarJurisdiction.Default) this.RemoveJurisdictionOverride(id);
            else this.JurisdictionOverride[id] = jurisdiction;
        }

        /// <summary>
        /// Remove a report jurisdiction override for a specific ID
        /// </summary>
        public bool RemoveJurisdictionOverride(uint id)
        {
            return this.JurisdictionOverride.Remove(id);
        }

        /// <summary>
        /// Remove all jurisdiction overrides
        /// </summary>
        public void RemoveAllJurisdictionOverrides()
        {
            this.JurisdictionOverride.Clear();
        }

        /// <summary>
        /// Get all jurisdiction overrides
        /// </summary>
        /// <returns></returns>
        public IDictionary<uint, SonarJurisdiction> GetJurisdictionOverrides()
        {
            // Force a copy
            return this.JurisdictionOverride.ToDictionary(o => o.Key, o => o.Value);
        }

        /// <summary>
        /// Main jurisdiction check function
        /// </summary>
        protected override SonarJurisdiction GetReportJurisdictionImpl(uint id)
        {
            var jurisdiction = this.GetJurisdictionOverride(id);
            if (jurisdiction == SonarJurisdiction.Default)
            {
                var hunt = Database.Hunts[id];
                jurisdiction = this.GetJurisdiction(hunt.Expansion, hunt.Rank);
                if (jurisdiction == SonarJurisdiction.Default)
                {
                    jurisdiction = GetDefaultJurisdiction(hunt.Expansion, hunt.Rank);
                }
            }
            return jurisdiction;
        }

        /// <summary>
        /// Sanitize configuration
        /// </summary>
        /// <param name="repair">Allow repairs</param>
        /// <param name="debug">Output debug messages to console</param>
        /// <returns>Sanitized status</returns>
        public bool Sanitize(bool repair = true, bool debug = false)
        {
            var isOkay = true;

            var expansions = Enum.GetValues<ExpansionPack>();
            var ranks = Enum.GetValues<HuntRank>();
            var jurisdictions = Enum.GetValues<SonarJurisdiction>();
            var hunts = Database.Hunts;

            if (debug) Console.WriteLine("HuntConfig Jurisdictions (1 of 2)");
            foreach (var expansion in this.Jurisdiction.Keys)
            {
                if (!expansions.Contains(expansion))
                {
                    if (debug) Console.WriteLine($"Invalid jurisdiction expansion detected");
                    isOkay = false;
                    if (repair) this.Jurisdiction.Remove(expansion);
                    continue;
                }
                foreach (var rank in this.Jurisdiction[expansion].Keys)
                {
                    if (!ranks.Contains(rank))
                    {
                        if (debug) Console.WriteLine($"Invalid jurisdiction rank detected");
                        isOkay = false;
                        if (repair) this.Jurisdiction[expansion].Remove(rank);
                        continue;
                    }
                    if (!jurisdictions.Contains(this.Jurisdiction[expansion][rank]))
                    {
                        if (debug) Console.WriteLine($"Invalid jurisdiction detected");
                        isOkay = false;
                        if (repair) GetDefaultJurisdiction(expansion, rank);
                        continue;
                    }
                }
            }

            if (debug) Console.WriteLine("HuntConfig Jurisdictions (2 of 2)");
            foreach (var expansion in expansions)
            {
                if (!this.Jurisdiction.ContainsKey(expansion))
                {
                    if (debug) Console.WriteLine($"Missing jurisdiction expansion detected");
                    isOkay = false;
                    if (repair) this.Jurisdiction[expansion] = new Dictionary<HuntRank, SonarJurisdiction>();
                }
                foreach (HuntRank rank in ranks)
                {
                    if (!this.Jurisdiction[expansion].ContainsKey(rank) || !jurisdictions.Contains(this.Jurisdiction[expansion][rank]))
                    {
                        if (debug) Console.WriteLine($"Missing or invalid jurisdiction rank detected");
                        isOkay = false;
                        if (repair) GetDefaultJurisdiction(expansion, rank);
                    }
                }
            }

            if (debug) Console.WriteLine("HuntConfig Jurisdictions Overrides");
            foreach (var id in this.JurisdictionOverride.Keys.ToList())
            {
                if (!hunts.ContainsKey(id))
                {
                    if (debug) Console.WriteLine($"Invalid id detected for a jurisdiction override");
                    isOkay = false;
                    if (repair) this.JurisdictionOverride.Remove(id);
                    continue;
                }
                if (jurisdictions.Contains(this.JurisdictionOverride[id]) || this.JurisdictionOverride[id] == SonarJurisdiction.Default)
                {
                    if (debug)
                    {
                        if (this.JurisdictionOverride[id] == SonarJurisdiction.Default)
                            Console.WriteLine($"Jurisdiction override for {id} should not be default");
                        else 
                            Console.WriteLine($"Invalid jurisdiction override detected for {id}: {this.JurisdictionOverride[id]}");
                    }
                    isOkay = false;
                    if (repair) this.JurisdictionOverride.Remove(id);
                    continue;
                }
            }

            return isOkay;
        }
    }
}
