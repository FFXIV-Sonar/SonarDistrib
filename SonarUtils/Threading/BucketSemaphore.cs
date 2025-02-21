using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils.Threading
{
    /// <summary>Semaphone using a token bucket</summary>
    [Obsolete("Use AG.Threading.BucketSemaphore (TODO: Update AG.Threading nuget)")]
    public sealed class BucketSemaphore : IDisposable
    {
        private long _tokens;
        private int _disposed;

        private sealed class BucketSemaphoreItem
        {
            public BucketSemaphoreItem(BucketSemaphore semaphore, long cost)
            {
                this.Cost = cost;
                this.Semaphore = semaphore;
            }

            public BucketSemaphore Semaphore { get; }
            public TaskCompletionSource<bool> Tcs { get; } = new();
            public long Cost { get; }
        }

        private readonly PriorityQueue<BucketSemaphoreItem, long> _queue = new();

        /// <summary>Token count.</summary>
        public long Tokens
        {
            get => Volatile.Read(ref this._tokens);
            set
            {
                Volatile.Write(ref this._tokens, value);
                this.ProcessItems(value);
            }
        }

        /// <summary>Add (or remove) tokens via a <see cref="Interlocked.Add(ref long, long)"/> operation.</summary>
        /// <param name="tokens">Tokens to add.</param>
        /// <returns>Tokens after addition.</returns>
        public long AtomicAddTokens(long tokens) => Interlocked.Add(ref this._tokens, tokens);

        /// <summary>Modify token count via a <see cref="Interlocked.CompareExchange(ref long, long, long)"/> operation.</summary>
        /// <remarks>May attempt multiple times.</remarks>
        /// <param name="tokens">New tokens value.</param>
        /// <returns>Previous tokens value.</returns>
        public long AtomicExchangeTokens(long tokens)
        {
            while (true)
            {
                var currentTokens = Interlocked.Read(ref this._tokens);
                if (Interlocked.CompareExchange(ref this._tokens, tokens, currentTokens) == currentTokens)
                {
                    this.ProcessItems(tokens);
                    return currentTokens;
                }
            }
        }

        /// <summary>Modify token count via a <see cref="Interlocked.CompareExchange(ref long, long, long)"/> operation.</summary>
        /// <remarks>May attempt multiple times. Returning the token count passed as arg will cancel the exchange and return the current tokens.</remarks>
        /// <param name="updater">Updater functions getting the current token count as its arg and returning the new token count. This may run multiple times.</param>
        /// <returns>Previous tokens value.</returns>
        public long AtomicExchangeTokens(Func<long, long> updater)
        {
            while (true)
            {
                var currentTokens = Interlocked.Read(ref this._tokens);
                var tokens = updater(currentTokens);
                if (currentTokens == tokens || Interlocked.CompareExchange(ref this._tokens, tokens, currentTokens) == currentTokens)
                {
                    this.ProcessItems(tokens);
                    return currentTokens;
                }
            }
        }

        /// <summary>Waiter count.</summary>
        public long Waiters => this._queue.Count;

        /// <summary>Construct a BucketSemaphore with a specified amount of tokens.</summary>
        /// <param name="tokens">Tokens</param>
        public BucketSemaphore(long tokens)
        {
            this._tokens = tokens;
        }

        /// <summary>Construct a SemaphoneBucket with the maximum amount of tokens.</summary>
        public BucketSemaphore() : this(long.MaxValue) { }

        public Task<bool> TryWaitAsync(long cost = 1, CancellationToken cancellationToken = default)
        {
            if (Volatile.Read(ref this._disposed) != 0) return Task.FromResult(false);

            // Reads available tokens and attempt returning atomically
            var tokens = Interlocked.Read(ref this._tokens);
            if (tokens >= cost && Interlocked.CompareExchange(ref this._tokens, tokens - cost, tokens) == tokens) return Task.FromResult(true);
            if (cancellationToken.IsCancellationRequested) return Task.FromResult(false);

            // Creates a bucket item and task chain
            var bucket = new BucketSemaphoreItem(this, cost);
            var task = bucket.Tcs.Task;
            if (cancellationToken.CanBeCanceled) task = task.WaitAsync(cancellationToken);
            task = task.ContinueWith((task, state) =>
            {
                var bucket = Unsafe.As<BucketSemaphoreItem>(state)!;
                var success = task.IsCompletedSuccessfully && task.Result;
                if (success) Interlocked.Add(ref bucket.Semaphore._tokens, -bucket.Cost);
                return success;
            }, bucket, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

            // Enqueue, process queued items and return the task
            lock (this._queue) this._queue.Enqueue(bucket, cost);
            this.ProcessItems();
            return task;
        }

        public async Task WaitAsync(long cost)
        {
            if (await this.TryWaitAsync(cost)) return;
            ObjectDisposedException.ThrowIf(Volatile.Read(ref this._disposed) != 0, this);
            throw new OperationCanceledException(); // Should never happen
        }

        public bool TryWait(long cost = 1, CancellationToken cancellationToken = default)
        {
            var task = this.TryWaitAsync(cost, cancellationToken);
            if (task.IsCompletedSuccessfully) return task.Result;
            return task.GetAwaiter().GetResult();
        }

        public void Wait(long cost = 1)
        {
            var task = this.WaitAsync(cost);
            if (task.IsCompletedSuccessfully) return;
            task.GetAwaiter().GetResult();
        }

        public void Release(long cost = 1)
        {
            var tokens = Interlocked.Add(ref this._tokens, cost);
            this.ProcessItems(tokens);
        }

        /// <summary>Dispose this <see cref="BucketSemaphore"/>.</summary>
        /// <remarks>Current waiters will still be able to proceed, but no new waiters can be added.</remarks>
        public void Dispose()
        {
            Volatile.Write(ref this._disposed, 1);
        }

        /// <summary>Utility method to wait for all waiters.</summary>
        public async Task WaitAllWaitersAsync(CancellationToken cancellationToken = default)
        {
            var yieldType = 0;
            var spinwait = new SpinWait();
            while (this._queue.Count > 0)
            {
                if (spinwait.NextSpinWillYield)
                {
                    switch (yieldType)
                    {
                        case 0:
                            await Task.Yield();
                            break;
                        case 1:
                            await Task.Delay(1, cancellationToken);
                            break;
                    }
                    yieldType ^= 1;
                    spinwait.Reset();
                }
                else spinwait.SpinOnce();
            }
        }

        /// <summary>Utility method to wait for all waiters.</summary>
        public void WaitAllWaiters(CancellationToken cancellationToken = default)
        {
            var spinwait = new SpinWait();
            while (this._queue.Count > 0)
            {
                if (spinwait.NextSpinWillYield) cancellationToken.ThrowIfCancellationRequested();
                spinwait.SpinOnce();
            }
        }

        /// <inheritdoc/>
        public override string ToString() => $"{this._tokens} Tokens | {this.Waiters} Waiters";

        private void ProcessItems() => this.ProcessItems(Volatile.Read(ref this._tokens));

        private void ProcessItems(long tokens)
        {
            if (this._queue.TryPeek(out _, out var cost) && cost <= tokens) this.ProcessItemsCore();
        }

        private void ProcessItemsCore()
        {
            lock (this._queue)
            {
                while (this._queue.TryPeek(out var item, out var cost) && cost <= Volatile.Read(ref this._tokens))
                {
                    this._queue.TryDequeue(out var _, out _);
                    item.Tcs.TrySetResult(true);
                }
            }
        }
    }
}
