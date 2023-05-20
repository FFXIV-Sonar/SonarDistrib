using MessagePack;
using Sonar.Messages;
using Sonar.Relays;
using System.Diagnostics.CodeAnalysis;

namespace Sonar.Models
{
    [MessagePackObject]
    //[SuppressMessage("Major Code Smell", "S2326", Justification = "Ability to tell relay type")]
    public readonly struct RelayConfirmationSlim<T> : ISonarMessage where T : Relay
    {
        [Key(0)]
        public string RelayKey { get; init; }
    }
}
