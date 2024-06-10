using System;
using Sonar.Numerics;
using MessagePack;
using Sonar.Enums;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Zone (TerritoryType) data row
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public sealed class MapRow : IDataRow
    {
        [Key(0)]
        public uint Id { get; set; }

        [Key(1)]
        public LanguageStrings Region { get; set; } = new LanguageStrings();
        [Key(2)]
        public LanguageStrings Name { get; set; } = new LanguageStrings();
        [Key(3)]
        public LanguageStrings SubName { get; set; } = new LanguageStrings();

        [Key(4)]
        public float Scale { get; set; }
        [Key(5)]
        public SonarVector3 Offset { get; set; }
        [Key(6)]
        public bool HasOffsetZ { get; set; }

        // Property for ingame resources
        [Key(7)]
        public string MapResourcePath { get; set; } = default!;

        [Key(8)]
        public uint ZoneId { get; set; }

        public override string ToString() => this.Name.ToString();
        public string ToString(SonarLanguage lang) => this.Name.ToString(lang);
    }
}
