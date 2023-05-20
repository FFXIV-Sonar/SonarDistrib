using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Utilities
{
    /// <summary>
    /// Ordinal string comparison using <see cref="Farmhash.Sharp.Farmhash"/>
    /// </remarks>
    /// Hash is generated using Hash64 instead of Hash32
    /// </remarks>
    public sealed class FarmHashStringComparer : IEqualityComparer<string>
    {
        public static readonly FarmHashStringComparer Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(string? x, string? y) => string.Equals(x, y); //x.AsSpan().SequenceEqual(y.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode([DisallowNull] string obj) => (int)Farmhash.Sharp.Farmhash.Hash64(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCodeStatic([DisallowNull] string obj) => (int)Farmhash.Sharp.Farmhash.Hash64(obj);
    }
}
