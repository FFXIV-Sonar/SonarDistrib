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

namespace Sonar.Sockets
{
    public abstract class SonarSocket : ISonarSocket
    {
        protected readonly Func<byte[], ISonarMessage> ConvertBytesToMessage;
        protected readonly Func<ISonarMessage, byte[]> ConvertMessageToBytes;
        private Dictionary<Type, Action<ISonarSocket, ISonarMessage>[]> _messageHandlers = new(); // Interlocked changes only
        private Dictionary<Type, Func<ISonarSocket, ISonarMessage, Task>[]> _asyncMessageHandlers = new(); // Interlocked changes only

        public abstract Task Completion { get; protected set; }

        protected SonarSocket(Func<byte[], ISonarMessage> bytesToMessages, Func<ISonarMessage, byte[]> messageToBytes)
        {
            this.ConvertBytesToMessage = bytesToMessages;
            this.ConvertMessageToBytes = messageToBytes;
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
                foreach (var item in messages)
                {
                    await this.ProcessMessageAsync(item);
                }
            }
            else
            {
                await this.DispatchEventPairAsync(this.MessageReceived, this.MessageReceivedAsync, message);

                if (this._messageHandlers.TryGetValue(message.GetType(), out var handlers))
                {
                    for (var i = 0; i < handlers.Length; i++)
                    {
                        try
                        {
                            handlers[i](this, message);
                        }
                        catch (Exception ex)
                        {
                            this.DispatchExceptionEvent(ex);
                        }
                    }
                }

                if (this._asyncMessageHandlers.TryGetValue(message.GetType(), out var asyncHandlers))
                {
                    var tasks = new List<Task>(asyncHandlers.Length);
                    foreach (var asyncHandler in asyncHandlers)
                    {
                        try
                        {
                            tasks.Add(asyncHandler(this, message));
                        }
                        catch (Exception ex)
                        {
                            this.DispatchExceptionEvent(ex);
                        }
                    }
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
        public event Action<ISonarSocket, ISonarMessage>? MessageReceived;
        public event Func<ISonarSocket, ISonarMessage, Task>? MessageReceivedAsync;
        public event Action<ISonarSocket, string>? TextReceived;
        public event Func<ISonarSocket, string, Task>? TextReceivedAsync;
        public event Action<ISonarSocket, Exception>? Exception;

        public void RegisterMessageHandler<T>(Action<ISonarSocket, T> handler) where T : ISonarMessage
        {
            while (true)
            {
                var originalDictionary = this._messageHandlers;
                var dictionaryCopy = new Dictionary<Type, Action<ISonarSocket, ISonarMessage>[]>(originalDictionary);
                if (!dictionaryCopy.TryGetValue(typeof(T), out var originalArray))
                {
                    originalArray = Array.Empty<Action<ISonarSocket, ISonarMessage>>();
                }
                var arrayCopy = originalArray.Append(Unsafe.As<Action<ISonarSocket, ISonarMessage>>(handler)).ToArray();
                dictionaryCopy[typeof(T)] = arrayCopy;
                if (Interlocked.CompareExchange(ref this._messageHandlers, dictionaryCopy, originalDictionary) == originalDictionary) return;
            }
        }

        public void RegisterMessageHandler<T>(Func<ISonarSocket, T, Task> handler) where T : ISonarMessage
        {
            while (true)
            {
                var originalDictionary = this._asyncMessageHandlers;
                var dictionaryCopy = new Dictionary<Type, Func<ISonarSocket, ISonarMessage, Task>[]>(originalDictionary);
                if (!dictionaryCopy.TryGetValue(typeof(T), out var originalArray))
                {
                    originalArray = Array.Empty<Func<ISonarSocket, ISonarMessage, Task>>();
                }
                var arrayCopy = originalArray.Append(Unsafe.As<Func<ISonarSocket, ISonarMessage, Task>>(handler)).ToArray();
                dictionaryCopy[typeof(T)] = arrayCopy;
                if (Interlocked.CompareExchange(ref this._asyncMessageHandlers, dictionaryCopy, originalDictionary) == originalDictionary) return;
            }
        }

        protected void DispatchExceptionEvent(Exception exception) => this.Exception?.SafeInvoke(this, exception);
        protected void DispatchConnectedEvent() => this.Connected?.Invoke(this);
        protected void DispatchDisconnectedEvent() => this.Disconnected?.Invoke(this);

        public bool RemoveMessageHandler<T>(Action<ISonarSocket, T> handler) where T : ISonarMessage
        {
            while (true)
            {
                var originalDictionary = this._messageHandlers;
                if (!originalDictionary.TryGetValue(typeof(T), out var originalArray)) return false;
                var removeIndex = originalArray.IndexOf(Unsafe.As<Action<ISonarSocket, ISonarMessage>>(handler));
                if (removeIndex == -1) return false;

                var dictionaryCopy = new Dictionary<Type, Action<ISonarSocket, ISonarMessage>[]>(originalDictionary);

                var newLength = originalArray.Length - 1;
                if (newLength == 0)
                {
                    dictionaryCopy.Remove(typeof(T));
                }
                else
                {
                    var arrayCopy = new Action<ISonarSocket, ISonarMessage>[originalArray.Length - 1];
                    for (var i = 0; i < arrayCopy.Length; i++) arrayCopy[i] = originalArray[i >= removeIndex ? i + 1 : i];
                    dictionaryCopy[typeof(T)] = arrayCopy;
                }
                if (Interlocked.CompareExchange(ref this._messageHandlers, dictionaryCopy, originalDictionary) == originalDictionary) return true;
            }
        }

        public bool RemoveMessageHandler<T>(Func<ISonarSocket, T, Task> handler) where T : ISonarMessage
        {
            while (true)
            {
                var originalDictionary = this._asyncMessageHandlers;
                if (!originalDictionary.TryGetValue(typeof(T), out var originalArray)) return false;
                var removeIndex = originalArray.IndexOf(Unsafe.As<Func<ISonarSocket, ISonarMessage, Task>>(handler));
                if (removeIndex == -1) return false;

                var dictionaryCopy = new Dictionary<Type, Func<ISonarSocket, ISonarMessage, Task>[]>(originalDictionary);

                var newLength = originalArray.Length - 1;
                if (newLength == 0)
                {
                    dictionaryCopy.Remove(typeof(T));
                }
                else
                {
                    var arrayCopy = new Func<ISonarSocket, ISonarMessage, Task>[originalArray.Length - 1];
                    for (var i = 0; i < arrayCopy.Length; i++) arrayCopy[i] = originalArray[i >= removeIndex ? i + 1 : i];
                    dictionaryCopy[typeof(T)] = arrayCopy;
                }
                if (Interlocked.CompareExchange(ref this._asyncMessageHandlers, dictionaryCopy, originalDictionary) == originalDictionary) return true;
            }
        }

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
