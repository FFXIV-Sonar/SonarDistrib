using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data.Rows
{
    [MessagePackObject]
    public sealed class AudienceRow : INamedRow
    {
        [Key(0)]
        public uint Id { get; set; }
        [Key(1)]
        public string Name { get; set; } = default!;
        [Key(2)]
        public bool IsPublic { get; set; }
    }
}
