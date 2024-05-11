using Sonar.Extensions;
using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SonarUtils;
using DryIoc;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace Sonar.Sockets
{
    public abstract class SonarSocket : ISonarSocket
    {
        protected readonly Func<byte[], ISonarMessage> ConvertBytesToMessage;
        protected readonly Func<ISonarMessage, byte[]> ConvertMessageToBytes;
        private readonly ConcurrentDictionary<Type, ImmutableArray<object>> _messageHandlers = new();
        private readonly ConcurrentDictionary<Type, ImmutableArray<object>> _asyncMessageHandlers = new();

        public abstract Task Completion { get; protected set; }

        protected SonarSocket(Func<byte[], ISonarMessage> bytesToMessages, Func<ISonarMessage, byte[]> messageToBytes)
        {
            this.ConvertBytesToMessage = bytesToMessages ?? throw new ArgumentNullException(nameof(bytesToMessages));
            this.ConvertMessageToBytes = messageToBytes ?? throw new ArgumentNullException(nameof(messageToBytes));
        }

        protected async Task ProcessReceivedBytesAsync(byte[] bytes)
        {
            var message = this.ConvertBytesToMessage(bytes);
            await this.DispatchEventPairAsync(this.RawReceived, this.RawReceivedAsync, bytes); // This is after conversion to ensure its a valid message
            await this.ProcessMessageAsync(message);
        }

        private async Task ProcessMessageAsync(ISonarMessage message)
        {
            if (message is null) // Should never happen but... its happening with very old versions of Sonar
            {
                this.DispatchExceptionEvent(ExceptionDispatchInfo.SetCurrentStackTrace(new NullReferenceException("message is null")));
                return;
            }
            if (message is MessageList messages)
            {
                foreach (var item in messages) await this.ProcessMessageAsync(item);
                return;
            }
            await this.ProcessMessageCoreAsync(message);
        }

        private async Task ProcessMessageCoreAsync(ISonarMessage message)
        {
            var types = message.GetAllTypes();
            foreach (var type in types)
            {
                if (this._messageHandlers.TryGetValue(type, out var handlers) && !handlers.IsEmpty)
                {
                    for (var i = 0; i < handlers.Length; i++)
                    {
                        try
                        {
                            Unsafe.As<Action<ISonarSocket, ISonarMessage>>(handlers[i])(this, message);
                        }
                        catch (Exception ex)
                        {
                            this.DispatchExceptionEvent(ex);
                        }
                    }
                }

                if (this._asyncMessageHandlers.TryGetValue(type, out var asyncHandlers) && !asyncHandlers.IsEmpty)
                {
                    for (var i = 0; i < asyncHandlers.Length; i++)
                    {
                        try
                        {
                            await Unsafe.As<Func<ISonarSocket, ISonarMessage, Task>>(asyncHandlers[i])(this, message);
                        }
                        catch (Exception ex)
                        {
                            this.DispatchExceptionEvent(ex);
                        }
                    }
                }
            }
        }

        public void AddHandler(Type type, Action<ISonarSocket, ISonarMessage> handler)
        {
            this._messageHandlers.AddOrUpdate(type, static (type, handler) => ImmutableArray.Create((object)handler), static (type, handlers, handler) => handlers.Add(handler), handler);
        }

        public void AddHandler(Type type, Func<ISonarSocket, ISonarMessage, Task> handler)
        {
            this._asyncMessageHandlers.AddOrUpdate(type, static (type, handler) => ImmutableArray.Create((object)handler), (type, handlers, handler) => handlers.Add(handler), handler);
        }

        public void AddHandler<T>(Action<ISonarSocket, T> handler) where T : ISonarMessage
        {
            this.AddHandler(typeof(T), Unsafe.As<Action<ISonarSocket, ISonarMessage>>(handler));
        }

        public void AddHandler<T>(Func<ISonarSocket, T, Task> handler) where T : ISonarMessage
        {
            this.AddHandler(typeof(T), Unsafe.As<Func<ISonarSocket, ISonarMessage, Task>>(handler));
        }

        public bool RemoveHandler(Type type, Action<ISonarSocket, ISonarMessage> handler)
        {
            while (true)
            {
                if (!this._messageHandlers.TryGetValue(type, out var handlers)) return false;
                var newHandlers = handlers.Remove(handler);
                if (handlers.Equals(newHandlers)) return false;
                if (this._messageHandlers.TryUpdate(type, newHandlers, handlers)) return true;
            }
        }

        public bool RemoveHandler(Type type, Func<ISonarSocket, ISonarMessage, Task> handler)
        {
            while (true)
            {
                if (!this._asyncMessageHandlers.TryGetValue(type, out var handlers)) return false;
                var newHandlers = handlers.Remove(handler);
                if (handlers.Equals(newHandlers)) return false;
                if (this._asyncMessageHandlers.TryUpdate(type, newHandlers, handlers)) return true;
            }
        }

        public bool RemoveHandler<T>(Action<ISonarSocket, T> handler) where T : ISonarMessage
        {
            return this.RemoveHandler(typeof(T), Unsafe.As<Action<ISonarSocket, ISonarMessage>>(handler));
        }

        public bool RemoveHandler<T>(Func<ISonarSocket, T, Task> handler) where T : ISonarMessage
        {
            return this.RemoveHandler(typeof(T), Unsafe.As<Func<ISonarSocket, ISonarMessage, Task>>(handler));
        }

        protected async Task ProcessReceivedTextAsync(string text)
        {
            await this.DispatchEventPairAsync(this.TextReceived, this.TextReceivedAsync, text);
        }

        public abstract void Start();
        public abstract void Send(byte[] bytes);
        public void Send(string text) => this.SendText(Encoding.UTF8.GetBytes(text));
        public void Send(ISonarMessage message) => this.Send(this.ConvertMessageToBytes(message));
        public abstract void SendText(byte[] textBytes);
        public abstract Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default);
        public Task SendAsync(string text, CancellationToken cancellationToken = default) => this.SendAsync(Encoding.UTF8.GetBytes(text), cancellationToken);
        public Task SendAsync(ISonarMessage message, CancellationToken cancellationToken = default) => this.SendAsync(this.ConvertMessageToBytes(message), cancellationToken);
        public abstract Task SendTextAsync(byte[] textBytes, CancellationToken cancellationToken = default);

        public event Action<ISonarSocket>? Connected;
        public event Action<ISonarSocket>? Disconnected;
        public event Action<ISonarSocket, byte[]>? RawReceived;
        public event Func<ISonarSocket, byte[], Task>? RawReceivedAsync;
        public event Action<ISonarSocket, string>? TextReceived;
        public event Func<ISonarSocket, string, Task>? TextReceivedAsync;
        public event Action<ISonarSocket, Exception>? Exception;

        public event Action<ISonarSocket, ISonarMessage>? MessageReceived
        {
            add => this.AddHandler(typeof(ISonarMessage), value!);
            remove => this.RemoveHandler(typeof(ISonarMessage), value!);
        }
        public event Func<ISonarSocket, ISonarMessage, Task>? MessageReceivedAsync
        {
            add => this.AddHandler(typeof(ISonarMessage), value!);
            remove => this.RemoveHandler(typeof(ISonarMessage), value!);
        }

        protected void DispatchExceptionEvent(Exception exception) => this.Exception?.SafeInvoke(this, exception);
        protected void DispatchConnectedEvent() => this.Connected?.Invoke(this);
        protected void DispatchDisconnectedEvent() => this.Disconnected?.Invoke(this);

        protected virtual void Dispose(bool disposing)
        {
            /* Empty */
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SonarSocket() => this.Dispose(false);

        protected void DispatchExceptionEvents(IEnumerable<Exception> exceptions) => exceptions.ForEach(this.DispatchExceptionEvent);

        private async Task DispatchEventPairAsync<T>(Action<ISonarSocket, T>? syncEvent, Func<ISonarSocket, T, Task>? asyncEvent, T arg)
        {
            IEnumerable<Exception> exceptions;

            if (syncEvent is not null)
            {
                syncEvent.SafeInvoke(this, arg, out exceptions);
                this.DispatchExceptionEvents(exceptions);
            }

            if (asyncEvent is not null)
            {
                var tasks = asyncEvent.SafeInvoke(this, arg, out exceptions);
                this.DispatchExceptionEvents(exceptions);
                foreach (var task in tasks)
                {
                    try
                    {
                        await task;
                    }
                    catch (Exception ex)
                    {
                        this.DispatchExceptionEvent(ex);
                    }
                }
            }
        }

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}
