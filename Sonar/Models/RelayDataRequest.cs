using MessagePack;
using System;
using Sonar.Messages;

namespace Sonar.Models
{
    [MessagePackObject]
    [Serializable]
    public readonly struct RelayDataRequest : ISonarMessage
    {
        /* Empty */
    }
}
