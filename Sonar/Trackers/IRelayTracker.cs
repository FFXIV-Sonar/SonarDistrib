using Sonar.Relays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Trackers
{
    /// <summary>Handles, receives and relay hunt tracking information.</summary>
    public interface IRelayTracker<T> : IRelayTrackerBase<T>, IRelayTracker where T : Relay
    {
        /// <summary>Relay tracker data source</summary>
        public new RelayTrackerData<T> Data { get; }

        /// <summary>Feeds a relay into this tracker</summary>
        /// <returns>Successful or sent to server</returns>
        public bool FeedRelay(T relay);

        /// <summary>Feeds a relay into this tracker</summary>
        /// <returns>Successful or sent to server</returns>
        internal bool FeedRelayInternal(T relay);

        /// <summary>Feeds relays into this tracker</summary>
        public void FeedRelays(IEnumerable<T> relays);

        /// <summary>Create a view of this tracker based on <paramref name="predicate"/> and <paramref name="index"/>, and optionally perform its own <paramref name="indexing"/></summary>
        public IRelayTrackerView<T> CreateView(Predicate<RelayState<T>>? predicate = null, string index = "all", bool indexing = false);
    }

    /// <summary>Handles, receives and relay hunt tracking information.</summary>
    /// <remarks>Please cast to the correct <see cref="IRelayTracker{T}"/> before use.</remarks>
    public interface IRelayTracker : IRelayTrackerBase
    {
        /// <summary>Relay tracker data source</summary>
        public IRelayTrackerData Data { get; }

        /// <summary>Associated Sonar Client</summary>
        public SonarClient Client { get; }
        
        /// <summary>Feeds a relay into this tracker</summary>
        /// <returns>Successful or sent to server</returns>
        public bool FeedRelay(Relay relay);

        /// <summary>Feeds a relay into this tracker</summary>
        /// <returns>Successful or sent to server</returns>
        internal bool FeedRelayInternal(Relay relay);

        /// <summary>Feeds relays into this tracker</summary>
        public void FeedRelays(IEnumerable<Relay> relays);

        /// <summary>Create a view of this tracker based on <paramref name="predicate"/> and <paramref name="index"/>, and optionally perform its own <paramref name="indexing"/></summary>
        public IRelayTrackerView CreateView(Predicate<RelayState>? predicate = null, string index = "all", bool indexing = false);
    }

    public static class IRelayTrackerExtensions
    {
        public static IRelayTracker<T> CreateView<T>(this IRelayTracker<T> tracker, Predicate<RelayState<T>>? predicate = null, string index = "all", bool indexing = false) where T : Relay
        {
            return new RelayTrackerView<T>(tracker, predicate, index, indexing);
        }
    }
}
