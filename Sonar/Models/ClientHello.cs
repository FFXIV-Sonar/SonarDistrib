using MessagePack;
using Sonar.Messages;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    /// <summary>
    /// Initial identification message
    /// </summary>
    [MessagePackObject]
    public sealed class ClientHello : ISonarMessage
    {
        #region Sonar OAuth
        private string? _clientHash;

        [Key(1)]
        public string? ClientId { get; init; }

        [Key(5)]
        public string? ClientSecret { get; init; }

        [IgnoreMember]
        public string? ClientHash => this._clientHash ??= IdentifierUtils.GenerateClientHash(this.ClientId, this.ClientSecret);
        #endregion

        #region Additional Data
        [Key(0)]
        public SonarVersion? Version { get; set; }

        [Key(2)]
        public string? HardwareIdentifier { get; set; }

        [Key(3)]
        public ClientSecret? SonarSecret { get; set; }

        [Key(4)]
        public ClientSecret? PluginSecret { get; set; }
        #endregion
    }
}
