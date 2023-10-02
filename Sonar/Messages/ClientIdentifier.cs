using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace Sonar.Messages
{
    [MessagePackObject]
    public sealed class ClientIdentifier : ISonarMessage
    {
        [Key(0)]
        public string? ClientId { get; set; }

        [Key(1)]
        public string? ClientSecret { get; set; } // TODO: Not implemented
    }
}
