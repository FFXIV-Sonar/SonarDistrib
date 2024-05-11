using MessagePack;
using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [Key(1)]
        public string? ClientId { get; set; } = null!;

        [Key(5)]
        public string? ClientSecret { get; set; } = null!; // TODO: Not implemented
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
