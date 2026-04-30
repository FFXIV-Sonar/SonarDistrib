using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SonarUtils.Diagnostics
{
    /// <summary>Holds a pooled <see cref="Stopwatch"/> instance.</summary>
    /// <remarks>Intended to be used in a <see langword="using"/> context.</remarks>
    public struct StopwatchHolder : IDisposable
    {
        private static readonly ConcurrentBag<Stopwatch> s_watches = [];

        private Stopwatch? _watch;
        private Action<Stopwatch> _action; // Only null if _watch is null

        /// <summary>Initializes a new instance of <see cref="StopwatchHolder"/> with an <paramref name="action"/> to call upon <see cref="Dispose"/>.</summary>
        /// <param name="action">Action to call upon <see cref="Dispose"/>.</param>
        public StopwatchHolder(Action<Stopwatch> action)
        {
            if (!s_watches.TryTake(out var watch)) watch = new();
            this._watch = watch;
            this._action = action;

            // Stopwatch objects are pooled, using .Restart() instead of .Start()
            // will perform a full reset while also starting the timer.
            watch.Restart();
        }

        /// <summary>Faults this <see cref="StopwatchHolder"/>, causing <see cref="Dispose"/> do nothing.</summary>
        public void Fault()
        {
            var watch = this._watch;
            if (watch is null) return;
            this._watch = null;
            this._action = null!;
            s_watches.Add(watch);
        }

        /// <summary>Disposes the watch and executes the associated action.</summary>
        public void Dispose()
        {
            var watch = this._watch;
            if (watch is null) return;
            this._watch = null;

            watch.Stop();
            try
            {
                var action = this._action;
                this._action = null!;
                action(watch);
            }
            finally
            {
                s_watches.Add(watch);
            }
        }
    }
}
