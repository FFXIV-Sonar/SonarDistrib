using MessagePack;
using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    /// <summary>Lodestone Verification Required.</summary>
    [MessagePackObject]
    public sealed class LodestoneVerificationNeeded : ISonarMessage
    {
        /// <summary>Code required on bio.</summary>
        [Key(0)]
        public required string Code { get; init; }

        /// <summary>Verification is a requirement.</summary>
        [Key(1)]
        public bool Required { get; init; }

        /// <summary>Reason of verification.</summary>
        [Key(2)]
        public required LodestoneVerificationReason Reason { get; init; }

        /// <summary>Lodestone ID, if available. -1 otherwise.</summary>
        [Key(3)]
        public long LodestoneId { get; init; }

        /// <summary>Time this becomes a requirement. <c>0</c> if none.</summary>
        [Key(4)]
        public double RequiredTime { get; init; }
    }
}
