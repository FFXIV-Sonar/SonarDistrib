using SonarUtils.Random;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xoshiro.PRNG64;

namespace SonarUtils.Tests
{
    public sealed class RngTests
    {
        public const int LoopCount = 65536;

        [Fact(DisplayName = "GetThreadRandom()")]
        public void GetThreadRandom()
        {
            var random = RandomUtils.GetThreadRandom();
            Assert.NotNull(random);
            Assert.Same(random, RandomUtils.GetThreadRandom());
        }

        [Fact(DisplayName = "GetThreadCryptoRandom()")]
        public void GetThreadCryptoRandom()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            Assert.NotNull(random);
            Assert.Same(random, RandomUtils.GetThreadCryptoRandom());
        }

        [Fact(DisplayName = "CreateRandom()")]
        public void CreateRandom()
        {
            var random = RandomUtils.CreateRandom();
            Assert.NotNull(random);
            Assert.NotSame(random, RandomUtils.CreateRandom());
        }

        [Fact(DisplayName = "CreateCryptoRandom()")]
        public void CreateCryptoRandom()
        {
            var random = RandomUtils.CreateCryptoRandom();
            Assert.NotNull(random);
            //Assert.NotSame(random, RandomUtils.CreateCryptoRandom()); // Its apparently a singleton but I'll pretend they're newly created
        }

        [Fact(DisplayName = "Next() returns values between 0 and int.MaxValue")]
        public void Next_1()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next();
                Assert.True(value is >= 0 and <= int.MaxValue, $"Value must be between 0 and {int.MaxValue}: {value}");
            }
        }

        [Fact(DisplayName = "Next() can return different values")]
        public void Next_2()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next();
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "Next(maxValue) can return values between 0 and maxValue")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(int.MaxValue)]
        public void Next_3(int maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next(maxValue);
                Assert.True(value >= 0 && value <= maxValue, $"Value must be between 0 and {maxValue}: {value}");
            }
        }

        [Theory(DisplayName = "Next(maxValue) can return different values")]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(int.MaxValue)]
        public void Next_4(int maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next(maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next(maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "Next(maxValue) with maxValue of 0 and 1 always return 0")]
        [InlineData(0)]
        [InlineData(1)]
        public void Next_5(int maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next(maxValue);
                Assert.True(result == 0, $"Value is not zero: {result}");
            }
        }

        [Theory(DisplayName = "Next(maxValue) with negative maxValue throws ArgumentOutOfRangeException")]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-41245249)]
        public void Next_6(int maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            Assert.Throws<ArgumentOutOfRangeException>(() => random.Next(maxValue));
        }

        [Theory(DisplayName = "Next(minValue, maxValue) must return value within [minValue..maxValue)")]
        [InlineData(2, 10)]
        [InlineData(-2, 2)]
        [InlineData(-15, -10)]
        [InlineData(int.MinValue, int.MaxValue)]
        public void Next_7(int minValue, int maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next(minValue, maxValue);
                Assert.True(value >= minValue && value < maxValue, $"Value must be within [{minValue}..{maxValue}): {value}");
            }
        }

        [Theory(DisplayName = "Next(minValue, maxValue) can return different values")]
        [InlineData(2, 10)]
        [InlineData(-2, 2)]
        [InlineData(-15, -10)]
        [InlineData(int.MinValue, int.MaxValue)]
        public void Next_8(int minValue, int maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next(minValue, maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next(minValue, maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Fact(DisplayName = "NextU() can return different values")]
        public void NextU_1()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.NextU();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.NextU();
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "NextU(maxValue) can return values between 0 and maxValue")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(int.MaxValue)]
        public void NextU_2(uint maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextU(maxValue);
                Assert.True(value >= 0 && value <= maxValue, $"Value must be between 0 and {maxValue}: {value}");
            }
        }

        [Theory(DisplayName = "NextU(maxValue) can return different values")]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(int.MaxValue)]
        public void NextU_3(uint maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.NextU(maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.NextU(maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "NextU(maxValue) with maxValue of 0 and 1 always return 0")]
        [InlineData(0)]
        [InlineData(1)]
        public void NextU_4(uint maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.NextU(maxValue);
                Assert.True(result == 0, $"Value is not zero: {result}");
            }
        }

        [Theory(DisplayName = "NextU(minValue, maxValue) must return value within [minValue..maxValue)")]
        [InlineData(2, 10)]
        [InlineData(100, 400)]
        [InlineData(0, 50)]
        [InlineData(uint.MinValue, uint.MaxValue)]
        public void NextU_7(uint minValue, uint maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextU(minValue, maxValue);
                Assert.True(value >= minValue && value < maxValue, $"Value must be within [{minValue}..{maxValue}): {value}");
            }
        }

        [Theory(DisplayName = "NextU(minValue, maxValue) can return different values")]
        [InlineData(2, 10)]
        [InlineData(100, 400)]
        [InlineData(0, 50)]
        [InlineData(uint.MinValue, uint.MaxValue)]
        public void NextU_8(uint minValue, uint maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.NextU(minValue, maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.NextU(minValue, maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Fact(DisplayName = "Next64() returns values between 0 and long.MaxValue")]
        public void Next64_1()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next64();
                Assert.True(value is >= 0 and <= long.MaxValue, $"Value must be between 0 and {long.MaxValue}: {value}");
            }
        }

        [Fact(DisplayName = "Next64() can return different values")]
        public void Next64_2()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next64();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64();
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "Next64(maxValue) can return values between 0 and maxValue")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(long.MaxValue)]
        public void Next64_3(long maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next64(maxValue);
                Assert.True(value >= 0 && value <= maxValue, $"Value must be between 0 and {maxValue}: {value}");
            }
        }

        [Theory(DisplayName = "Next64(maxValue) can return different values")]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(long.MaxValue)]
        public void Next64_4(long maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next64(maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64(maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "Next64(maxValue) with maxValue of 0 and 1 always return 0")]
        [InlineData(0)]
        [InlineData(1)]
        public void Next64_5(long maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64(maxValue);
                Assert.True(result == 0, $"Value is not zero: {result}");
            }
        }

        [Theory(DisplayName = "Next64(maxValue) with negative maxValue throws ArgumentOutOfRangeException")]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-41245249)]
        public void Next64_6(long maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            Assert.Throws<ArgumentOutOfRangeException>(() => random.Next64(maxValue));
        }

        [Theory(DisplayName = "Next64(minValue, maxValue) must return value within [minValue..maxValue)")]
        [InlineData(2, 10)]
        [InlineData(-2, 2)]
        [InlineData(-15, -10)]
        [InlineData(long.MinValue, long.MaxValue)]
        public void Next64_7(long minValue, long maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next64(minValue, maxValue);
                Assert.True(value >= minValue && value < maxValue, $"Value must be within [{minValue}..{maxValue}): {value}");
            }
        }

        [Theory(DisplayName = "Next64(minValue, maxValue) can return different values")]
        [InlineData(2, 10)]
        [InlineData(-2, 2)]
        [InlineData(-15, -10)]
        [InlineData(long.MinValue, long.MaxValue)]
        public void Next64_8(long minValue, long maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next64(minValue, maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64(minValue, maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Fact(DisplayName = "Next64U() can return different values")]
        public void Next64U_1()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next64U();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64U();
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "Next64U(maxValue) can return values between 0 and maxValue")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(long.MaxValue)]
        public void Next64U_2(ulong maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next64U(maxValue);
                Assert.True(value >= 0 && value <= maxValue, $"Value must be between 0 and {maxValue}: {value}");
            }
        }

        [Theory(DisplayName = "Next64U(maxValue) can return different values")]
        [InlineData(2)]
        [InlineData(1337)]
        [InlineData(20231122)]
        [InlineData(long.MaxValue)]
        public void Next64U_3(ulong maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next64U(maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64U(maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Theory(DisplayName = "Next64U(maxValue) with maxValue of 0 and 1 always return 0")]
        [InlineData(0)]
        [InlineData(1)]
        public void Next64U_4(ulong maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64U(maxValue);
                Assert.True(result == 0, $"Value is not zero: {result}");
            }
        }

        [Theory(DisplayName = "Next64U(minValue, maxValue) must return value within [minValue..maxValue)")]
        [InlineData(2, 10)]
        [InlineData(100, 400)]
        [InlineData(0, 50)]
        [InlineData(ulong.MinValue, ulong.MaxValue)]
        public void Next64U_7(ulong minValue, ulong maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.Next64U(minValue, maxValue);
                Assert.True(value >= minValue && value < maxValue, $"Value must be within [{minValue}..{maxValue}): {value}");
            }
        }

        [Theory(DisplayName = "Next64U(minValue, maxValue) can return different values")]
        [InlineData(2, 10)]
        [InlineData(100, 400)]
        [InlineData(0, 50)]
        [InlineData(ulong.MinValue, ulong.MaxValue)]
        public void Next64U_8(ulong minValue, ulong maxValue)
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var value = random.Next64U(minValue, maxValue);
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var result = random.Next64U(minValue, maxValue);
                if (result != value) return;
            }
            Assert.Fail($"Value was never different than {value}");
        }

        [Fact(DisplayName = "NextInt() can return positive and negative values")]
        public void NextInt()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var pos = false;
            var neg = false;
            var zero = false; // NOTE: This is not checked
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextInt();
                pos |= value > 0;
                neg |= value < 0;
                zero |= value == 0;
                if (pos && neg) return;
            }
            var types = new List<string>();
            if (pos) types.Add("positive");
            if (neg) types.Add("negative");
            if (zero) types.Add("zero");
            Assert.Fail($"NextInt() retrned ${string.Join(", ", types)}");
        }

        [Fact(DisplayName = "NextInt64() can return positive and negative values")]
        public void NextInt64()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var pos = false;
            var neg = false;
            var zero = false; // NOTE: This is not checked
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextInt64();
                pos |= value > 0;
                neg |= value < 0;
                zero |= value == 0;
                if (pos && neg) return;
            }
            var types = new List<string>();
            if (pos) types.Add("positive");
            if (neg) types.Add("negative");
            if (zero) types.Add("zero");
            Assert.Fail($"NextInt64() retrned ${string.Join(", ", types)}");
        }

        [Fact(DisplayName = "NextInt() can only return positive values")]
        public void NextIntNonNegative()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextIntNonNegative();
                if (value < 0) Assert.Fail($"NextIntNonNegative() retrned ${value}");
            }
        }

        [Fact(DisplayName = "NextInt64() can only return positive values")]
        public void NextInt64NonNegative()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextInt64NonNegative();
                if (value < 0) Assert.Fail($"NextIntNonNegative() retrned ${value}");
            }
        }

        [Fact(DisplayName = "NextFloat() tests")]
        public void NextFloat()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var buckets = new int[256];
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextFloat();
                if (value < 0 || value >= 1) Assert.Fail($"NextFloat() returned value out of range: {value}");
                buckets[(int)Math.Floor(value * 256)]++;
            }

            var min = (int)(LoopCount * (1.0 / 2.0) / 256);
            var max = (int)(LoopCount * (2.0 / 1.0) / 256);
            foreach (var count in buckets)
            {
                if (count < min || count > max)
                {
                    Assert.Fail($"NextFloat() uniformity test failed");
                }
            }

        }

        [Fact(DisplayName = "NextDouble() tests")]
        public void NextDouble()
        {
            var random = RandomUtils.GetThreadCryptoRandom();
            var buckets = new int[256];
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var value = random.NextDouble();
                if (value < 0 || value >= 1) Assert.Fail($"NextDouble() returned value out of range: {value}");
                buckets[(int)Math.Floor(value * 256)]++;
            }

            var min = (int)(LoopCount * (1.0 / 2.0) / 256);
            var max = (int)(LoopCount * (2.0 / 1.0) / 256);
            foreach (var count in buckets)
            {
                if (count < min || count > max)
                {
                    Assert.Fail($"NextDouble() uniformity test failed");
                }
            }

        }
    }
}
