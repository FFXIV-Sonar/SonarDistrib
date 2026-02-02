using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar.Services
{
    /// <summary>Handles sonar ticks while avoiding overlaps.</summary>
    public sealed partial class SonarTickService : IDisposable
    {
        private readonly Lock _timerLock = new();
        private readonly Timer _timer;
        private ImmutableArray<TickState> _tickStates = [];
        private int _tickInterval;

        private SonarClient Client { get; }

        /// <summary>Tick Interval in milliseconds.</summary>
        public int TickInterval
        {
            get => this._tickInterval;
            internal set
            {
                lock (this._timerLock)
                {
                    if (this._timer.Change(value, value)) this._tickInterval = value;
                }
            }
        }

        /// <summary>Initializes a <see cref="SonarTickService"/>.</summary>
        internal SonarTickService(SonarClient client)
        {
            this._tickInterval = SonarConstants.SonarTick;
            this.Client = client;
            this._timer = new Timer(TickHandler, this, this._tickInterval, this._tickInterval);
        }

        private static void TickHandler(object? tickerObj)
        {
            Debug.Assert(tickerObj is SonarTickService);

            var ticker = Unsafe.As<SonarTickService>(tickerObj);
            foreach (var state in ticker._tickStates)
            {
                if (!Interlocked.CompareExchange(ref state._running, true, false))
                {
                    ref var delayTicksRef = ref state._delayTicks;
                    var delayTicks = Volatile.Read(ref delayTicksRef);
                    if (delayTicks is 0)
                    {
                        _ = TickHandlerCoreAsync(state);
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref delayTicksRef, delayTicks - 1, delayTicks);
                        Volatile.Write(ref state._running, false);
                    }
                }
            }
        }

        private static async Task TickHandlerCoreAsync(TickState state)
        {
            await Task.Yield();

            var service = state.Ticker;
            var handler = state.Handler;
            try
            {
                if (handler is Action<SonarTickService> syncHandler) syncHandler(state.Ticker);
                else if (handler is Func<SonarTickService, Task> asyncHandler) await asyncHandler(state.Ticker).ConfigureAwait(false);
                else
                {
                    service.Client.LogError($"Unable to recognize tick handler: {handler.Method.Name}");
                    Volatile.Write(ref state._delayTicks, 100);
                }
            }
            catch (Exception ex)
            {
                service.Client.LogError(ex, "Exception occurred while running tick handler. Tick Handler will not run for 100 ticks");
                Volatile.Write(ref state._delayTicks, 100);
            }
            finally
            {
                Volatile.Write(ref state._running, false);
            }
        }

        /// <summary>Ticks every Sonar tick.</summary>
        public event Action<SonarTickService>? Tick
        {
            add
            {
                if (value is null) return;
                var state = new TickState(this, value);
                ImmutableInterlocked.Update(ref this._tickStates, (states, state) => states.Add(state), state);
            }
            remove
            {
                if (value is null) return;
                var state = new TickState(this, value);
                ImmutableInterlocked.Update(ref this._tickStates, (states, state) => states.Remove(state), state);
            }
        }

        /// <summary>Ticks every Sonar tick.</summary>
        public event Func<SonarTickService, Task>? AsyncTick
        {
            add
            {
                if (value is null) return;
                var state = new TickState(this, value);
                ImmutableInterlocked.Update(ref this._tickStates, (states, state) => states.Add(state), state);
            }
            remove
            {
                if (value is null) return;
                var state = new TickState(this, value);
                ImmutableInterlocked.Update(ref this._tickStates, (states, state) => states.Remove(state), state);
            }
        }
        public async ValueTask DisposeAsync()
        {
            this._tickStates = [];
            await this._timer.DisposeAsync();
        }

        public void Dispose()
        {
            this._tickStates = [];
            this._timer.Dispose();
        }
    }
}
