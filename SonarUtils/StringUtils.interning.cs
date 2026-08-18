using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AG.Collections.Concurrent;
using SonarUtils.Collections;
using SonarUtils.Text;

namespace SonarUtils
{
    /// <content>Interning functionality.</content>
    public static partial class StringUtils
    {
        private static readonly ConcurrentTrieSet<string> s_strings = new(comparer: FarmHashStringComparer.Instance);
        private static readonly ConcurrentDictionarySlim<int, string> s_spansCache = [];

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Intern(string str) => s_strings.GetOrAdd(str);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2) Intern(string str1, string str2) => (Intern(str1), Intern(str2));

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <param name="str3"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2, string Result3) Intern(string str1, string str2, string str3) => (Intern(str1), Intern(str2), Intern(str3));

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <param name="str3"><see langword="string"/> to be interned.</param>
        /// <param name="str4"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2, string Result3, string Result4) Intern(string str1, string str2, string str3, string str4) => (Intern(str1), Intern(str2), Intern(str3), Intern(str4));

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static string Intern(ReadOnlySpan<char> str)
        {
            var hash = FarmHashStringComparer.GetHashCodeStatic(str);
            if (s_spansCache.TryGetValue(hash, out var result) && str.SequenceEqual(result.AsSpan())) return result;
            s_spansCache[hash] = result = Intern(new string(str));
            return result;
        }

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2) Intern(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2) => (Intern(str1), Intern(str2));

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <param name="str3"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2, string Result3) Intern(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3) => (Intern(str1), Intern(str2), Intern(str3));

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <param name="str3"><see langword="string"/> to be interned.</param>
        /// <param name="str4"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2, string Result3, string Result4) Intern(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3, ReadOnlySpan<char> str4) => (Intern(str1), Intern(str2), Intern(str3), Intern(str4));

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static string Intern(ReadOnlyMemory<char> str) => Intern(str.Span);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2) Intern(ReadOnlyMemory<char> str1, ReadOnlyMemory<char> str2) => Intern(str1.Span, str2.Span);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <param name="str3"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2, string Result3) Intern(ReadOnlyMemory<char> str1, ReadOnlyMemory<char> str2, ReadOnlyMemory<char> str3) => Intern(str1.Span, str2.Span, str3.Span);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> to be interned.</param>
        /// <param name="str2"><see langword="string"/> to be interned.</param>
        /// <param name="str3"><see langword="string"/> to be interned.</param>
        /// <param name="str4"><see langword="string"/> to be interned.</param>
        /// <returns>Interned <see langword="string"/>s.</returns>
        public static (string Result1, string Result2, string Result3, string Result4) Intern(ReadOnlyMemory<char> str1, ReadOnlyMemory<char> str2, ReadOnlyMemory<char> str3, ReadOnlyMemory<char> str4) => Intern(str1.Span, str2.Span, str3.Span, str4.Span);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> reference to ensure is interned.</param>
        public static void EnsureInterned(ref string str1) => (str1) = Intern(str1);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> reference to ensure is interned.</param>
        /// <param name="str2"><see langword="string"/> reference to ensure is interned.</param>
        public static void EnsureInterned(ref string str1, ref string str2) => (str1, str2) = Intern(str1, str2);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> reference to ensure is interned.</param>
        /// <param name="str2"><see langword="string"/> reference to ensure is interned.</param>
        /// <param name="str3"><see langword="string"/> reference to ensure is interned.</param>
        public static void EnsureInterned(ref string str1, ref string str2, ref string str3) => (str1, str2, str3) = Intern(str1, str2, str3);

        /// <summary>Intern <see langword="string"/>s.</summary>
        /// <param name="str1"><see langword="string"/> reference to ensure is interned.</param>
        /// <param name="str2"><see langword="string"/> reference to ensure is interned.</param>
        /// <param name="str3"><see langword="string"/> reference to ensure is interned.</param>
        /// <param name="str4"><see langword="string"/> reference to ensure is interned.</param>
        public static void EnsureInterned(ref string str1, ref string str2, ref string str3, ref string str4) => (str1, str2, str3, str4) = Intern(str1, str2, str3, str4);

        /// <summary>Interns all <see langword="string"/>s contained in <paramref name="span"/>.</summary>
        /// <param name="span"><see cref="ReadOnlySpan{T}"/> of <see langword="string"/>s.</param>
        /// <remarks>This mutates <paramref name="span"/>!</remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void EnsureInterned(ReadOnlySpan<string> span)
        {
            foreach (ref readonly var input in span)
            {
                ref var str = ref Unsafe.AsRef(in input);
                EnsureInterned(ref str);
            }
        }
    }
}
