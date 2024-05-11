using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Trackers
{
    public interface IRelayTrackerBase<T> : IRelayTrackerBase where T : Relay
    {
        /// <summary>Relay is found</summary>
        public new event Action<RelayState<T>>? Found;

        /// <summary>Relay is updated</summary>
        public new event Action<RelayState<T>>? Updated;

        /// <summary>Relay is dead</summary>
        public new event Action<RelayState<T>>? Dead;

        /// <summary>Found, Updated or Dead</summary>
        public new event Action<RelayState<T>>? All;
    }

    public interface IRelayTrackerBase
    {
        /// <summary>Relay type</summary>
        public RelayType RelayType { get; }
        
        /// <summary>Relay is found</summary>
        public event Action<RelayState>? Found;

        /// <summary>Relay is updated</summary>
        public event Action<RelayState>? Updated;

        /// <summary>Relay is dead</summary>
        public event Action<RelayState>? Dead;

        /// <summary>Found, Updated or Dead</summary>
        public event Action<RelayState>? All;

        /// <summary>Dispatched on handler exceptions</summary>
        /// <remarks>Exceptions thrown in this handler will be swallowed</remarks>
        public event Action<Exception>? Exception;
    }
}
