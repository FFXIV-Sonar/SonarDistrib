using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xoshiro.PRNG64;

namespace SonarUtils.Tests
{
    public sealed class BouncySha256Tests
    {
        [Fact]
        public static async Task ConsistencyCheck()
        {
            var bytes = await File.ReadAllBytesAsync(typeof(BouncySha256).Assembly.Location);
            var expected = SHA256.HashData(bytes);
            var actual = BouncySha256.HashData(bytes);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(RandomTestSource))]
        public static void RandomTest(int length, int seed)
        {
            var random = new XoShiRo256starstar(seed);
            var bytes = new byte[length];
            random.NextBytes(bytes);

            var expected = SHA256.HashData(bytes);
            var actual = BouncySha256.HashData(bytes);
            Assert.Equal(expected, actual);
        }

        public static TheoryData<int, int> RandomTestSource()
        {
            var data = new TheoryData<int, int>();
            var random = new XoShiRo256starstar(42);
            for (var length = 0; length < 256; length++) data.Add(length, 42);
            for (var attempt = 0; attempt < 1024; attempt++) data.Add(random.Next(1048576), random.Next());
            return data;
        }
    }
}
