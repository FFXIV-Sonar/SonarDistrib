﻿using Sonar.Messages;
using Sonar.Models;
using Sonar.Sockets;
using Sonar.Utilities;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using static Sonar.SonarConstants;
using Sonar.Extensions;
using DryIoc;
using DryIocAttributes;
using System.ComponentModel.Composition;
using DryIoc.FastExpressionCompiler.LightExpression;
using SonarUtils;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Sonar.Connections
{
    /// <summary>Manages connectivity to the Sonar server</summary>
    [SingletonReuse]
    [ExportEx]
    public sealed partial class SonarConnectionManager : IDisposable, IAsyncDisposable
    {
        private readonly SonarUrlManager _urls = new();

        private int _started;
        private readonly CancellationTokenSource _cts = new();

        private volatile ISonarSocket? _socket;
        private readonly NonBlocking.NonBlockingDictionary<ISonarSocket, SonarConnectionInformation> _sockets = new();
        private volatile int _failCount;
        private volatile bool _reconnect;

        private SonarClient Client { get; }

        [ImportingConstructor]
        internal SonarConnectionManager(SonarClient client)
        {
            this.Client = client;
            this.Client.LogDebug(() => $"{nameof(SonarConnectionManager)} Initialized");
        }

        /// <summary>Connectivity status</summary>
        public bool IsConnected => this._socket is not null;

        /// <summary>Number of connections currently active and ready to Sonar Server</summary>
        /// <remarks>Redundant connections are used in case of disconnects or connectivity problems</remarks>
        public int Count => this._sockets.Count;

        /// <summary>Triggers a reconnect</summary>
        public void Reconnect() => this.ReconnectInternal();

        /// <summary>Triggers a reconnect</summary>
        public void ReconnectInternal(bool reconnect = true)
        {
            if (reconnect)
            {
                this._failCount = 0;
                this._reconnect = true;
            }
            this._socket?.Dispose();
            if (reconnect)
            {
                this._failCount = 0;
                this._reconnect = true;
            }
        }

        /// <summary>Remove redundant connections to Sonar Server</summary>
        /// <remarks>Redundant connections are used in case of disconnects or connectivity problems</remarks>
        public void RemoveRedundantConnections()
        {
            // This must be safe... yes its safe!
            this._sockets.Keys.Where(socket => socket != this._socket).ForEach(socket => socket.Dispose());
        }

        /// <summary>Sonar is connected</summary>
        [SuppressMessage("Annoying Code Smell", "S3264", Justification = ".SafeInvoke()")]
        public event Action<SonarConnectionManager, uint>? Connected;

        /// <summary>Sonar has disconnected</summary>
        [SuppressMessage("Annoying Code Smell", "S3264", Justification = ".SafeInvoke()")]
        public event Action<SonarConnectionManager>? Disconnected;

        internal event Action<SonarConnectionManager, ISonarSocket, uint>? ConnectedInternal;
        internal event Action<SonarConnectionManager, ISonarSocket>? DisconnectedInternal;
        internal event Action<SonarConnectionManager, byte[]>? RawReceived;
        internal event Action<SonarConnectionManager, ISonarMessage>? MessageReceived;
        internal event Action<SonarConnectionManager, string>? TextReceived;

        private async Task ConnectLoopTask()
        {
            var token = this._cts.Token;
            try
            {
                // Initial delay
                await Task.Delay(InitialDelayMs, token);

                // Snapshot this._failCount and begin loop
                var failCount = this._failCount;
                while (!this._cts.IsCancellationRequested)
                {
                    // Calculate when the next attempt should be made
                    var endTicks = System.Environment.TickCount + GetNextAttemptDelayMs(failCount);

                    // Try getting an available socket if current active socket is null
                    this.EnsureConnectionIfAvailable();
                    
                    // Attempting connecting if there is no active socket
                    if (this._socket is null)
                    {
                        _ = this.ConnectAttemptTask();
                        failCount = Interlocked.Increment(ref this._failCount); // await boundary of ConnectAttemptTask() is after getting url so this is safe
                    }
                    else
                    {
                        this._failCount = failCount = 0;
                        this._reconnect = false;
                    }

                    // Continue attempting as long as connected socket is below soft limit
                    if (this._sockets.Count > SocketsCapacity && this.TryGetNextSocket(out var socket, out _)) this.DisposeSocket(socket);

                    // Wait until next attempt interval
                    while (!this._cts.IsCancellationRequested && this._failCount == failCount)
                    {
                        var startTicks = System.Environment.TickCount;
                        var ticks = endTicks - startTicks;
                        if (ticks <= 0) break;
                        await Task.Delay(GetVariedIntervalMs(RecheckIntervalMs), token);
                        this.EnsureConnectionIfAvailable();
                    }

                    // Update snapshot
                    failCount = this._failCount;
                }
            }
            catch { /* Swallow */ }
        }

        private Task ConnectAttemptTask()
        {
            var url = this._urls.GetRandomUrl(this._reconnect || this._failCount >= FailureThreshold, this._reconnect);
            return url.Type switch
            {
                ConnectionType.WebSocket => this.ConnectionAttemptWebSocket(url),
                ConnectionType.SignalR => this.ConnectionAttemptSignalR(url),
                _ => throw new ArgumentException($"Invalid Connection Type for {url.Key}: {url.Type}")
            };

        }

        private async Task ConnectionAttemptWebSocket(SonarUrl url)
        {
            using var cts = new CancellationTokenSource(ConnectTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(this._cts.Token, cts.Token);
            var token = linkedCts.Token;

            var webSocket = new ClientWebSocket();

            var socket = SonarSocketWebSocket.CreateClientSocket(webSocket); // webSocket is now "owned" by this SonarSocket
            this.PrepareSocket(socket, url);

            try
            {
                await webSocket.ConnectAsync(url, HappyHttpUtils.CreateRandomlyHappyClient(), token);
                socket.Start();
                await Task.Delay(ReadyTimeoutMs, token);
                if (this._sockets.TryGetValue(socket, out var info) && !info.Id.HasValue) this.DisposeSocket(socket);
            }
            catch (Exception ex)
            {
                // Only log exception if not connected.
                if (this._socket is null && ex is not OperationCanceledException) this.Client.LogError(ex, $"Connection exception at {url.Key} ({url.Type})");

                // Dispose of faulty socket
                this.DisposeSocket(socket);
            }
        }

        private async Task ConnectionAttemptSignalR(SonarUrl url)
        {
            using var cts = new CancellationTokenSource(ConnectTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(this._cts.Token, cts.Token);
            var token = linkedCts.Token;

            var connection = new HubConnectionBuilder()
                .WithUrl(url.Url, configureHttpConnection: options => options.HttpMessageHandlerFactory = _ => HappyHttpUtils.CreateRandomlyHappyHandler())
                .AddMessagePackProtocol(configure => configure.SerializerOptions = SonarSerializer.MessagePackOptions)
                .Build();

            var socket = SonarSocketSignalR.CreateClientSocket(connection); // connection is now "owned" by this SonarSocket
            this.PrepareSocket(socket, url);

            try
            {
                await connection.StartAsync(token);
                socket.Start();
                await Task.Delay(ReadyTimeoutMs, token);
                if (this._sockets.TryGetValue(socket, out var info) && !info.Id.HasValue) this.DisposeSocket(socket);
            }
            catch (Exception ex)
            {
                // Only log exception if not connected.
                if (this._socket is null && ex is not OperationCanceledException) this.Client.LogError(ex, $"Connection exception at {url.Key} ({url.Type})");

                // Dispose of faulty socket
                this.DisposeSocket(socket);
            }
        }

        private void EnsureConnectionIfAvailable()
        {
            var socket = this._socket;

            // Make sure current active socket is valid (still connected) and disconnect if not
            if (socket is not null && !this._sockets.ContainsKey(socket) && Interlocked.CompareExchange(ref this._socket, null, socket) == socket)
            {
                this.Client.LogWarning("Sonar Disconnected");
                this.DisconnectedInternal?.Invoke(this, socket);
                if (this.Disconnected is not null) _ = Task.Run(() => this.Disconnected?.SafeInvoke(this));
            }

            // Try getting an available socket if current active socket is null
            if (this._socket is null && this.TryGetNextSocket(out socket, out var info) && Interlocked.CompareExchange(ref this._socket, socket, null) == null)
            {
                this.Client.LogInformation(() => $"Sonar Connected (Connection ID: {info.Id} | Type: {info.Type}{(info.Url.Proxy ? $" | Proxy" : string.Empty)})");
                this.ConnectedInternal?.Invoke(this, socket, info.Id!.Value);
                if (this.Connected is not null) _ = Task.Run(() => this.Connected?.SafeInvoke(this, info.Id!.Value));
            }
        }

        private void PrepareSocket(ISonarSocket socket, SonarUrl url)
        {
            this._sockets.TryAdd(socket, new() { Url = url });
            socket.Disconnected += this.SocketDisconnectedHandler;
            socket.RawReceived += this.SocketRawHandler;
            socket.MessageReceived += this.SocketMessageHandler;
            socket.TextReceived += this.SocketTextHandler;
            // socket.Start() // NOTE: This is now done at the attempt tasks
        }

        private void DisposeSocket(ISonarSocket socket)
        {
            try
            {
                this._sockets.TryRemove(socket, out _);
                socket.Disconnected -= this.SocketDisconnectedHandler;
                socket.RawReceived -= this.SocketRawHandler;
                socket.MessageReceived -= this.SocketMessageHandler;
                socket.TextReceived -= this.SocketTextHandler;
                socket.Dispose();
            }
            catch { /* Swallow */ }
        }

        private void SocketTextHandler(ISonarSocket socket, string arg2)
        {
            if (this._socket == socket) this.TextReceived?.Invoke(this, arg2);
        }

        private void SocketRawHandler(ISonarSocket socket, byte[] arg2)
        {
            if (this._socket == socket) this.RawReceived?.Invoke(this, arg2);
        }

        private void SocketDisconnectedHandler(ISonarSocket socket)
        {
            // Dispose this socket
            this.DisposeSocket(socket);

            // Insta-reconnect if we got another readily available socket
            this.EnsureConnectionIfAvailable();
        }

        private void SocketMessageHandler(ISonarSocket socket, ISonarMessage message)
        {
            if (this._socket == socket) this.MessageReceived?.Invoke(this, message);
            else if (message is ServerReady ready && this._sockets.TryGetValue(socket, out var info))
            {
                info.Id = ready.ConnectionId;
                info.Type = ready.ConnectionType;
                socket.Send(ready);
                this.EnsureConnectionIfAvailable();
            }
        }

        private bool TryGetNextSocket([NotNullWhen(true)] out ISonarSocket socket, out SonarConnectionInformation info)
        {
            if (!this._cts.IsCancellationRequested)
            {
                (socket, info) = this._sockets.FirstOrDefault(kvp => kvp.Key != this._socket && kvp.Value.Id.HasValue)!;
                if (socket is null) return false;
                return true;
            }
            socket = null!; info = null!;
            return false;
        }

        void IDisposable.Dispose()
        {
            this._cts.Cancel();
            foreach (var socket in this._sockets.Keys)
            {
                socket.Dispose();
            }
            this.Client.LogDebug(() => $"{nameof(SonarConnectionManager)} disposed");
            this._cts.Dispose();
            this._urls.Dispose();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await this._cts.CancelAsync();
            await Task.WhenAll(this._sockets.Keys.Select(s => s.DisposeAsync().AsTask()));
            this.Client.LogDebug(() => $"{nameof(SonarConnectionManager)} disposed (async)");
            this._cts.Dispose();
            await this._urls.DisposeAsync();
        }
    }
}
