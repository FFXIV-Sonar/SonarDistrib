using DryIoc.FastExpressionCompiler.LightExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar.Threading
{
    // License for SonarAsyncManualResetEvent: MIT, feel free to use (originally named AsyncManualResetEvent but had to rename to avoid conflict with Nito.AsyncEx version)

    public sealed class SonarAsyncManualResetEvent
    {
        private volatile TaskCompletionSource _tcs = new();

        public bool IsSet => this._tcs.Task.IsCompleted;

        public SonarAsyncManualResetEvent() { }
        public SonarAsyncManualResetEvent(bool set)
        {
            if (set) this.Set();
        }

        /// <summary>Set this <see cref="SonarAsyncManualResetEvent"/> into a signaled state</summary>
        public void Set()
        {
            this._tcs.TrySetResult();
        }

        /// <summary>Quick <see cref="Set"/> and <see cref="Reset"/></summary>
        /// <remarks><see cref="IsSet"/> will be <see cref="false"/>. If already signaled, result is non-signaled.</remarks>
        public void SetAndReset()
        {
            var tcs = this._tcs;
            Interlocked.CompareExchange(ref this._tcs, new(), tcs);
            tcs.TrySetResult();
        }

        /// <summary>ReSet this <see cref="SonarAsyncManualResetEvent"/> into a non-signaled state</summary>
        public void Reset()
        {
            var tcs = this._tcs;
            if (!tcs.Task.IsCompleted) return;
            Interlocked.CompareExchange(ref this._tcs, new(), tcs);
        }

        public Task WaitAsync() => this._tcs.Task;
        public Task WaitAsync(TimeSpan timeout) => this._tcs.Task.WaitAsync(timeout);
        public Task WaitAsync(CancellationToken token) => this._tcs.Task.WaitAsync(token);
        public Task WaitAsync(TimeSpan timeout, CancellationToken token) => this._tcs.Task.WaitAsync(timeout, token);

        public void Wait() => this._tcs.Task.Wait();
        public void Wait(int millisecondsTimeout) => this._tcs.Task.Wait(millisecondsTimeout);
        public void Wait(TimeSpan timeout) => this._tcs.Task.Wait(timeout);
        public void Wait(CancellationToken token) => this._tcs.Task.Wait(token);
        public void Wait(int millisecondsTimeout, CancellationToken token) => this._tcs.Task.Wait(millisecondsTimeout, token);
        public void Wait(TimeSpan timeout, CancellationToken token) => this._tcs.Task.Wait(timeout, token);
    }
}
