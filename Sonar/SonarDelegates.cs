using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Sonar.Enums;
using Sonar.Logging;
using Sonar.Messages;

namespace Sonar
{
    public delegate void SonarClientEventHandler(SonarClient source);
    public delegate void SonarClientDisconnectHandler(SonarClient source);
    public delegate void SonarClientRawHandler(SonarClient source, byte[] bytes);
    public delegate void SonarClientLogHandler(SonarClient source, SonarLogMessage log);
    public delegate void SonarClientMessageHandler(SonarClient source, ISonarMessage message);
    public delegate void SonarClientTextHandler(SonarClient source, string message);
    public delegate void SonarClientPingHandler(SonarClient source, double ping);

    public delegate void SonarServerMessageHandler(SonarClient source, string? message);
}
