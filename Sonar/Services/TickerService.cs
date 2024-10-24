using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Sonar.Utilities;

namespace Sonar.Services
{
    /// <summary>Handles sonar ticks</summary>
    public sealed class TickerService : IDisposable
    {
        private readonly Timer _timer;
        private ImmutableArray<Action<TickerService>> _tickHandlers = [];
        private double _tickInterval;

        private SonarClient Client { get; }

        /// <summary>Tick Interval in milliseconds</summary>
        public double TickInterval
        {
            get => this._tickInterval;
            set
            {
                if (this._timer.Change((int)value, (int)value)) this._tickInterval = (int)value;
            }
        }

        /// <summary>Initializes a TickerService with a specified tick size</summary>
        internal TickerService(SonarClient client)
        {
            this._tickInterval = SonarConstants.SonarTick;
            this.Client = client;
            this._timer = new Timer(this.TickHandler, null, (int)this._tickInterval, (int)this._tickInterval);
        }

        private void TickHandler(object? _)
        {
            foreach (var handler in this._tickHandlers)
            {
                try
                {
                    handler.Invoke(this);
                }
                catch (Exception ex)
                {
                    if (this.Client.LogErrorEnabled) this.Client.LogError(ex, string.Empty);
                }
            }
        }

        /// <summary>Ticks every raw tick</summary>
        public event Action<TickerService>? Tick
        {
            add
            {
                if (value is null) return;
                while (true)
                {
                    var handlers = this._tickHandlers;
                    if (ImmutableInterlocked.InterlockedCompareExchange(ref this._tickHandlers, handlers.Add(value), handlers) == handlers) return;
                }
            }
            remove
            {
                if (value is null) return;
                while (true)
                {
                    var handlers = this._tickHandlers;
                    if (ImmutableInterlocked.InterlockedCompareExchange(ref this._tickHandlers, handlers.Remove(value), handlers) == handlers) return;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            this._tickHandlers = [];
            await this._timer.DisposeAsync();
        }

        public void Dispose()
        {
            this._tickHandlers = [];
            this._timer.Dispose();
        }
    }
}
