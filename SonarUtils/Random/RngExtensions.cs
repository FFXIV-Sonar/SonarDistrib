using System;
using System.Security.Cryptography;
using Xoshiro.PRNG64;

namespace SonarUtils.Random
{
    /// <summary>Extend <see cref="RandomNumberGenerator"/> functionality</summary>
    public static class RngExtensions
    {
        public static unsafe int Next(this RandomNumberGenerator random)
        {
            var result = 0;
            var buffer = new Span<byte>(&result, sizeof(int));
            do
            {
                random.GetBytes(buffer);
            }
            while (result == int.MaxValue);
            return result & 0x7FFFFFFF;
        }

        public static unsafe int Next(this RandomNumberGenerator random, int maxValue)
        {
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l171
            if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "'maxValue' must be greater than zero.");
            if (maxValue <= 1) return 0;

            // Debiasing
            var result = int.MaxValue / maxValue;
            var tooLarge = result * maxValue;
            var buffer = new Span<byte>(&result, sizeof(int));
            do
            {
                random.GetBytes(buffer);
                result &= 0x7FFFFFFF;
            }
            while (result >= tooLarge);
            return result % maxValue;
        }

        public static int Next(this RandomNumberGenerator random, int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal maxValue");
            if (minValue == maxValue) maxValue++;
            return (int)(random.NextU((uint)(maxValue - minValue)) + minValue);
        }

        public static unsafe long Next64(this RandomNumberGenerator random)
        {
            var result = 0L;
            var buffer = new Span<byte>(&result, sizeof(long));
            random.GetBytes(buffer);
            return result & 0x7FFFFFFFFFFFFFFFL;
        }

        public static unsafe long Next64(this RandomNumberGenerator random, long maxValue)
        {
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l171
            if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "'maxValue' must be greater than zero.");
            if (maxValue <= 1) return 0;

            // Debiasing
            var result = long.MaxValue / maxValue;
            var tooLarge = result * maxValue;
            var buffer = new Span<byte>(&result, sizeof(long));
            do
            {
                random.GetBytes(buffer);
                result &= 0x7FFFFFFFFFFFFFFF;
            }
            while (result >= tooLarge);
            return result % maxValue;
        }

        public static long Next64(this RandomNumberGenerator random, long minValue, long maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal maxValue");
            if (minValue == maxValue) maxValue++;
            return (long)(random.Next64U((ulong)(maxValue - minValue)) + (ulong)minValue);
        }

        public static unsafe uint NextU(this RandomNumberGenerator random)
        {
            var result = 0u;
            var buffer = new Span<byte>(&result, sizeof(uint));
            random.GetBytes(buffer);
            return result;
        }

        public static unsafe uint NextU(this RandomNumberGenerator random, uint maxValue)
        {
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l115
            if (maxValue <= 1) return 0;

            // Debiasing
            var result = uint.MaxValue / maxValue;
            var tooLarge = result * maxValue;
            var buffer = new Span<byte>(&result, sizeof(uint));
            do
            {
                random.GetBytes(buffer);
            } while (result >= tooLarge);
            return result % maxValue;
        }

        public static uint NextU(this RandomNumberGenerator random, uint minValue, uint maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal maxValue");
            if (minValue == maxValue) maxValue++;
            return random.NextU(maxValue - minValue) + minValue;
        }

        public static unsafe ulong Next64U(this RandomNumberGenerator random)
        {
            var result = 0UL;
            var buffer = new Span<byte>(&result, sizeof(ulong));
            random.GetBytes(buffer);
            return result;
        }

        public static unsafe ulong Next64U(this RandomNumberGenerator random, ulong maxValue)
        {
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/XoshiroBase.cs#l115
            if (maxValue <= 1) return 0;

            // Debiasing
            var result = ulong.MaxValue / maxValue;
            var tooLarge = result * maxValue;
            var buffer = new Span<byte>(&result, sizeof(ulong));
            do
            {
                random.GetBytes(buffer);
            } while (result >= tooLarge);
            return result % maxValue;
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

        public static byte[] GetBytes(this RandomNumberGenerator random, int length)
        {
            var bytes = GC.AllocateUninitializedArray<byte>(length);
            random.GetBytes(bytes);
            return bytes;
        }

        public static unsafe double NextDouble(this RandomNumberGenerator random)
        {
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/Xoshiro64Base.cs#l130
            var result = 0.0;
            var buffer = new Span<byte>(&result, sizeof(double));
            do
            {
                random.GetBytes(buffer);
                result = (BitConverter.ToUInt64(buffer) >> 11) * (1.0 / ((ulong)1 << 53));
            } while ((result >= 1.0) || (result < 0.0));
            return result;
        }

        public static unsafe double NextFloat(this RandomNumberGenerator random)
        {
            // Adapted from: https://sourceforge.net/p/xoshiroprng-net/code/ci/default/tree/XoshiroPRNG.Net/Xoshiro64Base.cs#l163
            var result = 0.0f;
            var buffer = new Span<byte>(&result, sizeof(float));
            do
            {
                random.GetBytes(buffer);
                result = (BitConverter.ToUInt32(buffer) >> 8) * ((float)1.0 / ((uint)1 << 24));
            } while ((result >= 1.0f) || (result < 0.0f));
            return result;
        }
    }
}
