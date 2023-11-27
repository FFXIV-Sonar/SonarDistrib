using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xoshiro.PRNG64;

namespace SonarUtils.Random
{
    /// <summary>Extend <see cref="RandomNumberGenerator"/> functionality</summary>
    public static class RngExtensions
    {
        private static readonly ThreadLocal<byte[]> s_buffer = new(() => new byte[sizeof(long)]);

        public static int Next(this RandomNumberGenerator random)
        {
            var buffer = s_buffer.Value!;
            int result;
            do
            {
                random.GetBytes(buffer, 0, sizeof(int));
                result = BitConverter.ToInt32(buffer, 0) & 0x7FFFFFFF;
            }
            while (result == int.MaxValue);
            return result;
        }

        public static int Next(this RandomNumberGenerator random, int maxValue)
        {
            var buffer = s_buffer.Value!;

            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l171
            if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "'maxValue' must be greater than zero.");
            if (maxValue <= 1) return 0;

            // Debiasing
            var r = int.MaxValue / maxValue;
            var tooLarge = r * maxValue;
            do
            {
                random.GetBytes(buffer, 0, sizeof(int));
                r = BitConverter.ToInt32(buffer, 0) & 0x7FFFFFFF;
            }
            while (r >= tooLarge);
            return r % maxValue;
        }

        public static int Next(this RandomNumberGenerator random, int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal maxValue");
            if (minValue == maxValue) maxValue++;
            return (int)(random.NextU((uint)(maxValue - minValue)) + minValue);
        }

        public static long Next64(this RandomNumberGenerator random)
        {
            var buffer = s_buffer.Value!;
            random.GetBytes(buffer, 0, sizeof(long));
            return BitConverter.ToInt64(buffer, 0) & 0x7FFFFFFFFFFFFFFFL;
        }

        public static long Next64(this RandomNumberGenerator random, long maxValue)
        {
            var buffer = s_buffer.Value!;

            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l171
            if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "'maxValue' must be greater than zero.");
            if (maxValue <= 1) return 0;

            // Debiasing
            var r = long.MaxValue / maxValue;
            var tooLarge = r * maxValue;
            do
            {
                random.GetBytes(buffer, 0, sizeof(long));
                r = BitConverter.ToInt64(buffer, 0) & 0x7FFFFFFFFFFFFFFFL;
            }
            while (r >= tooLarge);
            return r % maxValue;
        }

        public static long Next64(this RandomNumberGenerator random, long minValue, long maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal maxValue");
            if (minValue == maxValue) maxValue++;
            return (long)(random.Next64U((ulong)(maxValue - minValue)) + (ulong)minValue);
        }

        public static uint NextU(this RandomNumberGenerator random)
        {
            var buffer = s_buffer.Value!;
            random.GetBytes(buffer, 0, sizeof(uint));
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static uint NextU(this RandomNumberGenerator random, uint maxValue)
        {
            var buffer = s_buffer.Value!;
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l115
            if (maxValue <= 1) return 0;

            // Debiasing
            var r = uint.MaxValue / maxValue;
            var tooLarge = r * maxValue;
            do
            {
                random.GetBytes(buffer, 0, sizeof(uint));
                r = BitConverter.ToUInt32(buffer, 0);
            } while (r >= tooLarge);
            return r % maxValue;
        }

        public static uint NextU(this RandomNumberGenerator random, uint minValue, uint maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal maxValue");
            if (minValue == maxValue) maxValue++;
            return random.NextU(maxValue - minValue) + minValue;
        }

        public static ulong Next64U(this RandomNumberGenerator random)
        {
            var buffer = s_buffer.Value!;
            random.GetBytes(buffer, 0, sizeof(ulong));
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ulong Next64U(this RandomNumberGenerator random, ulong maxValue)
        {
            var buffer = s_buffer.Value!;
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l115
            if (maxValue <= 1) return 0;

            // Debiasing
            var r = ulong.MaxValue / maxValue;
            var tooLarge = r * maxValue;
            do
            {
                random.GetBytes(buffer, 0, sizeof(ulong));
                r = BitConverter.ToUInt64(buffer, 0);
            } while (r >= tooLarge);
            return r % maxValue;
        }

        public static ulong Next64U(this RandomNumberGenerator random, ulong minValue, ulong maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal maxValue");
            if (minValue == maxValue) maxValue++;
            return random.Next64U(maxValue - minValue) + minValue;
        }

        public static int NextInt(this RandomNumberGenerator random)
        {
            return (int)random.NextU();
        }

        public static int NextIntNonNegative(this RandomNumberGenerator random)
        {
            return (int)random.NextU() & 0x7FFFFFFF;
        }

        public static long NextInt64(this RandomNumberGenerator random)
        {
            return (long)random.Next64U();
        }

        public static long NextInt64NonNegative(this RandomNumberGenerator random)
        {
            return (long)random.Next64U() & 0x7FFFFFFFFFFFFFFFL;
        }

        public static long NextInt64NonNegative(this XoShiRo256starstar random) // Might as well
        {
            return (long)random.Next64U() & 0x7FFFFFFFFFFFFFFFL;
        }

        public static void NextBytes(this RandomNumberGenerator random, byte[] bytes)
        {
            random.GetBytes(bytes);
        }

        public static void NextBytes(this RandomNumberGenerator random, Span<byte> bytes)
        {
            random.GetBytes(bytes);
        }

        public static double NextDouble(this RandomNumberGenerator random)
        {
            var buffer = s_buffer.Value!;

            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/Xoshiro64Base.cs#l130
            double r;
            do
            {
                random.GetBytes(buffer, 0, sizeof(double));
                r = (BitConverter.ToUInt64(buffer, 0) >> 11) * (1.0 / ((ulong)1 << 53));
            } while ((r >= 1.0) || (r < 0.0));
            return r;
        }

        public static double NextFloat(this RandomNumberGenerator random)
        {
            var buffer = s_buffer.Value!;

            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/Xoshiro64Base.cs#l163
            float r;
            do
            {
                random.GetBytes(buffer, 0, sizeof(float));
                r = (BitConverter.ToUInt32(buffer, 0) >> 40) * ((float)1.0 / ((uint)1 << 24));
            } while ((r >= 1.0f) || (r < 0.0f));
            return r;
        }
    }
}
