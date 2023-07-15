using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Sonar.Data;
using System;
using ConcurrentCollections;

// TODO: Remove obsolete methods once their usage are removed from SonarServer

namespace Sonar.Utilities
{
    /// <summary>String utilities for Sonar</summary>
    internal static class StringUtils
    {
        private static readonly ConcurrentHashSet<string> s_strings = new(comparer: FarmHashStringComparer.Instance);

        /// <summary>Interns a <see cref="string"/> into a <see cref="ConcurrentHashSet{T}"/></summary>
        /// <remarks>This is faster than <see cref="string.Intern(string)"/></remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string Intern(string key)
        {
            if (!s_strings.TryGetValue(key, out var result))
            {
                if (!s_strings.Add(key)) return Intern(key);
                return key;
            }
            return result;
        }

        /// <summary>Try to get an interned <see cref="string"/> from the <see cref="ConcurrentHashSet{T}"/></summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static string? GetInternedIfExist(string key)
        {
            s_strings.TryGetValue(key, out var result);
            return result;
        }

        // === vvv Obsolete stuff beyond this line vvv ===

        [Obsolete]
        private static readonly NonBlocking.NonBlockingDictionary<StringKey, string> s_keys = new();

        /// <summary>
        /// Generate a 1 part key. Fully cached.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [Obsolete]
        public static string GenerateKey(uint part1) => s_keys.GetOrAdd(StringKey.Create(part1), static key => key.ToString()!);

        /// <summary>
        /// Generate a 2 parts key. Fully cached.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [Obsolete]
        public static string GenerateKey(uint part1, uint part2) => s_keys.GetOrAdd(StringKey.Create(part1, part2), static key => key.ToString()!);

        /// <summary>
        /// Generate a 3 parts key. Fully cached.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [Obsolete]
        public static string GenerateKey(uint part1, uint part2, uint part3) => s_keys.GetOrAdd(StringKey.Create(part1, part2, part3), static key => key.ToString()!);

        /// <summary>
        /// Generate a 4 parts key. Fully cached.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        [Obsolete]
        public static string GenerateKey(uint part1, uint part2, uint part3, uint part4) => s_keys.GetOrAdd(StringKey.Create(part1, part2, part3, part4), static key => key.ToString()!);

        /// <summary>
        /// Generate a key with 5 or more parts. Partially cached (first 4 parts).
        /// </summary>
        [Obsolete]
        public static string GenerateKey(params uint[] parts) => string.Join('_', parts.Select(p => p.ToString()));

        internal static void Reset()
        {
            s_keys.Clear(); // Obsolete
            s_strings.Clear(); // New
        }
    }
}
