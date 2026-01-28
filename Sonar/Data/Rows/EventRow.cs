using MessagePack;
using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sonar.Data.Rows
{
    [MessagePackObject]
    public sealed class EventRow : IRelayDataRow
    {
        private IReadOnlyCollection<uint>? _zoneIds;
        
        [Key(0)]
        public uint Id { get; set; }

        [Key(1)]
        public int Level { get; set; }

        [Key(2)]
        public uint ZoneId { get; set; }

        [Key(3)]
        public LanguageStrings Name { get; set; } = new();

        [Key(4)]
        public LanguageStrings Description { get; set; } = new();

        [Key(5)]
        public HuntRank Rank { get; set; } = HuntRank.Event;

        [Key(6)]
        public ExpansionPack Expansion { get; set; }

        [Key(7)]
        public uint GroupId { get; set; }

        [Key(8)]
        public bool GroupMain { get; set; }


        IReadOnlyCollection<uint> IRelayDataRow.ZoneIds => this._zoneIds ??= [this.ZoneId];

        public override string ToString() => this.Name.ToString();
    }
}
