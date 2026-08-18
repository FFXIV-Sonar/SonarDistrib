using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using DryIocAttributes;
using Microsoft.Extensions.Hosting;
using Sonar;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin
{
    [ExportMany]
    [SingletonReuse]
    public sealed class SonarFramework : IHostedService
    {
        private readonly Lock _frameLock = new();
        private readonly Lock _timerLock = new();
        private readonly Lock _onceLock = new();
        private readonly Queue<Action<SonarFramework>> _prepareQueue = new();
        private readonly PriorityQueue<Action<SonarFramework>, long> _timerHandlers = new();
        private readonly PriorityQueue<Action<SonarFramework>, long> _frameHandlers = new();
        private readonly Queue<Action<SonarFramework>> _onceHandlers = new();
        private ImmutableArray<Action<SonarFramework>> _updateHandlers = [];
        private ImmutableArray<Action<SonarFramework>> _tickHandlers = [];
        private bool _safe;
        private bool _tick;

        public IFramework Framework { get; }
        private ICondition Condition { get; }
        private SonarClient Client { get; }
        private IPluginLog Logger { get; }

        public SonarFramework(IFramework framework, ICondition condition, SonarClient client, IPluginLog logger)
        {
            this.Framework = framework;
            this.Condition = condition;
            this.Client = client;
            this.Logger = logger;

            this.Logger.Info("Sonar Framework Initialized");
        }

        /// <summary>Last snapshot of <see cref="Environment.TickCount64"/>.</summary>
        public long TickCount { get; private set; }

        /// <summary>Ticks elapsed since previous snapshot (<see cref="TickCount"/>).</summary>
        public long TickDelta { get; private set; }

        /// <summary>Frame counter.</summary>
        public long FrameCount { get; private set; }

        /// <summary>Safe frames.</summary>
        public long SafeFrames { get; private set; }

        /// <summary>Player is in duty.</summary>
        public bool IsDuty { get; private set; }

        public bool IsSafe(uint safeFrames = 2) => this.SafeFrames >= safeFrames;

        /// <summary>Sets <see cref="SafeFrames"/> to <c>0</c> at next framework event.</summary>
        public void Unsafe() => Volatile.Write(ref this._safe, false);

        private void Framework_Update(IFramework framework)
        {
            // Update tick and frame count
            var tickCount = Environment.TickCount64;
            this.TickDelta = tickCount - this.TickCount;
            this.TickCount = tickCount;
            var frameCount = ++this.FrameCount;

            // Check if its safe for certain operations
            var safe = Interlocked.CompareExchange(ref this._safe, true, false) && !this.Condition[ConditionFlag.BetweenAreas51];
            if (safe) this.SafeFrames++;
            else this.SafeFrames = 0;

            // Check if in duty
            this.IsDuty = this.Condition[ConditionFlag.BoundByDuty56];

            // Run normal handlers
            this.Framework_UpdateCore(this._updateHandlers.AsSpan());

            // Run tick handlers
            if (Interlocked.CompareExchange(ref this._tick, false, true))
                this.Framework_UpdateCore(this._tickHandlers.AsSpan());

            // Prepare scheduled timer and frame handlers
            var queue = this._prepareQueue;
            Framework_UpdateCore_Prepare(this._timerHandlers, this._timerLock, tickCount, queue);
            Framework_UpdateCore_Prepare(this._frameHandlers, this._frameLock, frameCount, queue);
            Framework_UpdateCore_Prepare(this._onceHandlers, this._onceLock, queue);

            // Run prepared items from scheduled timer, frame and once handlers
            if (queue.Count is not 0) this.Framework_UpdateCore_RunPrepared(queue);
        }

        private void Framework_UpdateCore(ReadOnlySpan<Action<SonarFramework>> handlers)
        {
            foreach (var handler in handlers)
            {
                try
                {
                    handler(this);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Framework handler exception");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Framework_UpdateCore_Prepare(Queue<Action<SonarFramework>> handlers, Lock handlersLock, Queue<Action<SonarFramework>> queue)
        {
            // ASSERT: Safe to do without locking - this just reads the first item without doing
            // anything else and only acts as a hint to continue with the heavier logic.
            // Uses .TryEnter to avoid latency.
            if (handlers.Count is 0) return;

            // Heavy logic is here
            Framework_UpdateCore_PrepareCore(handlers, handlersLock, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Framework_UpdateCore_Prepare(PriorityQueue<Action<SonarFramework>, long> handlers, Lock handlersLock, long priority, Queue<Action<SonarFramework>> queue)
        {
            // ASSERT: Safe to do without locking - this just reads the first item without doing
            // anything else and only acts as a hint to continue with the heavier logic.
            // Uses .TryEnter to avoid latency.
            if (!handlers.TryPeek(out _, out var handlerPriority) || handlerPriority > priority) return; // O(1)
            
            // Heavy logic is here
            Framework_UpdateCore_PrepareCore(handlers, handlersLock, priority, queue);
        }

        private static void Framework_UpdateCore_PrepareCore(Queue<Action<SonarFramework>> handlers, Lock handlersLock, Queue<Action<SonarFramework>> queue)
        {
            if (!handlersLock.TryEnter()) return;
            try
            {
                while (handlers.TryDequeue(out var handler)) queue.Enqueue(handler);
            }
            finally
            {
                handlersLock.Exit();
            }
        }

        private static void Framework_UpdateCore_PrepareCore(PriorityQueue<Action<SonarFramework>, long> handlers, Lock handlersLock, long priority, Queue<Action<SonarFramework>> queue)
        {
            if (!handlersLock.TryEnter()) return;
            try
            {
                while (handlers.TryPeek(out var handler, out var handlerPriority) && handlerPriority <= priority) // O(1)
                {
                    _ = handlers.Dequeue(); // O(log(N)) ASSERT: It returns the same handler we peeked
                    queue.Enqueue(handler);
                }
            }
            finally
            {
                handlersLock.Exit();
            }
        }

        private void Framework_UpdateCore_RunPrepared(Queue<Action<SonarFramework>> queue)
        {
            while (queue.TryDequeue(out var handler))
            {
                try
                {
                    handler(this);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Framework handler exception");
                }
            }
        }

        // Sonar ticks
        private void Client_Tick(SonarClient source) => Volatile.Write(ref this._tick, true);

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this.Framework.Update += this.Framework_Update;
            this.Client.Tick += this.Client_Tick;
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            this.Framework.Update -= this.Framework_Update;
            this.Client.Tick -= this.Client_Tick;
            return Task.CompletedTask;
        }

        /// <summary>Schedule a <paramref name="handler"/> to run at or after a specified <paramref name="tickCountAt"/> referencing <see cref="TickCount"/>.</summary>
        /// <param name="handler">Handler.</param>
        /// <param name="tickCountAt">Ticks to run at (<see cref="TickCount"/>), equivalent to milliseconds.</param>
        public void ScheduleTimerAt(Action<SonarFramework> handler, long tickCountAt) => ScheduleAtCore(handler, tickCountAt, this._timerHandlers, this._timerLock);

        /// <summary>Schedule a <paramref name="handler"/> to run after a specified number of <paramref name="tickCountAfter"/> relative to <see cref="TickCount"/>.</summary>
        /// <param name="handler">Handler.</param>
        /// <param name="tickCountAfter">Ticks after <see cref="TickCount"/> to run at, equivalent to milliseconds.</param>
        public void ScheduleTimerAfter(Action<SonarFramework> handler, long tickCountAfter) => this.ScheduleTimerAt(handler, this.TickCount + tickCountAfter);

        /// <summary>Schedule a <paramref name="handler"/> to run at or after a specified <paramref name="frameCountAt"/> referencing <see cref="FrameCount"/>.</summary>
        /// <param name="handler">Handler.</param>
        /// <param name="frameCountAt">Frame to run at (<see cref="FrameCount"/>).</param>
        public void ScheduleFrameAt(Action<SonarFramework> handler, long frameCountAt) => ScheduleAtCore(handler, frameCountAt, this._frameHandlers, this._frameLock);

        /// <summary>Schedule a <paramref name="handler"/> to run after a specified number of <paramref name="frameCountAfter"/> relative to <see cref="FrameCount"/>.</summary>
        /// <param name="handler">Handler.</param>
        /// <param name="frameCountAfter">Frames after <see cref="FrameCount"/> to run at.</param>
        public void ScheduleFrameAfter(Action<SonarFramework> handler, long frameCountAfter) => this.ScheduleFrameAt(handler, this.FrameCount + frameCountAfter);

        /// <summary>Scheduling core functionality.</summary>
        private static void ScheduleAtCore(Action<SonarFramework> handler, long priority, PriorityQueue<Action<SonarFramework>, long> handlers, Lock handlersLock)
        {
            lock (handlersLock) handlers.Enqueue(handler, priority); // O(log(N))
        }

        public Task RunAsync(Action action)
        {
            // Run synchrounosly if in framework thread.
            if (this.Framework.IsInFrameworkUpdateThread)
            {
                try
                {
                    action();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            }

            // Run asynchrounosly if not in framework thread.
            var tcs = new TaskCompletionSource();
            this.Once += framework =>
            {
                try
                {
                    action();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };
            return tcs.Task;
        }

        public Task<T> RunAsync<T>(Func<T> action)
        {
            // Run synchrounosly if in framework thread.
            if (this.Framework.IsInFrameworkUpdateThread)
            {
                try
                {
                    var result = action();
                    return Task.FromResult(result);
                }
                catch (Exception ex)
                {
                    return Task.FromException<T>(ex);
                }
            }

            // Run asynchrounosly if not in framework thread.
            var tcs = new TaskCompletionSource<T>();
            this.Once += framework =>
            {
                try
                {
                    var result = action();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };
            return tcs.Task;
        }

        /// <summary>Wait for next framework event.</summary>
        public Task WaitAsync() => this.RunAsync(() => { /* Nothing to do */ });

        public void Run(Action action) => this.RunAsync(action).GetAwaiter().GetResult();

        public T Run<T>(Func<T> action) => this.RunAsync(action).GetAwaiter().GetResult();

        public void Wait() => this.WaitAsync().GetAwaiter().GetResult();

        /// <summary>Triggers once after checks.</summary>
        /// <remarks>Removals are no-op.</remarks>
        public event Action<SonarFramework>? Once
        {
            add
            {
                if (value is not null) lock (this._onceLock) this._onceHandlers.Enqueue(value);
            }
            remove
            {
                /* Nothing to do (this assumes the handler already ran) */
            }
        }

        /// <summary>Triggers every framework update after checks.</summary>
        public event Action<SonarFramework>? Update
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._updateHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._updateHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }

        /// <summary>Triggers every framework update after checks, but only once per Sonar tick (400ms).</summary>
        public event Action<SonarFramework>? Tick
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._tickHandlers, (handlers, handler) => handlers.Add(handler), value);
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref this._tickHandlers, (handlers, handler) => handlers.Remove(handler), value);
            }
        }
    }
}
