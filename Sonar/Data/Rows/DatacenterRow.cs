using System;
using MessagePack;
using Sonar.Enums;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Data Center data row
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public sealed class DatacenterRow : IDataRow
    {
        [Key(0)]
        public uint Id { get; set; }
        [Key(1)]
        public string Name { get; set; } = default!;
        [Key(2)]
        public uint RegionId { get; set; }
        [Key(3)]
        public uint AudienceId { get; set; }
        [Key(4)]
        public bool HasLodestone { get; set; }
        [Key(5)]
        public bool IsPublic { get; set; }

        public override string ToString() => this.Name.ToString();
    }
}
