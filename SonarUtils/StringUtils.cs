using SonarUtils.Collections;
using SonarUtils.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils
{
    /// <summary>String utilities for Sonar</summary>
    public static class StringUtils
    {
        private static readonly ConcurrentHashSetSlim<string> s_strings = new(comparer: FarmHashStringComparer.Instance);
        private static readonly ConcurrentDictionarySlim<int, string> s_spansCache = new();

        /// <summary>Interns a <see cref="string"/> into a <see cref="ConcurrentHashSetSlim{T}"/></summary>
        /// <remarks>This is faster than <see cref="string.Intern(string)"/></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string Intern(string key)
        {
            while (true)
            {
                if (s_strings.TryGetValue(key, out var result)) return result;
                if (s_strings.Add(key)) return key;
            }
        }

        /// <summary>Gets an interned string out of a <see cref="ReadOnlySpan{T}"/></summary>
        /// <remarks>This is an alternative version to avoid allocating a <see cref="string"/> while possible.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string Intern(ReadOnlyMemory<char> memory) => Intern(memory.Span);

        /// <summary>Try to get an interned <see cref="string"/> from the <see cref="ConcurrentHashSetSlim{T}"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string? GetInternedIfExist(string key)
        {
            s_strings.TryGetValue(key, out var result);
            return result;
        }

        // === vvv Obsolete stuff beyond this line vvv ===

        /// <summary>
        /// Generate a 1 part key. Interned.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [Obsolete($"This is now a wrapper for {nameof(StringUtils)}.{nameof(Intern)}")]
        public static string GenerateKey(uint part1) => Intern($"{part1}");

        /// <summary>
        /// Generate a 2 parts key. Interned.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [Obsolete($"This is now a wrapper for {nameof(StringUtils)}.{nameof(Intern)}")]
        public static string GenerateKey(uint part1, uint part2) => Intern($"{part1}_{part2}");

        /// <summary>
        /// Generate a 3 parts key. Interned.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [Obsolete($"This is now a wrapper for {nameof(StringUtils)}.{nameof(Intern)}")]
        public static string GenerateKey(uint part1, uint part2, uint part3) => Intern($"{part1}_{part2}_{part3}");

        public static void Reset()
        {
            s_strings.Clear();
            s_spansCache.Clear();
        }
    }
}
