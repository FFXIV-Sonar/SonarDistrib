using SonarUtils;
using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sonar.Sockets
{
    public sealed partial class SonarSocketWebSocket : SonarSocket
    {
        private readonly int _receiveBufferSize;
        private readonly int _maxMessageBytes;
        private readonly CancellationTokenSource _cts = new();

        private readonly ActionBlock<(WebSocketMessageType, byte[])> _sendBlock;

        public WebSocket WebSocket { get; }

        private int _started;
        private Task? _receiveTask;

        public override Task Completion { get; protected set; }

        public SonarSocketWebSocket(WebSocket webSocket, int receiveBufferSize, int maxMessageBytes, int sendQueueSize, Func<byte[], ISonarMessage> bytesToMessages, Func<ISonarMessage, byte[]> messageToBytes) : base(bytesToMessages, messageToBytes)
        {
            this.WebSocket = webSocket;
            this._receiveBufferSize = receiveBufferSize;
            this._maxMessageBytes = maxMessageBytes;
            this._sendBlock = new(this.SendBlockHandler, new()
            {
                BoundedCapacity = sendQueueSize,
                MaxDegreeOfParallelism = 1,
                CancellationToken = this._cts.Token
            });
            this.Completion = this._sendBlock.Completion;
        }

        private async Task SendBlockHandler((WebSocketMessageType type, byte[] bytes) message)
        {
            try
            {
                await this.WebSocket.SendAsync(message.bytes, message.type, true, this._cts.Token);
            }
            catch (OperationCanceledException) { /* Swallow */ }
            catch (WebSocketException ex) when (ex.Message == "The WebSocket is in an invalid state ('Aborted') for this operation. Valid states are: 'Open, CloseReceived'") { /* Swallow */ }
            catch (Exception ex)
            {
                this.DispatchExceptionEvent(ex);
            }
        }

        private async Task WebSocketReceiveTask()
        {
            if (this.WebSocket.State is WebSocketState.Open)
            {
                var token = this._cts.Token;
                try
                {
                    var messageBytes = new List<byte>(this._receiveBufferSize);
                    var buffer = new byte[this._receiveBufferSize];
                    this.DispatchConnectedEvent();
                    while (true)
                    {
                        var result = await this.WebSocket.ReceiveAsync(buffer, token);
                        if (result.MessageType is WebSocketMessageType.Close)
                        {
                            if (result.CloseStatus != WebSocketCloseStatus.NormalClosure &&
                                !(result.CloseStatus == WebSocketCloseStatus.EndpointUnavailable && result.CloseStatusDescription is "Socket is Disposed" or "[maybe]CloudFlare WebSocket proxy restarting"))
                            {
                                this.DispatchExceptionEvent(ExceptionDispatchInfo.SetCurrentStackTrace(new WebSocketException($"{result.CloseStatus}: {result.CloseStatusDescription}")));
                            }
                            await this.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
                            break;
                        }

                        if (messageBytes.Count + result.Count > this._maxMessageBytes)
                        {
                            await this.WebSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, $"Maximum Message Size is {this._maxMessageBytes}", token);
                            break;
                        }
                        messageBytes.AddRange(buffer.AsSpan(0, result.Count));

                        if (result.EndOfMessage)
                        {
                            switch (result.MessageType)
                            {
                                case WebSocketMessageType.Binary:
                                    await this.ProcessReceivedBytesAsync(messageBytes.ToArray());
                                    break;
                                case WebSocketMessageType.Text:
                                    await this.ProcessReceivedTextAsync(Encoding.UTF8.GetString(messageBytes.ToArray()));
                                    break;
                                default:
                                    this.DispatchExceptionEvent(new WebSocketException(WebSocketError.InvalidMessageType, $"{result.MessageType}"));
                                    break;
                            }
                            messageBytes.Clear();
                            if (messageBytes.Capacity > buffer.Length) messageBytes.Capacity = buffer.Length;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    using var cts = new CancellationTokenSource(1000);
                    try { await this.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cts.Token); } catch { /* Swallow */ }
                }
                catch (WebSocketException ex) when ((uint)ex.HResult == 0x80004005)
                {
                    /* Swallow */
                }
                catch (WebSocketException ex)
                {
                    this.DispatchExceptionEvent(ex);
                }
                catch (Exception ex)
                {
                    this.DispatchExceptionEvent(ex);
                    try { await this.WebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.ToString(), token); } catch (Exception ex2) { this.DispatchExceptionEvent(ex2); }
                }
            }

            this._cts.Cancel();
            this.WebSocket.Dispose();

            try
            {
                this.DispatchDisconnectedEvent();
            }
            catch (Exception ex)
            {
                this.DispatchExceptionEvent(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            this._cts.Cancel();
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            this._cts.Cancel();
            await base.DisposeAsync();
        }

        public override void Start()
        {
            if (Interlocked.CompareExchange(ref this._started, 1, 0) != 0) return;
            this._receiveTask = this.WebSocketReceiveTask();
            this.Completion = Task.WhenAll(this._sendBlock.Completion, this._receiveTask);
        }

        public override void Send(byte[] bytes) => this._sendBlock.Post((WebSocketMessageType.Binary, bytes));

        public override void SendText(byte[] textBytes) => this._sendBlock.Post((WebSocketMessageType.Text, textBytes));

        public override Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default) => this._sendBlock.SendAsync((WebSocketMessageType.Binary, bytes), cancellationToken);

        public override Task SendTextAsync(byte[] textBytes, CancellationToken cancellationToken = default) => this._sendBlock.SendAsync((WebSocketMessageType.Text, textBytes), cancellationToken);
    }
}
