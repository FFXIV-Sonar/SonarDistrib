using System.Threading;
using System;
using static Sonar.Utilities.UnixTimeHelper;
using System.Diagnostics;

namespace Sonar.Threading
{
    // License for AdaptiveSpinLock: MIT, feel free to use (NOTE: This is experimental and I never made actual use of this, initial testing were slower than lock and SpinLock)

    /// <summary>
    /// <para>Adaptive spin lock, which transitions between the following stages:</para>
    /// <para>1. Pure busy wait, which checks the lock state every loop. Timeout is not checked during this loop.</para>
    /// <para>2. <see cref="Thread.SpinWait(int)"/> loop with an increasing number of iterations per iteration.</para>
    /// <para>3. <see cref="Thread.Yield()"/> loop, giving other threads a chance to run.</para>
    /// <para>4. <see cref="Thread.Sleep(int)"/> loop with 0 millisecond iterations, giving other threads a chance to run.</para>
    /// <para>5. <see cref="Thread.Sleep(int)"/> loop with 1 millisecond iterations, freeing up CPU resources</para>
    /// <para>6. <see cref="Thread.Sleep(int)"/> loop with N milliseconds iterations, freeing up CPU resources further as time goes.</para>
    /// <para><see cref="AdaptiveSpinLock"/> should be used in cases where the locks are fine grained and wait times are short. This class is an alternative to <see cref="SpinLock"/>.</para>
    /// </summary>
    /// <remarks>
    /// This lock is slower than <see cref="SpinLock"/>...
    /// </remarks>
    [DebuggerDisplay("IsHeld = {IsHeld} | ContentionCount = {ContentionCount}")]
    public struct AdaptiveSpinLock
    {
        private const int BusyWait = 16;
        private const int SpinWaitCount = 16;
        private const int YieldWaitCount = 16;
        private const int Sleep0WaitCount = 16;
        private const int Sleep1WaitCount = 16;
        private const int SleepNWaitIncreaseInterval = 16;
        private const int SleepNWaitMax = 5000;

        private volatile int _locked;
        private bool _trackOwner;
        private long _contentionCount;

        private int GetThreadIdInternal() => this._trackOwner ? Environment.CurrentManagedThreadId : 1;

        public AdaptiveSpinLock(bool trackOwner = false, bool initialLocked = false)
        {
            this._trackOwner = trackOwner;
            this._locked = initialLocked ? trackOwner ? Environment.CurrentManagedThreadId : 1 : 0;
            this._contentionCount = 0;
        }

        /// <summary>
        /// If taken and tracking is enabled: Thread ID
        /// If taken and tracking is disabled: 1
        /// If not taken: 0
        /// </summary>
        public int OwnerThreadId => this._locked;
        public bool IsHeld => this._locked != 0;
        public long ContentionCount => this._contentionCount;

        public bool OwnerTracking
        {
            get => this._trackOwner;
            set
            {
                this.Enter();
                this._trackOwner = value;
                this.DangerousExit();
            }
        }

        
        public void Enter()
        {
            var threadId = this.GetThreadIdInternal();
            if (this.TryEnterInternal(threadId)) return;
            this.ContinueEnterInternal(threadId);
        }

        public bool TryEnter()
        {
            var threadId = this.GetThreadIdInternal();
            return this.TryEnterInternal(threadId);
        }

        public bool TryEnter(double timeout)
        {
            var threadId = this.GetThreadIdInternal();
            if (this.TryEnterInternal(threadId)) return true;
            if (timeout == 0) return false;
            if (timeout < 0)
            {
                this.ContinueEnterInternal(threadId);
                return true;
            }
            return this.ContinueEnterInternal(threadId, timeout);
        }

        public bool TryEnter(TimeSpan timeout) => this.TryEnter(timeout.TotalMilliseconds);

        private bool TryEnterInternal(int threadId) => Interlocked.CompareExchange(ref this._locked, threadId, 0) == 0;

        private void ContinueEnterInternal(int threadId)
        {
            if (this._trackOwner && this._locked == threadId) throw new LockRecursionException();
            Interlocked.Increment(ref this._contentionCount);

            // Loop 1: BusyWait
            for (var loopCount = 0; loopCount < BusyWait; loopCount++)
            {
                if (this.TryEnterInternal(threadId)) return;
            }

            // Loop 2: SpinWait (less frequent Interlocked calls)
            for (var loopCount = 0; loopCount < SpinWaitCount; loopCount++)
            {
                Thread.SpinWait(loopCount);
                if (this.TryEnterInternal(threadId)) return;
            }

            // Loop 3: YieldWait
            for (var loopCount = 0; loopCount < YieldWaitCount; loopCount++)
            {
                Thread.Yield();
                if (this.TryEnterInternal(threadId)) return;
            }

            // Loop 4: Sleep0Wait
            for (var loopCount = 0; loopCount < Sleep0WaitCount; loopCount++)
            {
                Thread.Sleep(0);
                if (this.TryEnterInternal(threadId)) return;
            }

            // Loop 5: Sleep1Wait (Slow)
            for (var loopCount = 0; loopCount < Sleep1WaitCount; loopCount++)
            {
                Thread.Sleep(1);
                if (this.TryEnterInternal(threadId)) return;
            }

            // Loop 6: SleepNWait (Very Slow)
            for (var loopCount = SleepNWaitIncreaseInterval; ; loopCount++)
            {
                loopCount = Math.Min(SleepNWaitIncreaseInterval * SleepNWaitMax, loopCount);
                Thread.Sleep(loopCount / SleepNWaitIncreaseInterval);
                if (this.TryEnterInternal(threadId)) return;
            }
        }

        private bool ContinueEnterInternal(int threadId, double timeout)
        {
            var startTime = UnixNow;
            if (this._trackOwner && this._locked == threadId) throw new LockRecursionException();
            Interlocked.Increment(ref this._contentionCount);

            // Loop 1: BusyWait
            for (var loopCount = 0; loopCount < BusyWait; loopCount++)
            {
                if (this.TryEnterInternal(threadId)) return true;
            }
            if (UnixNow - startTime > timeout) return false; // Intentionally outside

            // Loop 2: SpinWait (less frequent Interlocked calls)
            for (var loopCount = 0; loopCount < SpinWaitCount; loopCount++)
            {
                Thread.SpinWait(loopCount);
                if (this.TryEnterInternal(threadId)) return true;
                if (UnixNow - startTime > timeout) return false;
            }

            // Loop 3: YieldWait
            for (var loopCount = 0; loopCount < YieldWaitCount; loopCount++)
            {
                Thread.Yield();
                if (this.TryEnterInternal(threadId)) return true;
                if (UnixNow - startTime > timeout) return false;
            }

            // Loop 4: Sleep0Wait
            for (var loopCount = 0; loopCount < Sleep0WaitCount; loopCount++)
            {
                Thread.Sleep(0);
                if (this.TryEnterInternal(threadId)) return true;
                if (UnixNow - startTime > timeout) return false;
            }

            // Loop 5: Sleep1Wait (Slow)
            for (var loopCount = 0; loopCount < Sleep1WaitCount; loopCount++)
            {
                Thread.Sleep(1);
                if (this.TryEnterInternal(threadId)) return true;
                if (UnixNow - startTime > timeout) return false;
            }

            // Loop 6: SleepNWait (Very Slow)
            for (var loopCount = SleepNWaitIncreaseInterval; ; loopCount++)
            {
                loopCount = Math.Min(SleepNWaitIncreaseInterval * SleepNWaitMax, loopCount);
                Thread.Sleep(loopCount / SleepNWaitIncreaseInterval);
                if (this.TryEnterInternal(threadId)) return true;
                if (UnixNow - startTime > timeout) return false;
            }
        }

        public void Exit()
        {
            //Debug.Assert(this._locked == this.GetThreadIdInternal());
            this.DangerousExit();
        }

        public void EnterAndExit()
        {
            this.Enter();
            this.Exit();
        }

        public bool TryEnterAndExit(double timeout)
        {
            if (this.TryEnter(timeout))
            {
                this.Exit();
                return true;
            }
            return false;
        }

        public bool TryEnterAndExit(TimeSpan timeout) => this.TryEnterAndExit(timeout.TotalMilliseconds);

        public void ExitAndEnter()
        {
            this.Exit();
            this.Enter();
        }

        public bool TryExitAndEnter()
        {
            this.Exit();
            return this.TryEnter();
        }

        public bool TryExitAndEnter(double timeout)
        {
            this.Exit();
            return this.TryEnter(timeout);
        }

        public bool TryExitAndEnter(TimeSpan timeout) => this.TryExitAndEnter(timeout.TotalMilliseconds);

        /// <summary>
        /// Lock directly without a care in the world
        /// </summary>
        public void DangerousDirectLock() => this._locked = this.GetThreadIdInternal();

        /// <summary>
        /// Locks directly without a care in the world using a thread ID. Implicictly enables tracking.
        /// </summary>
        public void DangerousDirectLock(int threadId)
        {
            this._trackOwner = true;
            this._locked = threadId;
        }

        /// <summary>
        /// Unlocks directly without a care in the world
        /// </summary>
        public void DangerousExit()
        {
            if (this._locked != 0) this._locked = 0;
        }
    }
}
