using Sonar.Enums;
using System;
using System.Collections.Generic;
using MessagePack;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Hunt data row
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public sealed class HuntRow : IDataRow
    {
        [Key(0)]
        public uint Id { get; set; }
        [Key(1)]
        public LanguageStrings Name { get; set; } = new LanguageStrings();
        [Key(2)]
        public HuntRank Rank { get; set; }
        [Key(3)]
        public ExpansionPack Expansion { get; set; }
        [Key(4)]
        public SpawnTimers SpawnTimers { get; set; }
        [Key(5)]
        public HashSet<uint> SpawnZoneIds { get; set; } = new();

        [IgnoreMember]
        string IDataRow.Name => this.Name.ToString();

        public override string ToString() => this.Name.ToString();
        public string ToString(SonarLanguage lang) => this.Name.ToString(lang);
    }
}
