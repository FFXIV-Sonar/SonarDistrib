using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SonarUtils.Diagnostics
{
    /// <summary>Holds a pooled <see cref="Stopwatch"/> instance.</summary>
    /// <remarks>Intended to be used in a <see langword="using"/> context.</remarks>
    [SuppressMessage("Performance", "CA1815", Justification = "Not applicable.")]
    public struct StopwatchHolder : IDisposable
    {
        /// <summary><see cref="Stopwatch"/> pool.</summary>
        private static readonly ConcurrentBag<Stopwatch> s_watches = [];

        /// <summary>Pooled <see cref="Stopwatch"/>.</summary>
        private Stopwatch? _watch;

        /// <summary><see cref="Action"/> run on dispose.</summary>
        private Action<StopwatchHolder>? _action;

        /// <summary>Elapsed <see cref="TimeSpan"/> copy populated at <see cref="Dispose"/>.</summary>
        private TimeSpan? _elapsed;

        /// <summary>Initializes a new instance of <see cref="StopwatchHolder"/> with an optional <paramref name="action"/> to call upon <see cref="Dispose"/>.</summary>
        /// <param name="action">Action to call upon <see cref="Dispose"/>.</param>
        public StopwatchHolder(Action<StopwatchHolder>? action)
        {
            if (!s_watches.TryTake(out var watch)) watch = new();
            this._watch = watch;
            this._action = action;

            // Stopwatch objects are pooled, using .Restart() instead of .Start()
            // will perform a full reset while also starting the timer.
            watch.Restart();
        }

        /// <summary>Initializes a new instance of <see cref="StopwatchHolder"/> with an optional <paramref name="action"/> to call upon <see cref="Dispose"/>.</summary>
        public StopwatchHolder() : this(null) { /* Empty */ }

        /// <summary>Whether this <see cref="StopwatchHolder"/> timer is running.</summary>
        public readonly bool IsRunning => this._watch?.IsRunning ?? false;

        /// <summary>Elapsed <see cref="TimeSpan"/> or <see cref="Timeout.InfiniteTimeSpan"/> if faulted.</summary>
        public readonly TimeSpan Elapsed => this._watch?.Elapsed ?? this._elapsed ?? Timeout.InfiniteTimeSpan;

        /// <summary>Elapsed ticks or <c>-1</c> if faulted.</summary>
        /// <remarks><see cref="TimeSpan.Ticks"/>.</remarks>
        public readonly long ElapsedTicks => this._watch?.Elapsed.Ticks ?? this._elapsed?.Ticks ?? -1;

        /// <summary>Elapsed milliseconds or <c>-1</c> if faulted.</summary>
        public readonly long ElapsedMilliseconds => (this._watch?.Elapsed.Ticks ?? this._elapsed?.Ticks) / TimeSpan.TicksPerMillisecond ?? -1;

        /// <summary>Whether this <see cref="StopwatchHolder"/> is disposed or faulted.</summary>
        /// <remarks>More efficient than checking <see cref="IsFaulted"/> and <see cref="IsDisposed"/> individually.</remarks>
        public readonly bool IsDisposedOrFaulted => this._watch is null;

        /// <summary>Whether this <see cref="StopwatchHolder"/> is faulted.</summary>
        /// <remarks><see cref="Fault"/>.</remarks>
        public readonly bool IsFaulted => this._watch is null && this._elapsed is null;

        /// <summary>Whether this <see cref="StopwatchHolder"/> is disposed.</summary>
        /// <remarks><see cref="Dispose"/>.</remarks>
        public readonly bool IsDisposed => this._watch is null && this._elapsed is not null;

        /// <summary>Faults this <see cref="StopwatchHolder"/>, causing <see cref="Dispose"/> do nothing and all elapsed timers be set to <c>-1</c> or <see cref="Timeout.InfiniteTimeSpan"/>.</summary>
        public void Fault()
        {
            var watch = this._watch;
            if (watch is null || !ReferenceEquals(Interlocked.CompareExchange(ref this._watch, null, watch), watch)) return;
            s_watches.Add(watch);

            var action = this._action;
            if (action is null || !ReferenceEquals(Interlocked.CompareExchange(ref this._action, null!, action), action)) return;
        }

        /// <summary>Disposes the watch and executes the associated action.</summary>
        public void Dispose()
        {
            var watch = this._watch;
            if (watch is null || !ReferenceEquals(Interlocked.CompareExchange(ref this._watch, null, watch), watch)) return;
            this._elapsed = watch.Elapsed;
            s_watches.Add(watch);

            var action = this._action;
            if (action is null || !ReferenceEquals(Interlocked.CompareExchange(ref this._action, null!, action), action)) return;
            action(this);
        }

        /// <summary>Returns a <see cref="string"/> representing this <see cref="StopwatchHolder"/> status.</summary>
        public readonly override string ToString()
        {
            var status = this.IsRunning ? "Running" :
                this.IsFaulted ? "Faulted" :
                this.IsDisposed ? "Finished" :
                "Unknown"; // Should never happen

            return $"Elapsed: {this.Elapsed} ({status})";
        }

        /// <summary>Returns the pooled <see cref="Stopwatch"/> directly.</summary>
        /// <returns>Pooled <see cref="Stopwatch"/> instance used by this <see cref="StopwatchHolder"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public readonly Stopwatch? DangerousGetPooledStopwatch() => this._watch;

        /// <summary>Returns the pooled <see cref="Stopwatch"/> directly.</summary>
        /// <param name="removeFromPool">If <see langword="true"/> this <see cref="StopwatchHolder"/> will be faulted and the returned <see cref="Stopwatch"/> will be removed from pool.</param>
        /// <returns>Pooled or removed from pool <see cref="Stopwatch"/> instance used by this <see cref="StopwatchHolder"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public Stopwatch? DangerousGetPooledStopwatch(bool removeFromPool)
        {
            var watch = this._watch;
            if (removeFromPool)
            {
                watch = Interlocked.CompareExchange(ref this._watch, null, watch);
                var action = this._action;
                Interlocked.CompareExchange(ref this._action, null!, action);
            }
            return watch;
        }
    }
}
