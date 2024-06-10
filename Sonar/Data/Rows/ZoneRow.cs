using Sonar.Enums;
using Sonar.Numerics;
using System;
using System.Collections.Generic;
using MessagePack;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Zone (TerritoryType) data row
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public sealed class ZoneRow : IDataRow
    {
        [Key(0)]
        public uint Id { get; set; }
        [Key(1)]
        public LanguageStrings Region { get; set; } = new();
        [Key(2)]
        public LanguageStrings Name { get; set; } = new();

        // Properties for Flags (used by MapFlagUtils)
        [Key(3)]
        public float Scale { get; set; }
        [Key(4)]
        public SonarVector3 Offset { get; set; }
        [Key(5)]
        public bool HasOffsetZ { get; set; }

        // Property for ingame resources
        [Key(6)]
        public string? MapResourcePath { get; set; }

        [Key(7)]
        public ExpansionPack Expansion { get; set; }

        [Key(8)]
        public uint MapId { get; set; }

        [Key(9)]
        public bool IsField { get; set; }

        [Key(10)]
        public bool LocalOnly { get; set; }

        [Key(11)]
        public HashSet<uint> HuntIds { get; set; } = new();

        [Key(12)]
        public HashSet<uint> FateIds { get; set; } = new();

        public override string ToString() => this.Name.ToString();
        public string ToString(SonarLanguage lang) => this.Name.ToString(lang);
    }
}
