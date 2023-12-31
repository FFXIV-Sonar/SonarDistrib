using MessagePack;
using Sonar.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data.Rows
{
    [MessagePackObject]
    public sealed class AetheryteRow : IDataRow
    {
        [Key(0)]
        public uint Id { get; set; }

        [Key(1)]
        public LanguageStrings Name { get; set; } = new();

        [Key(2)]
        public uint ZoneId { get; set; }

        [Key(3)]
        public SonarVector3 Coords { get; set; }

        [Key(4)]
        public float DistanceCostModifier { get; set; }

        [Key(5)]
        public bool Teleportable { get; set; }

        [Key(6)]
        public int AethernetGroup { get; set; }

        string IDataRow.Name => this.Name.ToString();

        public override string ToString() => this.Name.ToString();
    }
}
