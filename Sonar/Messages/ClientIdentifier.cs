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
        public string? Identifier { get; set; }
    }
}
