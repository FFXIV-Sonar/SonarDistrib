using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Trackers
{
    /// <summary>Handles, receives and relay hunt tracking information.</summary>
    public interface IRelayTracker<T> : IRelayTracker where T : Relay
    {
        /// <summary>Feeds a relay into this tracker</summary>
        /// <returns>Successful or sent to server</returns>
        public bool FeedRelay(T relay);

        /// <summary>Feeds relays into this tracker</summary>
        public void FeedRelays(IEnumerable<T> relays);

        /// <summary>Relay is found</summary>
        public event Action<RelayState<T>>? Found;

        /// <summary>Relay is updated</summary>
        public event Action<RelayState<T>>? Updated;

        /// <summary>Relay is dead</summary>
        public event Action<RelayState<T>>? Dead;

        /// <summary>Found, Updated or Dead</summary>
        public event Action<RelayState<T>>? All;
    }

    /// <summary>Handles, receives and relay hunt tracking information.</summary>
    /// <remarks>Please cast to the correct <see cref="IRelayTracker{T}"/> before use.</remarks>
    public interface IRelayTracker
    {
        /// <summary>Feeds a relay into this tracker</summary>
        /// <returns>Successful or sent to server</returns>
        public bool FeedRelay(Relay relay);

        /// <summary>Feeds relays into this tracker</summary>
        public void FeedRelays(IEnumerable<Relay> relays);
    }
}
