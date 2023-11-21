using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Text
{
    /// <summary>
    /// Ordinal string comparison using <see cref="Farmhash.Sharp.Farmhash"/>
    /// </summary>
    /// <remarks>
    /// Hash is generated using Hash64 instead of Hash32
    /// </remarks>
    public sealed class FarmHashStringComparer : IEqualityComparer<string>, IEqualityComparer<ReadOnlyMemory<char>>
    {
        public static readonly FarmHashStringComparer Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(string? x, string? y) => string.Equals(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y) => x.SequenceEqual(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => x.Span.SequenceEqual(y.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode([DisallowNull] string obj) => (int)Farmhash.Sharp.Farmhash.Hash64(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode([DisallowNull] ReadOnlySpan<char> obj) => (int)Farmhash.Sharp.Farmhash.Hash64(MemoryMarshal.Cast<char, byte>(obj));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode([DisallowNull] ReadOnlyMemory<char> obj) => (int)Farmhash.Sharp.Farmhash.Hash64(MemoryMarshal.Cast<char, byte>(obj.Span));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCodeStatic([DisallowNull] string obj) => (int)Farmhash.Sharp.Farmhash.Hash64(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCodeStatic([DisallowNull] ReadOnlySpan<char> obj) => (int)Farmhash.Sharp.Farmhash.Hash64(MemoryMarshal.Cast<char, byte>(obj));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCodeStatic([DisallowNull] ReadOnlyMemory<char> obj) => (int)Farmhash.Sharp.Farmhash.Hash64(MemoryMarshal.Cast<char, byte>(obj.Span));
    }
}
