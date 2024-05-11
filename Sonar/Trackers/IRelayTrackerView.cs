using System;
using Sonar.Relays;

namespace Sonar.Trackers
{
    public interface IRelayTrackerView<T> : IRelayTracker<T>, IRelayTrackerView where T : Relay
    {
        /* Empty */
    }

    public interface IRelayTrackerView : IRelayTracker, IDisposable
    {
        /* Empty */
    }
}