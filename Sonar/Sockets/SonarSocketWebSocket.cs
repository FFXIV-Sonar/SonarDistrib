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
using System.Xml.XPath;
using System.Collections.Concurrent;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Sonar.Sockets
{
    public sealed partial class SonarSocketWebSocket : SonarSocket
    {
        private const int BufferSize = 4096;
        private const int MaximumReturnedMessageBufferSize = 65536;

        private static readonly ConcurrentBag<List<byte>> s_messageBufferBag = [];
        private readonly int _maxMessageBytes;
        private readonly CancellationTokenSource _cts = new();
        private readonly ActionBlock<(WebSocketMessageType, byte[])> _sendBlock;
        private bool _disposed;

        public WebSocket WebSocket { get; }

        private bool _started;
        private Task? _receiveTask;

        public override Task Completion { get; protected set; }

        public SonarSocketWebSocket(WebSocket webSocket, int maxMessageBytes, int sendQueueSize, Func<byte[], ISonarMessage> bytesToMessages, Func<ISonarMessage, byte[]> messageToBytes) : base(bytesToMessages, messageToBytes)
        {
            this.WebSocket = webSocket;
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
                await this.WebSocket.SendAsync(message.bytes, message.type, WebSocketMessageFlags.EndOfMessage | WebSocketMessageFlags.DisableCompression, this._cts.Token);
            }
            catch (OperationCanceledException) { /* Swallow */ }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.InvalidState) { /* Swallow */ }
            catch (Exception ex)
            {
                this.DispatchExceptionEvent(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] RentReceiveBuffer()
        {
            return ArrayPool<byte>.Shared.Rent(4096);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnReceiveBuffer(byte[]? buffer)
        {
            if (buffer is null || buffer.Length is 0) return;
            ArrayPool<byte>.Shared.Return(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<byte> RentMessageBuffer()
        {
            if (!s_messageBufferBag.TryTake(out var buffer)) buffer = new(BufferSize);
            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnMessageBuffer(List<byte>? buffer)
        {
            if (buffer is null) return;
            buffer.Clear();
            if (buffer.Capacity > MaximumReturnedMessageBufferSize) buffer.Capacity = BufferSize;
            Debug.Assert(buffer.Capacity is >= BufferSize and <= MaximumReturnedMessageBufferSize);
            s_messageBufferBag.Add(buffer);
        }

        private async Task WebSocketReceiveTask(CancellationToken cancellationToken)
        {
            // These are rented or created only as needed.
            List<byte>? messageBuffer = null; // Taken from bag
            byte[] receiveBuffer = []; // Rented from ArrayPool<byte>.Shared

            // Receive logic
            try
            {
                this.DispatchConnectedEvent();
                while (this.WebSocket.State is WebSocketState.Open)
                {
                    // Step 1: Receive data into buffer
                    ValueWebSocketReceiveResult result;
                    while (true)
                    {
                        // Read into receive buffer (empty buffer on first iteration only).
                        result = await this.WebSocket.ReceiveAsync(receiveBuffer.AsMemory(), cancellationToken);
                        if (result.EndOfMessage || result.Count > 0) break;

                        // Rent a receive buffer.
                        if (receiveBuffer.Length is 0) receiveBuffer = RentReceiveBuffer();
                    }

                    // Step 2: Assemble message
                    if ((messageBuffer?.Count ?? 0) + result.Count > this._maxMessageBytes)
                    {
                        await this.WebSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, $"Maximum Message Size is {this._maxMessageBytes}", cancellationToken);
                        break;
                    }

                    if (result.Count is not 0)
                    {
                        // Rent a message buffer.
                        messageBuffer ??= RentMessageBuffer();

                        // Append received buffer into message buffer.
                        messageBuffer.AddRange(receiveBuffer.AsSpan(0, result.Count));
                    }

                    // Return the receive buffer.
                    ReturnReceiveBuffer(receiveBuffer);
                    receiveBuffer = [];

                    // Step 3: Process message
                    if (result.EndOfMessage)
                    {
                        switch (result.MessageType)
                        {
                            case WebSocketMessageType.Binary:
                                await this.ProcessReceivedBytesAsync(messageBuffer is not null ? [.. messageBuffer] : []);
                                break;
                            case WebSocketMessageType.Text:
                                await this.ProcessReceivedTextAsync(Encoding.UTF8.GetString(CollectionsMarshal.AsSpan(messageBuffer)));
                                break;
                            case WebSocketMessageType.Close:
                                if (this.WebSocket.CloseStatus is not WebSocketCloseStatus.NormalClosure and not WebSocketCloseStatus.EndpointUnavailable)
                                {
                                    this.DispatchExceptionEvent(ExceptionDispatchInfo.SetCurrentStackTrace(new WebSocketException($"{this.WebSocket.CloseStatus}: {this.WebSocket.CloseStatusDescription}")));
                                }
                                await this.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                                break;
                            default:
                                this.DispatchExceptionEvent(new WebSocketException(WebSocketError.InvalidMessageType, $"{result.MessageType}"));
                                break;
                        }
                            
                        // Return the message buffer.
                        ReturnMessageBuffer(messageBuffer);
                        messageBuffer = null;
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
            catch (Exception ex)
            {
                this.DispatchExceptionEvent(ex);
            }
            finally
            {
                // Return the buffers.
                ReturnReceiveBuffer(receiveBuffer);
                ReturnMessageBuffer(messageBuffer);

                await this.DisposeAsync();

                try
                {
                    this.DispatchDisconnectedEvent();
                }
                catch (Exception ex)
                {
                    this.DispatchExceptionEvent(ex);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref this._disposed, true, false)) return;
            try
            {
                this._cts.Cancel();
                this._cts.Dispose();
                this.WebSocket.Dispose();
                this._sendBlock.Complete();
            }
            catch
            {
                /* Swallow */
            }
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref this._disposed, true, false)) return;
            try
            {
                await this._cts.CancelAsync();
                this._cts.Dispose();
                this.WebSocket.Dispose();
                this._sendBlock.Complete();
            }
            catch
            {
                /* Swallow */
            }
            await base.DisposeAsync();
        }

        public override void Start()
        {
            if (Interlocked.CompareExchange(ref this._started, true, false)) return;
            this._receiveTask = this.WebSocketReceiveTask(this._cts.Token);
            this.Completion = Task.WhenAll(this._sendBlock.Completion, this._receiveTask);
        }

        public override void Send(byte[] bytes) => this._sendBlock.Post((WebSocketMessageType.Binary, bytes));

        public override void SendText(byte[] textBytes) => this._sendBlock.Post((WebSocketMessageType.Text, textBytes));

        public override Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default) => this._sendBlock.SendAsync((WebSocketMessageType.Binary, bytes), cancellationToken);

        public override Task SendTextAsync(byte[] textBytes, CancellationToken cancellationToken = default) => this._sendBlock.SendAsync((WebSocketMessageType.Text, textBytes), cancellationToken);
    }
}
