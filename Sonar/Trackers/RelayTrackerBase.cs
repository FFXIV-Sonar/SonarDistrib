using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar.Trackers
{
    /// <summary>Base relay tracker class containing common code between different types of relay trackers</summary>
    public abstract class RelayTrackerBase<T> : IRelayTrackerBase<T> where T : Relay
    {
        private static readonly RelayType s_relayType = RelayUtils.GetRelayType<T>();
        private ImmutableArray<Action<RelayState<T>>> _foundHandlers = ImmutableArray.Create<Action<RelayState<T>>>();
        private ImmutableArray<Action<RelayState<T>>> _updatedHandlers = ImmutableArray.Create<Action<RelayState<T>>>();
        private ImmutableArray<Action<RelayState<T>>> _deadHandlers = ImmutableArray.Create<Action<RelayState<T>>>();
        private ImmutableArray<Action<RelayState<T>>> _allHandlers = ImmutableArray.Create<Action<RelayState<T>>>();
        private ImmutableArray<Action<Exception>> _exceptionHandlers = ImmutableArray.Create<Action<Exception>>();

        public RelayType RelayType => s_relayType;

        protected void DispatchEvents(RelayState<T> state, bool isFound)
        {
            if (this._foundHandlers.IsEmpty && this._updatedHandlers.IsEmpty && this._deadHandlers.IsEmpty && this._allHandlers.IsEmpty) return;
            if (state.IsAliveInternal())
            {
                if (isFound) this.DispatchEventsCore(this._foundHandlers, state);
                else this.DispatchEventsCore(this._updatedHandlers, state);
            }
            else
            {
                this.DispatchEventsCore(this._deadHandlers, state);
            }
            this.DispatchEventsCore(this._allHandlers, state);
        }

        private void DispatchEventsCore(IEnumerable<Action<RelayState<T>>> actions, RelayState<T> state)
        {
            foreach (var action in actions)
            {
                try
                {
                    action(state);
                }
                catch (Exception ex)
                {
                    foreach (var handler in this._exceptionHandlers)
                    {
                        try
                        {
                            handler(ex);
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

        /// <inheritdoc/>
        public event Action<RelayState<T>>? Found
        {
            add => ImmutableInterlocked.Update(ref this._foundHandlers, static (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._foundHandlers, static (handlers, value) => handlers.Remove(value!), value);
        }

        /// <inheritdoc/>
        public event Action<RelayState<T>>? Updated
        {
            add => ImmutableInterlocked.Update(ref this._updatedHandlers, static (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._updatedHandlers, static (handlers, value) => handlers.Remove(value!), value);
        }

        /// <inheritdoc/>
        public event Action<RelayState<T>>? Dead
        {
            add => ImmutableInterlocked.Update(ref this._deadHandlers, static (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._deadHandlers, static (handlers, value) => handlers.Remove(value!), value);
        }

        /// <inheritdoc/>
        public event Action<RelayState<T>>? All
        {
            add => ImmutableInterlocked.Update(ref this._allHandlers, static (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._allHandlers, static (handlers, value) => handlers.Remove(value!), value);
        }

        /// <inheritdoc/>
        event Action<RelayState>? IRelayTrackerBase.Found
        {
            add => this.Found += value;
            remove => this.Found -= value;
        }

        /// <inheritdoc/>
        event Action<RelayState>? IRelayTrackerBase.Updated
        {
            add => this.Updated += value;
            remove => this.Updated -= value;
        }

        /// <inheritdoc/>
        event Action<RelayState>? IRelayTrackerBase.Dead
        {
            add => this.Dead += value;
            remove => this.Dead -= value;
        }

        /// <inheritdoc/>
        event Action<RelayState>? IRelayTrackerBase.All
        {
            add => this.All += value;
            remove => this.All -= value;
        }

        public event Action<Exception>? Exception
        {
            add => ImmutableInterlocked.Update(ref this._exceptionHandlers, static (handlers, value) => handlers.Add(value!), value);
            remove => ImmutableInterlocked.Update(ref this._exceptionHandlers, static (handlers, value) => handlers.Remove(value!), value);
        }
    }
}
