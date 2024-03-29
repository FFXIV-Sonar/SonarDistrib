﻿using Sonar.Messages;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Sonar.Sockets
{
    public interface ISonarSocket : IDisposable, IAsyncDisposable
    {
        public Task Completion { get; }
        public void Start();
        public void Send(byte[] bytes);
        public void Send(string text);
        public void Send(ISonarMessage message);
        public void SendText(byte[] textBytes);
        public Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default);
        public Task SendAsync(string text, CancellationToken cancellationToken = default);
        public Task SendAsync(ISonarMessage message, CancellationToken cancellationToken = default);
        public Task SendTextAsync(byte[] textBytes, CancellationToken cancellationToken = default);

        public void AddHandler(Type type, Action<ISonarSocket, ISonarMessage> handler);
        public void AddHandler(Type type, Func<ISonarSocket, ISonarMessage, Task> handler);
        public void AddHandler<T>(Action<ISonarSocket, T> handler) where T : ISonarMessage;
        public void AddHandler<T>(Func<ISonarSocket, T, Task> handler) where T : ISonarMessage;

        public bool RemoveHandler(Type type, Action<ISonarSocket, ISonarMessage> handler);
        public bool RemoveHandler(Type type, Func<ISonarSocket, ISonarMessage, Task> handler);
        public bool RemoveHandler<T>(Action<ISonarSocket, T> handler) where T : ISonarMessage;
        public bool RemoveHandler<T>(Func<ISonarSocket, T, Task> handler) where T : ISonarMessage;

        public event Action<ISonarSocket>? Connected;
        public event Action<ISonarSocket>? Disconnected;
        public event Action<ISonarSocket, byte[]>? RawReceived;
        public event Func<ISonarSocket, byte[], Task>? RawReceivedAsync;
        public event Action<ISonarSocket, ISonarMessage>? MessageReceived;
        public event Func<ISonarSocket, ISonarMessage, Task>? MessageReceivedAsync;
        public event Action<ISonarSocket, string>? TextReceived;
        public event Func<ISonarSocket, string, Task>? TextReceivedAsync;
        public event Action<ISonarSocket, Exception>? Exception;
    }
}
