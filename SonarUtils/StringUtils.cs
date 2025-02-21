using AG.Collections.Concurrent;
using SonarUtils.Collections;
using SonarUtils.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils
{
    /// <summary>String utilities for Sonar</summary>
    public static partial class StringUtils
    {
        private static readonly ConcurrentTrieSet<string> s_strings = new(comparer: FarmHashStringComparer.Instance);
        private static readonly ConcurrentDictionarySlim<int, string> s_spansCache = [];
        private static readonly string?[] s_integerCache = new string?[65536];

        /// <summary>Interns a <see cref="string"/> into a <see cref="ConcurrentTrieSet{T}"/></summary>
        /// <remarks>This is faster than <see cref="string.Intern(string)"/></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Intern(string key) => s_strings.GetOrAdd(key);

        /// <summary>Gets an interned string out of a <see cref="ReadOnlySpan{T}"/></summary>
        /// <remarks>This is an alternative version to avoid allocating a <see cref="string"/> while possible.</remarks>
        public static string Intern(ReadOnlySpan<char> span)
        {
            var hash = FarmHashStringComparer.GetHashCodeStatic(span);
            if (s_spansCache.TryGetValue(hash, out var key) && span.SequenceEqual(key.AsSpan())) return key;
            var result = Intern(new string(span));
            s_spansCache[hash] = result;
            return result;
        }

        /// <summary>Gets an interned string out of a <see cref="ReadOnlyMemory{T}"/></summary>
        /// <remarks>This is an alternative version to avoid allocating a <see cref="string"/> while possible.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Intern(ReadOnlyMemory<char> memory) => Intern(memory.Span);

        /// <summary>Gets a string out of an <see cref="int"/>.</summary>
        /// <remarks>Values within <see cref="short.MinValue"/> and <see cref="short.MaxValue"/> are cached.</remarks>
        /// <param name="number">Number to get the string from.</param>
        /// <returns><paramref name="number"/> as a string.</returns>
        public static string GetNumber(long number)
        {
            if (number is < short.MinValue or > short.MaxValue) return number.ToString();
            return s_integerCache[number - short.MinValue] ??= Intern(number.ToString());
        }

        /// <summary>Gets a string out of an <see cref="int"/>.</summary>
        /// <remarks>Values within <see cref="short.MinValue"/> and <see cref="short.MaxValue"/> are cached.</remarks>
        /// <param name="number">Number to get the string from.</param>
        /// <returns><paramref name="number"/> as a string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetNumber(ulong number)
        {
            if (number is > (ulong)short.MaxValue) return number.ToString();
            return GetNumber((long)number);
        }

        /// <summary>Try to get an interned <see cref="string"/> from the <see cref="ConcurrentTrieSet{T}"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetInternedIfExist(string key)
        {
            s_strings.TryGetValue(key, out var result);
            return result;
        }

        /// <summary>Make sure <paramref name="stringA"/> and <paramref name="stringB"/> are in ordinal order</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureOrdinalOrder(ref string stringA, ref string stringB)
        {
            if (stringA.CompareTo(stringB) >= 0) (stringA, stringB) = (stringB, stringA);
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
