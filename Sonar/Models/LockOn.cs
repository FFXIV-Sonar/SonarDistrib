using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Sonar.Messages;
using MessagePack;
using Sonar.Utilities;

namespace Sonar.Models
{
    /// <summary>Represents a lock on request</summary>
    /// <typeparam name="T">Relay type</typeparam>
    [SuppressMessage("Major Code Smell", "S2326", Justification = "Intentional")]
    [MessagePackObject]
    public sealed class LockOn<T> : ISonarMessage where T : Relay
    {
        [Key(0)]
        public required string RelayKey { get; init; }
    }
}
