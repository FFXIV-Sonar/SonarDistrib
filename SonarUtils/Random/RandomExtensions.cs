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
    public static partial class RandomExtensions
    {
        public static string GetString(this XoShiRo256starstar random, int length, string chars = RandomChars.Base64Url)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(chars));
            return new(random.GetItems(chars.AsSpan(), length));
        }

        public static string GetString(this RandomNumberGenerator random, int length, string chars = RandomChars.Base64Url)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(chars));
            return new(random.GetItems(chars.AsSpan(), length));
        }
    }
}
