using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Connections
{
    public sealed partial class SonarConnectionManager
    {
        // Houses connection information
        private sealed class SonarConnectionInformation
        {
            public SonarUrl Url { get; init; }
            public uint? Id { get; set; }
        }
    }
}
