using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static Sonar.SonarConstants;
using static Sonar.Utilities.UnixTimeHelper;
using Sonar.Messages;
using Sonar.Connections;

namespace Sonar.Services
{
    public sealed class PingService : IAsyncDisposable, IDisposable
    {
        private Timer timer;
        private uint pingSequence; // Interlocked
        private double pingTimestamp;
        private double lastMessageTimestamp;

        private SonarClient Client { get; }

        public double Ping { get; private set; }

        internal PingService(SonarClient client)
        {
            this.Client = client;

            this.Client.Connection.MessageReceived += this.MessageHandler;

            this.lastMessageTimestamp = UnixNow;
            this.timer = new(this.TimerHandler, null, (int)EarthSecond * 5, (int)EarthSecond * 5);
        }

        private void TimerHandler(object? _)
        {
            try
            {
                var now = UnixNow;
                if (now > this.lastMessageTimestamp + EarthSecond * 15)
                {
                    this.Client.Connection.ReconnectInternal(false); // Cause a normal reconnect
                    this.lastMessageTimestamp = now;
                }
                else
                {
                    var sequence = Interlocked.Increment(ref this.pingSequence);
                    this.pingTimestamp = now;
                    this.Client.Connection.SendIfConnected(() => new SonarPing { Sequence = sequence });
                }
            }
            catch (Exception ex)
            {
                this.Client.LogError(ex, "Ping handler exception");
            }
        }

        internal void Poke()
        {
            this.lastMessageTimestamp = UnixNow;
        }

        private void MessageHandler(SonarConnectionManager _, ISonarMessage message)
        {
            this.Poke();
            if (message is not SonarPong pong) return;
            if (pong.Sequence != this.pingSequence) return;

            var now = UnixNow;
            this.lastMessageTimestamp = now;

            this.Ping = now - this.pingTimestamp; // Worst case: at exactly 5000ms ping there's a small tiny chance of a race condition, you got 0 ping! <-- determined not possible due to order of operations
            if (this.Client.LogDebugEnabled) this.Client.LogVerbose($"Server Ping: {(int)this.Ping}ms");
            try
            {
                this.Pong?.Invoke(this, this.Ping);
            }
            catch (Exception ex)
            {
                if (this.Client.LogErrorEnabled) this.Client.LogError(ex, string.Empty);
            }
        }

        public event PingServiceEventHandler? Pong;

        public async ValueTask DisposeAsync()
        {
            this.Client.Connection.MessageReceived -= this.MessageHandler;
            await this.timer.DisposeAsync();
        }

        public void Dispose()
        {
            this.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    public delegate void PingServiceEventHandler(PingService pinger, double ping);
}
