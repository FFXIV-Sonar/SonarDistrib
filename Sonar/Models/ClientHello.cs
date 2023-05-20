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
        [Key(0)]
        public SonarVersion? Version { get; set; } = null!;

        [Key(1)]
        public string? ClientIdentifier { get; set; } = null!;

        [Key(2)]
        public string? HardwareIdentifier { get; set; } = null!;

        [Key(3)]
        public ClientSecret SonarSecret { get; set; } = default;

        [Key(4)]
        public ClientSecret PluginSecret { get; set; } = default;
    }
}
