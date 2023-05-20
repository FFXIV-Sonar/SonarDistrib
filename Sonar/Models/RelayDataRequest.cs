using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Sonar.Numerics;
using Sonar.Messages;

namespace Sonar.Models
{
    [MessagePackObject]
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public readonly struct RelayDataRequest : ISonarMessage
    {
        /* Empty */
    }
}
