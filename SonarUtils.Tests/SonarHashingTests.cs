using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xoshiro.PRNG64;

namespace SonarUtils.Tests
{
    public static class SonarHashingTests
    {
        [Fact]
        public static void Sha256_ConsistencyCheck_Span()
        {
            var bytes = "Bye Moon!, Hello, Phaenna!"u8;
            var expected = SHA256.HashData(bytes);
            var actual = SonarHashing.Sha256(bytes);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Sha256_ConsistencyCheck_Stream()
        {
            var stream = new MemoryStream("Bye Moon!, Hello, Phaenna!"u8.ToArray());

            stream.Seek(0, SeekOrigin.Begin);
            var expected = SHA256.HashData(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var actual = SonarHashing.Sha256(stream);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public static async Task Sha256_ConsistencyCheck_AsyncStream()
        {
            var stream = new MemoryStream("Bye Moon!, Hello, Phaenna!"u8.ToArray());

            stream.Seek(0, SeekOrigin.Begin);
            var expected = await SHA256.HashDataAsync(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var actual = await SonarHashing.Sha256Async(stream);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(LengthsAndSeedsSource))]
        public static void Sha256_RandomTest_Span(int length, int seed)
        {
            var bytes = GetInputBytes(length, seed);
            var expected = SHA256.HashData(bytes);
            var actual = SonarHashing.Sha256(bytes);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(LengthsAndSeedsSource))]
        public static void Sha256_RandomTest_Stream(int length, int seed)
        {
            var stream = new MemoryStream(GetInputBytes(length, seed));

            stream.Seek(0, SeekOrigin.Begin);
            var expected = SHA256.HashData(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var actual = SonarHashing.Sha256(stream);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(LengthsAndSeedsSource))]
        public static async Task Sha256_RandomTest_AsyncStream(int length, int seed)
        {
            var stream = new MemoryStream(GetInputBytes(length, seed));

            stream.Seek(0, SeekOrigin.Begin);
            var expected = await SHA256.HashDataAsync(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var actual = await SonarHashing.Sha256Async(stream);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void Sha256_OutputTooShort_Span()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                var bytes = "What are you expecting to happen?"u8;
                var output = new byte[7]; // Digest size is 32 bytes... please fit into 7 bytes!
                Assert.InRange(output.Length, 0, SonarHashing.Sha256DigestSize - 1);
                SonarHashing.Sha256(bytes, output);
            });
            Assert.Equal(GetLengthArgumentExceptionMessage(SonarHashing.Sha256DigestSize), exception.Message);
        }

        [Fact]
        public static void Sha256_OutputTooShort_Stream()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                var stream = new MemoryStream("What are you expecting to happen?"u8.ToArray());
                var output = new byte[7]; // Digest size is 32 bytes... please fit into 7 bytes!
                Assert.InRange(output.Length, 0, SonarHashing.Sha256DigestSize - 1);
                SonarHashing.Sha256(stream, output);
            });
            Assert.Equal(GetLengthArgumentExceptionMessage(SonarHashing.Sha256DigestSize), exception.Message);
        }

        [Fact]
        public static async Task Sha256_OutputTooShort_AsyncStream()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var stream = new MemoryStream("What are you expecting to happen?"u8.ToArray());
                var output = new byte[7]; // Digest size is 32 bytes... please fit into 7 bytes!
                Assert.InRange(output.Length, 0, SonarHashing.Sha256DigestSize - 1);
                await SonarHashing.Sha256Async(stream, output);
            });
            Assert.Equal(GetLengthArgumentExceptionMessage(SonarHashing.Sha256DigestSize), exception.Message);
        }

        public static TheoryData<int, int> LengthsAndSeedsSource()
        {
            var data = new TheoryData<int, int>();

            // Part 0: Random source
            const int MagicSeed = 42;
            var random = new XoShiRo256starstar(MagicSeed);

            // Part 1: Increasing length
            const int Part1MaxLength = 256;
            for (var length = 0; length < Part1MaxLength; length++) data.Add(length, MagicSeed);

            // Part 2: Random blocks
            const int Part2Attempts = 256;
            const int Part2MaxBytes = 1048576;
            for (var attempt = 0; attempt < Part2Attempts; attempt++) data.Add(random.Next(Part2MaxBytes), random.Next());

            return data;
        }

        private static byte[] GetInputBytes(int length, int seed)
        {
            var random = new XoShiRo256starstar(seed);
            var bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }

        private static string GetLengthArgumentExceptionMessage(int length)
        {
            var methodInfo = typeof(SonarHashing).GetMethod(nameof(GetLengthArgumentExceptionMessage), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(methodInfo);
            var func = methodInfo.CreateDelegate<Func<int, string>>();
            Assert.NotNull(func);
            return func(length);
        }
    }
}
