using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public static T[] GetItems<T>(this RandomNumberGenerator random, ReadOnlySpan<T> choices, int length)
        {
            var items = new T[length];
            random.GetItems(choices, items.AsSpan());
            return items;
        }

        [SuppressMessage("Minor Code Smell", "S3236", Justification = "Some trickery")]
        public static void GetItems<T>(this RandomNumberGenerator random, ReadOnlySpan<T> choices, Span<T> destination)
        {
            ArgumentException.ThrowIfNullOrEmpty(choices.IsEmpty ? string.Empty : "x", nameof(choices));
            for (var i = 0; i < destination.Length; i++)
            {
                destination[i] = choices[random.Next(choices.Length)];
            }
        }
    }
}
