using System.Threading;

namespace SonarUtils
{
    /// <summary>
    /// Additional interlocked functionality
    /// </summary>
    public static class InterlockedUtils
    {
        /// <summary>
        /// Change values into the lowest of itself or min atomically
        /// </summary>
        public static void Min(ref int value, int min)
        {
            while (true)
            {
                var original = value;
                if (original <= min || Interlocked.CompareExchange(ref value, min, original) == original) break;
            }
        }

        /// <summary>
        /// Change values into the lowest of itself or min atomically
        /// </summary>
        public static void Min(ref uint value, uint min)
        {
            while (true)
            {
                var original = value;
                if (original <= min || Interlocked.CompareExchange(ref value, min, original) == original) break;
            }
        }

        /// <summary>
        /// Change values into the highest of itself or max atomically
        /// </summary>
        public static void Max(ref int value, int max)
        {
            while (true)
            {
                var original = value;
                if (original >= max || Interlocked.CompareExchange(ref value, max, original) == original) break;
            }
        }

        /// <summary>
        /// Change values into the highest of itself or max atomically
        /// </summary>
        public static void Max(ref uint value, uint max)
        {
            while (true)
            {
                var original = value;
                if (original >= max || Interlocked.CompareExchange(ref value, max, original) == original) break;
            }
        }
    }
}
