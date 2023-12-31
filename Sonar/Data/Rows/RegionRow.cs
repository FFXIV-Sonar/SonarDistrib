using MessagePack;

namespace Sonar.Data.Rows
{
    [MessagePackObject]
    public sealed class RegionRow : IDataRow
    {
        [Key(0)]
        public uint Id { get; set; }
        [Key(1)]
        public string Name { get; set; } = default!;
        [Key(2)]
        public uint AudienceId { get; set; }
        [Key(3)]
        public bool IsPublic { get; set; }
    }
}
