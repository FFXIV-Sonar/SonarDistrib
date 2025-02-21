using Microsoft.Extensions.Options;
using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Trackers
{
    public sealed partial class RelayTrackerData<T> where T : Relay
    {
        private static readonly RelayType s_relayType = RelayUtils.GetRelayType<T>();
        private ImmutableArray<Action<RelayTrackerData<T>, RelayState<T>>> _addedHandlers = ImmutableArray.Create<Action<RelayTrackerData<T>, RelayState<T>>>();
        private ImmutableArray<Action<RelayTrackerData<T>, RelayState<T>>> _removedHandlers = ImmutableArray.Create<Action<RelayTrackerData<T>, RelayState<T>>>();
        private ImmutableArray<Action<RelayTrackerData<T>>> _clearedHandlers = ImmutableArray.Create<Action<RelayTrackerData<T>>>();
        private ImmutableArray<Action<RelayTrackerData<T>, Exception>> _exceptionHandlers = ImmutableArray.Create<Action<RelayTrackerData<T>, Exception>>();

        public RelayType RelayType => s_relayType;


        public event Action<IRelayTrackerData<T>, RelayState<T>>? Added
        {
            add => ImmutableInterlocked.Update(ref this._addedHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._addedHandlers, (handlers, value) => handlers.Remove(value!), value);
        }
        public event Action<IRelayTrackerData<T>, RelayState<T>>? Removed
        {
            add => ImmutableInterlocked.Update(ref this._removedHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._removedHandlers, (handlers, value) => handlers.Remove(value!), value);
        }
        public event Action<IRelayTrackerData<T>>? Cleared
        {
            add => ImmutableInterlocked.Update(ref this._clearedHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._clearedHandlers, (handlers, value) => handlers.Remove(value!), value);
        }
        public event Action<IRelayTrackerData<T>, Exception>? Exception
        {
            add => ImmutableInterlocked.Update(ref this._exceptionHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._exceptionHandlers, (handlers, value) => handlers.Remove(value!), value);
        }

        event Action<IRelayTrackerData, RelayState>? IRelayTrackerData.Added
        {
            add => ImmutableInterlocked.Update(ref this._addedHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._addedHandlers, (handlers, value) => handlers.Remove(value!), value);
        }
        event Action<IRelayTrackerData, RelayState>? IRelayTrackerData.Removed
        {
            add => ImmutableInterlocked.Update(ref this._removedHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._removedHandlers, (handlers, value) => handlers.Remove(value!), value);
        }
        event Action<IRelayTrackerData>? IRelayTrackerData.Cleared
        {
            add => ImmutableInterlocked.Update(ref this._clearedHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._clearedHandlers, (handlers, value) => handlers.Remove(value!), value);
        }
        event Action<IRelayTrackerData, Exception>? IRelayTrackerData.Exception
        {
            add => ImmutableInterlocked.Update(ref this._exceptionHandlers, (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._exceptionHandlers, (handlers, value) => handlers.Remove(value!), value);
        }

        private void DispatchEvent(ImmutableArray<Action<RelayTrackerData<T>, RelayState<T>>> actions, RelayState<T> state)
        {
            foreach (var action in actions)
            {
                try
                {
                    action(this, state);
                }
                catch (Exception ex)
                {
                    foreach (var handler in this._exceptionHandlers)
                    {
                        try
                        {
                            handler(this, ex);
                        }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Unnecessary assignment of a value
                        catch (Exception ex2)
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore CS0168 // Unnecessary assignment of a value
                        {
                            /* Swallow: Nothing to do */
                        }
                    }
                }
            }
        }

        private void DispatchCleredEvent()
        {
            var actions = this._clearedHandlers;
            foreach (var action in actions)
            {
                try
                {
                    action(this);
                }
                catch (Exception ex)
                {
                    foreach (var handler in this._exceptionHandlers)
                    {
                        try
                        {
                            handler(this, ex);
                        }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Unnecessary assignment of a value
                        catch (Exception ex2)
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore CS0168 // Unnecessary assignment of a value
                        {
                            /* Swallow: Nothing to do */
                        }
                    }
                }
            }
        }
    }
}
