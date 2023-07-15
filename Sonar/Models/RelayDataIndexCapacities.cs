using MessagePack;
using Sonar.Messages;
using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    [MessagePackObject]
    [Obsolete("No longer used")]
    public sealed class RelayDataIndexCapacities : ISonarMessage
    {
        [Key(0)]
        public required IReadOnlyDictionary<string, int> Hunts { get; init; }

        [Key(1)]
        public required IReadOnlyDictionary<string, int> Fates { get; init; }
    }
}
