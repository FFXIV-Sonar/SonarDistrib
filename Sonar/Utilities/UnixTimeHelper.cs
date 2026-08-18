using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sonar.Utilities
{
    public static class UnixTimeHelper
    {
        private const long UpdateIntervalMs = 1;

        private static SpinLock s_lock = new(false);
        private static long s_lastTicks;
        private static long s_unixNowLong;
        private static double s_unixNow;

        /// <summary>Current time using Unix Epoch (in Milliseconds) as a <see langword="long"/>.</summary>
        public static long UnixNowLong
        {
            get
            {
                EnsureUpdated();
                return Volatile.Read(ref s_unixNowLong);
            }
        }

        /// <summary>Current time using Unix Epoch (in Milliseconds) as a <see langword="double"/>.</summary>
        // TODO: Figure out if its safe to just change this to long instead of having both double and long paths
        public static double UnixNow
        {
            get
            {
                EnsureUpdated();
                return Volatile.Read(ref s_unixNow);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureUpdated()
        {
            var currentTicks = Environment.TickCount;
            var lastTicks = Volatile.Read(ref s_lastTicks);
            var ticksDelta = currentTicks - lastTicks;
            if (ticksDelta is >= UpdateIntervalMs or <= -UpdateIntervalMs) EnsureUpdatedSlow(currentTicks);
        }

        private static void EnsureUpdatedSlow(long currentTicks)
        {
            var lockTaken = false;
            s_lock.Enter(ref lockTaken);
            Debug.Assert(lockTaken);
            try
            {
                var lastTicks = Volatile.Read(ref s_lastTicks);
                var ticksDelta = currentTicks - lastTicks;
                if (ticksDelta is >= UpdateIntervalMs or <= -UpdateIntervalMs)
                {
                    Volatile.Write(ref s_lastTicks, currentTicks);
                    var unixNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    s_unixNowLong = unixNow;
                    s_unixNow = unixNow;
                }
            }
            finally
            {
                s_lock.Exit(false);
            }
        }

        /// <summary>Time synchronization offset relative to server (+ = behind, - = ahead)</summary>
        public static double UnixTimeOffset { get; internal set; }

        /// <summary>Synchronized time using Unix Epoch (in Milliseconds)</summary>
        public static double SyncedUnixNow => UnixNow + UnixTimeOffset;

        /// <summary>Get a Unix Timestamp from a DateTimeOffset object</summary>
        /// <param name="dto">DateTimeOffset object</param>
        /// <returns>Unix timestamp</returns>
        public static double GetUnixTime(DateTimeOffset dto) => dto.ToUnixTimeMilliseconds();

        /// <summary>Get a Unix Timestamp from a DateTime object</summary>
        /// <param name="dt">DateTime object</param>
        /// <returns>Unix timestamp</returns>
        public static double GetUnixTime(DateTime dt) => GetUnixTime(new DateTimeOffset(dt));

        /// <summary>Get a DateTimeOffset object from a Unix timestamp</summary>
        /// <param name="ut">Unix timestamp</param>
        /// <returns>DateTimeOffset</returns>
        public static DateTimeOffset GetDateTimeOffset(double ut) => DateTimeOffset.FromUnixTimeMilliseconds((long)ut);

        /// <summary>Get a DateTime object from a Unix timestamp</summary>
        /// <param name="ut">Unix timestamp</param>
        /// <returns>DateTimeOffset</returns>
        public static DateTime GetDateTime(double ut) => GetDateTimeOffset(ut).UtcDateTime;
    }
}
