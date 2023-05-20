using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Relays;
using Sonar.Trackers;

namespace Sonar.Config.Experimental
{
    using static HuntConfigStatics;

    internal static class HuntConfigStatics
    {
        public static readonly IEnumerable<HuntRank> Ranks = Enum.GetValues<HuntRank>();
        public static readonly IEnumerable<ExpansionPack> Expansions = Enum.GetValues<ExpansionPack>();
    }

    /// <summary>
    /// Container allowing you to hold arbitrary hunt configuration
    /// </summary>
    public sealed class HuntConfig<T> : IEnumerable<T>
    {
        /// <summary>
        /// Default Configuration returned in place of missing configuration
        /// </summary>
        public T? DefaultConfig { get; set; }

        /// <summary>
        /// Rank Expansion configurations
        /// </summary>
        public IDictionary<RankExpansion, T> RankExpansionConfigs { get; set; } = new Dictionary<RankExpansion, T>();

        /// <summary>
        /// Configuration override by hunt ID
        /// </summary>
        public IDictionary<uint, T> OverrideConfigs { get; set; } = new Dictionary<uint, T>();

        /// <summary>
        /// Resolves hunt configuration for a specific hunt id
        /// </summary>
        public T? GetHuntConfig(uint huntId)
        {
            if (this.OverrideConfigs.TryGetValue(huntId, out var value)) return value;
            if (Database.Hunts.TryGetValue(huntId, out var info) && this.RankExpansionConfigs.TryGetValue(new(info.Rank, info.Expansion), out value)) return value;
            return this.DefaultConfig;
        }

        /// <summary>
        /// Resolves hunt configuration for a specific hunt relay
        /// </summary>
        public T? GetHuntConfig(HuntRelay hunt) => this.GetHuntConfig(hunt.Id);

        /// <summary>
        /// Resolves hunt configuration for a specific hunt relay state
        /// </summary>
        public T? GetHuntConfig(RelayState<HuntRelay> hunt) => this.GetHuntConfig(hunt.Relay);

        /// <summary>
        /// Resolves hunt configuration for a specific hunt information
        /// </summary>
        public T? GetHuntConfig(HuntRow info) => this.GetHuntConfig(info.Id);

        /// <summary>
        /// Resolves hunt configuration for a specific rank expansion
        /// </summary>
        public T? GetRankExpansionConfig(RankExpansion rankExpansion)
        {
            return this.RankExpansionConfigs.TryGetValue(rankExpansion, out var value) ? value : this.DefaultConfig;
        }

        /// <summary>
        /// Resolves hunt configuration for a specific rank expansion
        /// </summary>
        public T? GetRankExpansionConfig(HuntRow info) => this.GetRankExpansionConfig(new RankExpansion(info.Rank, info.Expansion));

        /// <summary>
        /// Resolves hunt configuration for a specific rank expansion derived from a hunt id
        /// </summary>
        public T? GetRankExpansionConfig(uint id)
        {
            return Database.Hunts.TryGetValue(id, out var info) ? this.GetRankExpansionConfig(info) : this.DefaultConfig;
        }

        /// <summary>
        /// Resolves hunt configuration for a specific rank expansion derived from a hunt relay
        /// </summary>
        public T? GetRankExpansionConfig(HuntRelay relay) => this.GetRankExpansionConfig(relay.Id);

        /// <summary>
        /// Resolves hunt configuration for a specific rank expansion derived from a hunt relay state
        /// </summary>
        public T? GetRankExpansionConfig(RelayState<HuntRelay> state) => this.GetRankExpansionConfig(state.Relay);

        /// <summary>
        /// Set rank config for all expansions
        /// </summary>
        public void SetRankConfig(HuntRank rank, T config)
        {
            foreach (var expansion in Expansions) this.RankExpansionConfigs[new(rank, expansion)] = config;
        }

        /// <summary>
        /// Set expansion config for all ranks
        /// </summary>
        public void SetExpansionConfig(ExpansionPack expansion, T config)
        {
            foreach (var rank in Ranks) this.RankExpansionConfigs[new(rank, expansion)] = config;
        }

        /// <summary>
        /// Remove rank config for all expansions
        /// </summary>
        public void RemoveRankConfig(HuntRank rank)
        {
            foreach (var expansion in Expansions) this.RankExpansionConfigs.Remove(new(rank, expansion));
        }

        /// <summary>
        /// Remove rank config for all expansions
        /// </summary>
        public void RemoveExpansionConfig(ExpansionPack expansion)
        {
            foreach (var rank in Ranks) this.RankExpansionConfigs.Remove(new(rank, expansion));
        }

        /// <summary>
        /// Gets an enumerator that cover all configs (Overrides => Rank Expansions => Default)
        /// </summary>
        /// <remarks>Used by IEnumerable implementation</remarks>
        public IEnumerable<T> GetConfigsEnumerable()
        {
            foreach (var config in this.OverrideConfigs.Values) yield return config;
            foreach (var config in this.RankExpansionConfigs.Values) yield return config;
            if (this.DefaultConfig is not null) yield return this.DefaultConfig;
        }

        /// <summary>
        /// Remove all configs that matches a specific predicate (Overrides => Rank Expansions => Default)
        /// </summary>
        public void RemoveConfigsWhere(Func<T, bool> predicate)
        {
            foreach (var (key, config) in this.OverrideConfigs)
            {
                if (predicate(config)) this.OverrideConfigs.Remove(key);
            }
            foreach (var (key, config) in this.RankExpansionConfigs)
            {
                if (predicate(config)) this.RankExpansionConfigs.Remove(key);
            }
            if (this.DefaultConfig is not null && predicate(this.DefaultConfig)) this.DefaultConfig = default;
        }

        public int Count => this.OverrideConfigs.Count + this.RankExpansionConfigs.Count + (this.DefaultConfig is not null ? 1 : 0);
        public IEnumerator<T> GetEnumerator() => this.GetConfigsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public readonly struct RankExpansion : IEquatable<RankExpansion>
    {
        public RankExpansion(HuntRank rank, ExpansionPack expansion)
        {
            this.Rank = rank;
            this.Expansion = expansion;
        }

        public HuntRank Rank { get; init; }
        public ExpansionPack Expansion { get; init; }

        public bool Equals(RankExpansion other) => this.Rank == other.Rank && this.Expansion == other.Expansion;
        public override bool Equals(object? obj) => obj is RankExpansion other && this.Equals(other);
        public override int GetHashCode() => this.Rank.GetHashCode() ^ (this.Expansion.GetHashCode() << 16);

        public static bool operator ==(RankExpansion first, RankExpansion second) => first.Equals(second);
        public static bool operator !=(RankExpansion first, RankExpansion second) => !first.Equals(second);

        public static bool operator ==(RankExpansion first, HuntRank second) => first.Rank == second;
        public static bool operator !=(RankExpansion first, HuntRank second) => first.Rank != second;

        public static bool operator ==(RankExpansion first, ExpansionPack second) => first.Expansion == second;
        public static bool operator !=(RankExpansion first, ExpansionPack second) => first.Expansion != second;

        public static bool operator ==(HuntRank first, RankExpansion second) => first == second.Rank;
        public static bool operator !=(HuntRank first, RankExpansion second) => first != second.Rank;

        public static bool operator ==(ExpansionPack first, RankExpansion second) => first == second.Expansion;
        public static bool operator !=(ExpansionPack first, RankExpansion second) => first != second.Expansion;

        public static bool operator ==(RankExpansion first, HuntRow second) => first.Equals(second);
        public static bool operator !=(RankExpansion first, HuntRow second) => !first.Equals(second);

        public static bool operator ==(HuntRow first, RankExpansion second) => second == first;
        public static bool operator !=(HuntRow first, RankExpansion second) => second != first;

        public static implicit operator RankExpansion(HuntRow info) => new(info.Rank, info.Expansion);
    }

}
