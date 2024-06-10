using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data.Rows
{
    public interface IRelayDataRow : ILanguageNamedRow
    {
        public int Level { get; }
        public ExpansionPack Expansion { get; }
        public HuntRank Rank { get; }

        public uint GroupId { get; }
        public bool GroupMain { get; }

        public IReadOnlyCollection<uint> ZoneIds { get; }
    }
}
