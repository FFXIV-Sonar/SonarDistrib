using MessagePack;
using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    /// <summary>Request Server to perform verification.</summary>
    [MessagePackObject]
    public sealed class LodestoneVerificationRequest : ISonarMessage
    {
        // Empty
    }
}
