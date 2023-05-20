using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar.Connections
{
    public sealed partial class SonarConnectionManager
    {
        internal bool SendIfConnected(ISonarMessage message)
        {
            var socket = this._socket;
            if (socket is null) return false;
            try
            {
                socket.Send(message.SerializeClientToServer());
                return true;
            }
            catch
            {
                /* Swallow exception */
                return false;
            }
        }

        internal bool SendIfConnected(Func<ISonarMessage> messageFactory)
        {
            if (this._socket is null) return false;
            return this.SendIfConnected(messageFactory()); // It still possible this return false if disconnected while running the factory
        }

        internal bool Start()
        {
            if (Interlocked.CompareExchange(ref this._started, 1, 0) != 0) return false;
            _ = this.ConnectLoopTask();
            return true;
        }
    }
}
