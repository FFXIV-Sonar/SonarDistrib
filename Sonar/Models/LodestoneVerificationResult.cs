using MessagePack;
using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    /// <summary>Inform client of verification result.</summary>
    [MessagePackObject]
    public sealed class LodestoneVerificationResult : ISonarMessage
    {
        /// <summary>Verification succeeded.</summary>
        [Key(0)]
        public bool Success { get; init; }
    }
}
