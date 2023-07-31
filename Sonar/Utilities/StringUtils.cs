using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Sonar.Data;
using System;
using SonarUtils.Collections;

// TODO: Remove obsolete methods once their usage are removed from SonarServer

namespace Sonar.Utilities
{
    /// <summary>String utilities for Sonar</summary>
    internal static class StringUtils
    {
        private static readonly ConcurrentHashSetSlim<string> s_strings = new(comparer: FarmHashStringComparer.Instance);

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

        internal static void Reset()
        {
            s_strings.Clear();
        }
    }
}
