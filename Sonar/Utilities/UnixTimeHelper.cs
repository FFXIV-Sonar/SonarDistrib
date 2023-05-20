using System;
using System.Runtime.CompilerServices;

namespace Sonar.Utilities
{
    public static class UnixTimeHelper
    {
        private static int _lastTickCount;
        private static double _lastUnixNow;

        /// <summary>
        /// Current time using Unix Epoch (in Milliseconds)
        /// </summary>
        public static double UnixNow
        {
            get
            {
                var currentTicks = Environment.TickCount;
                if (currentTicks != _lastTickCount)
                {
                    _lastTickCount = currentTicks;
                    _lastUnixNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                return _lastUnixNow;
            }
        }

        /// <summary>
        /// Time synchronization offset relative to server (+ = behind, - = ahead)
        /// </summary>
        public static double UnixTimeOffset { get; internal set; } = 0;

        /// <summary>
        /// Synchronized time using Unix Epoch (in Milliseconds)
        /// </summary>
        public static double SyncedUnixNow => UnixNow + UnixTimeOffset;

        /// <summary>
        /// Get a Unix Timestamp from a DateTimeOffset object
        /// </summary>
        /// <param name="dto">DateTimeOffset object</param>
        /// <returns>Unix timestamp</returns>
        public static double GetUnixTime(DateTimeOffset dto) => dto.ToUnixTimeMilliseconds();

        /// <summary>
        /// Get a Unix Timestamp from a DateTime object
        /// </summary>
        /// <param name="dt">DateTime object</param>
        /// <returns>Unix timestamp</returns>
        public static double GetUnixTime(DateTime dt) => GetUnixTime(new DateTimeOffset(dt));

        /// <summary>
        /// Get a DateTimeOffset object from a Unix timestamp
        /// </summary>
        /// <param name="ut">Unix timestamp</param>
        /// <returns>DateTimeOffset</returns>
        public static DateTimeOffset GetDateTimeOffset(double ut) => DateTimeOffset.FromUnixTimeMilliseconds((long)ut);

        /// <summary>
        /// Get a DateTime object from a Unix timestamp
        /// </summary>
        /// <param name="ut">Unix timestamp</param>
        /// <returns>DateTimeOffset</returns>
        public static DateTime GetDateTime(double ut) => GetDateTimeOffset(ut).UtcDateTime;
    }
}
