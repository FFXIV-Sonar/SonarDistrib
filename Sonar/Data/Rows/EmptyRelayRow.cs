using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data.Rows
{
    public sealed class EmptyRelayRow : IRelayDataRow
    {
        public static readonly EmptyRelayRow Instance = new EmptyRelayRow();

        private EmptyRelayRow() { }
        public int Level { get; } =  0;
        public ExpansionPack Expansion { get; } = ExpansionPack.Unknown;
        public HuntRank Rank { get; } = HuntRank.None;
        public uint GroupId { get; } = 0;
        public bool GroupMain { get; } = true;
        public IReadOnlyCollection<uint> ZoneIds { get; } = [];
        public LanguageStrings Name => new() { { SonarLanguage.English, string.Empty } };
        public uint Id { get; } = 0;
    }
}
