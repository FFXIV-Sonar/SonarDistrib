using MessagePack;
using Sonar.Utilities;

namespace Sonar.Messages
{
    [MessagePackObject]
    public sealed class ClientIdentifier : ISonarMessage
    {
        private string? _clientHash;

        [Key(0)]
        public string? ClientId { get; init; }

        [Key(1)]
        public string? ClientSecret { get; init; }

        [IgnoreMember]
        public string? ClientHash => this._clientHash ??= IdentifierUtils.GenerateClientHash(this.ClientId, this.ClientSecret);
    }
}
