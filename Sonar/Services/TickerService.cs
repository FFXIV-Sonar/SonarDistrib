using System;
using System.Threading;
using System.Threading.Tasks;
using Sonar.Utilities;

namespace Sonar.Services
{
    /// <summary>
    /// Handles sonar ticks
    /// </summary>
    public sealed class TickerService : IDisposable
    {
        private readonly Timer Timer;

        private double tickInterval;
        public double TickInterval
        {
            get => this.tickInterval;
            set
            {
                this.Timer.Change((int)value, (int)value);
                this.tickInterval = (int)value;
            }
        }

        private SonarClient Client { get; }

        /// <summary>
        /// Initializes a TickerService with a specified tick size
        /// </summary>
        internal TickerService(SonarClient client)
        {
            this.tickInterval = SonarConstants.SonarTick;
            this.Client = client;
            this.Timer = new Timer(this.TickHandler, null, (int)this.tickInterval, (int)this.tickInterval);
        }

        private void TickHandler(object? _)
        {
            try
            {
                this.Tick?.Invoke(this);
            }
            catch (Exception ex)
            {
                if (this.Client.LogErrorEnabled) this.Client.LogError(ex, string.Empty);
            }
        }

        #region Event Handlers
        /// <summary>
        /// Ticks every raw tick
        /// </summary>
        public event TickerTickHandler? Tick;
        #endregion

        #region IDisposable Pattern
        private int disposed;
        public bool IsDisposed => this.disposed == 1;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1) return;
            await this.Timer.DisposeAsync();
        }
        public void Dispose() => this.DisposeAsync().AsTask().GetAwaiter().GetResult();
        #endregion
    }

    public delegate void TickerTickHandler(TickerService ticker);
}
