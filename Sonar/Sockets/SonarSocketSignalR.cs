using Microsoft.AspNetCore.SignalR.Client;
using Sonar.Messages;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sonar.Sockets
{
    public sealed partial class SonarSocketSignalR : SonarSocket
    {
        private readonly HubConnection _connection;
        private readonly CancellationTokenSource _cts = new();
        private readonly ActionBlock<(string, byte[])> _sendBlock;

        public override Task Completion { get; protected set; }

        public SonarSocketSignalR(HubConnection connection, int sendQueueSize, Func<byte[], ISonarMessage> bytesToMessages, Func<ISonarMessage, byte[]> messageToBytes) : base(bytesToMessages, messageToBytes)
        {
            this._sendBlock = new(this.SendBlockHandler, new()
            {
                BoundedCapacity = sendQueueSize,
                MaxDegreeOfParallelism = 1,
                CancellationToken = this._cts.Token
            });
            this.Completion = this._sendBlock.Completion;

            connection.On<byte[]>("message", this.MessageHandler);
            connection.On<byte[]>("text", this.TextHandler);
            connection.Closed += this.Connection_Closed;
            this._connection = connection;
        }

        private Task Connection_Closed(Exception? arg)
        {
            this.DispatchDisconnectedEvent();
            return Task.CompletedTask;
        }

        private async Task SendBlockHandler((string method, byte[] obj) message)
        {
            try
            {
                await this._connection.SendAsync(message.method, message.obj, this._cts.Token);
            }
            catch (OperationCanceledException) { /* Swallow */ }
            catch (Exception ex)
            {
                this.DispatchExceptionEvent(ex);
            }
        }

        public Task MessageHandler(byte[] bytes)
        {
            return this.ProcessReceivedBytesAsync(bytes);
        }

        public Task TextHandler(byte[] textBytes)
        {
            return this.ProcessReceivedTextAsync(Encoding.UTF8.GetString(textBytes));
        }

        public override void Start()
        {
            _ = this._connection.StartAsync(this._cts.Token);
        }

        public override void Send(byte[] bytes)
        {
            this._sendBlock.Post(("message", bytes));
        }

        public override void SendText(byte[] textBytes)
        {
            this._sendBlock.Post(("text", textBytes));
        }

        public override Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            return this._sendBlock.SendAsync(("message", bytes), this._cts.Token);
        }

        public override Task SendTextAsync(byte[] textBytes, CancellationToken cancellationToken = default)
        {
            return this._sendBlock.SendAsync(("text", textBytes), this._cts.Token);
        }

        protected override void Dispose(bool disposing)
        {
            this._cts.Cancel();
            this._cts.Dispose();
            this._connection.DisposeAsync().AsTask().GetAwaiter().GetResult();
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await this._cts.CancelAsync();
            this._cts.Dispose();
            await this._connection.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
