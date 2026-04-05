using DryIocAttributes;
using Microsoft.Extensions.Logging;
using Sonar.Relays;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin.Events
{
    [ExportEx]
    [SingletonReuse]
    public sealed class SonarEventManager
    {
        private ImmutableArray<object> _handlers;

        private ILogger Logger { get; }
        public SonarEventManager(ILogger<SonarEventManager> logger)
        {
            this.Logger = logger;
        }

        public event Action<SonarEventManager, EventRelay>? Event
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._handlers, static (handlers, handler) => handlers.Add(handler), (object)value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._handlers, static (handlers, handler) => handlers.Remove(handler), (object)value);
            }
        }

        public event Func<SonarEventManager, EventRelay, Task>? AsyncEvent
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._handlers, static (handlers, handler) => handlers.Add(handler), (object)value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._handlers, static (handlers, handler) => handlers.Remove(handler), (object)value);
            }
        }

        public void DispatchEvent(EventRelay relay)
        {
            foreach (var handler in this._handlers.AsSpan())
            {
                var task = handler switch
                {
                    Action<SonarEventManager, EventRelay> action => Task.Factory.StartNew(() => action(this, relay), CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default),
                    Func<SonarEventManager, EventRelay, Task> asyncAction => Task.Factory.StartNew(() => asyncAction(this, relay), CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default),
                    _ => Task.CompletedTask // ASSERT: Never happens
                };

                task.ContinueWith(task =>
                {
                    if (task.IsFaulted) this.Logger.LogError(task.Exception, "Event relay handler exception");
                });
            }
        }
    }
}
