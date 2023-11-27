using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xoshiro.PRNG64;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace SonarUtils.Random
{
    public static partial class RandomExtensions
    {
#if !NET8_0_OR_GREATER
        public static T[] GetItems<T>(this XoShiRo256starstar random, ReadOnlySpan<T> choices, int length)
        {
            var items = new T[length];
            random.GetItems(choices, items.AsSpan());
            return items;
        }

        [SuppressMessage("Minor Code Smell", "S3236", Justification = "Some trickery")]
        public static void GetItems<T>(this XoShiRo256starstar random, ReadOnlySpan<T> choices, Span<T> destination)
        {
            ArgumentException.ThrowIfNullOrEmpty(choices.IsEmpty ? string.Empty : "x", nameof(choices));
            for (var i = 0; i < destination.Length; i++)
            {
                destination[i] = choices[random.Next(choices.Length)];
            }
        }
#endif
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
