using Sonar.Enums;
using Sonar.Numerics;
using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Sonar.Data.Extensions;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Fate data row
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public sealed class FateRow : IRelayDataRow
    {
        private IReadOnlyCollection<uint>? _zoneIds;

        [Key(0)]
        public uint Id { get; set; }
        [Key(1)]
        public int Level { get; set; }
        [Key(2)]
        public uint IconId { get; set; }
        [Key(3)]
        public uint ObjectiveIconId { get; set; }
        [Key(4)]
        public LanguageStrings Name { get; set; } = new();
        [Key(5)]
        public LanguageStrings Description { get; set; } = new();
        [Key(6)]
        public LanguageStrings Objective { get; set; } = new();
        [Key(7)]
        public SonarVector3 Coordinates { get; set; }
        [Key(8)]
        public SonarVector3 Scale { get; set; }
        [Key(9)]
        public ExpansionPack Expansion { get; set; }
        [Key(10)]
        public bool IsHidden { get; set; }
        [Key(11)]
        public bool HasAchievement { get; set; }
        [Key(12)]
        public LanguageStrings AchievementName { get; set; } = new();
        [Key(13)]
        public uint ZoneId { get; set; }
        [Key(14)]
        public HashSet<uint> GroupFateIds { get; set; } = default!;
        [Key(15)]
        public bool GroupMain { get; set; }
        [Key(16)]
        public uint GroupId { get; set; }
        [Key(17)]
        public uint LgbId { get; set; }

        HuntRank IRelayDataRow.Rank => HuntRank.Fate;
        IReadOnlyCollection<uint> IRelayDataRow.ZoneIds => this._zoneIds ??= [this.ZoneId];

        public override string ToString() => this.Name.ToString();
        public string ToString(SonarLanguage lang) => this.Name.ToString(lang);
    }
}
