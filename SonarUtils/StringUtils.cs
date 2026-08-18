using AG.Collections.Concurrent;
using SonarUtils.Collections;
using SonarUtils.Text;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SonarUtils
{
    /// <summary>String utilities for Sonar</summary>
    public static partial class StringUtils
    {
        private static readonly string?[] s_integerCache = new string?[65536];

        /// <summary>Gets a string out of an <see cref="int"/>.</summary>
        /// <remarks>Values within <see cref="short.MinValue"/> and <see cref="short.MaxValue"/> are cached.</remarks>
        /// <param name="number">Number to get the string from.</param>
        /// <returns><paramref name="number"/> as a string.</returns>
        public static string GetNumber(long number)
        {
            if (number is < short.MinValue or > short.MaxValue) return number.ToString(CultureInfo.InvariantCulture);
            return s_integerCache[number - short.MinValue] ??= Intern(number.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Gets a string out of an <see cref="int"/>.</summary>
        /// <remarks>Values within <see cref="short.MinValue"/> and <see cref="short.MaxValue"/> are cached.</remarks>
        /// <param name="number">Number to get the string from.</param>
        /// <returns><paramref name="number"/> as a string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetNumber(ulong number)
        {
            if (number is > (ulong)short.MaxValue) return number.ToString(CultureInfo.InvariantCulture);
            return GetNumber((long)number);
        }

        /// <summary>Try to get an interned <see cref="string"/> from the <see cref="ConcurrentTrieSet{T}"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetInternedIfExist(string key)
        {
            s_strings.TryGetValue(key, out var result);
            return result;
        }

        /// <summary>Make sure <paramref name="stringA"/> and <paramref name="stringB"/> are in ordinal order, and returns its comparison result.</summary>
        /// <returns>Result of <see cref="MemoryExtensions.CompareTo(ReadOnlySpan{char}, ReadOnlySpan{char}, StringComparison)"/> <see cref="StringComparison.Ordinal"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EnsureOrdinalOrder(ref string stringARef, ref string stringBRef)
        {
            // Capture variables
            var stringA = stringARef;
            var stringB = stringBRef;

            // Perform comparison
            var compare = stringA.CompareTo(stringB, StringComparison.Ordinal);

            // Swap to keep ordinal order and return comparison result
            if (compare > 0) (stringARef, stringBRef) = (stringB, stringA);
            return compare;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreCharsInclusive(ReadOnlySpan<char> chars, ReadOnlySpan<char> validChars)
        {
            foreach (var ch in chars)
            {
                if (!validChars.Contains(ch)) return false;
            }
            return true;
        }

        /// <summary>Resets interned strings</summary>
        public static void Reset()
        {
            s_strings.Clear();
            s_spansCache.Clear();
            Array.Clear(s_integerCache);
        }
    }
}
