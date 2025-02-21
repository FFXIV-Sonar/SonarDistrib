using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SonarUtils.Text
{
    /// <summary>High performance keyword based full text index</summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class KeywordTextIndex<T>
    {
        private readonly FrozenDictionary<string, ImmutableArray<int>> _index;
        private readonly IEqualityComparer<string> _comparer;
        private readonly ImmutableArray<T> _items;
        private readonly Func<T, string> _getter;
        private readonly int _minLength;
        private readonly int _maxLength;

        public FrozenDictionary<string, ImmutableArray<int>> Index => this._index;
        public ImmutableArray<T> Items => this._items;

        public KeywordTextIndex(IEnumerable<T> items, int minLength = 1, int maxLength = int.MaxValue, Func<T, string>? getter = null, IEqualityComparer<string>? comparer = null)
        {
            this._getter = getter ?? (item => item?.ToString() ?? string.Empty);
            this._minLength = Math.Max(minLength, 1);
            this._maxLength = Math.Max(maxLength, minLength);
            this._items = items.ToImmutableArray();
            this._comparer = comparer ?? StringComparer.InvariantCultureIgnoreCase;
            this._index = this.GenerateIndex();
        }

        private FrozenDictionary<string, ImmutableArray<int>> GenerateIndex()
        {
            var items = this._items.AsSpan();
            var index = new Dictionary<string, HashSet<int>>(items.Length, this._comparer); // Needs to be hashset to avoid possible duplicates from similar keywords
            for (var i = 0; i < items.Length; i++)
            {
                ref readonly var item = ref items[i];
                var text = this._getter(item);
                var match = KeywordTextIndex.s_tokenizerRegex.Match(text);
                while (match.Success)
                {
                    var wordSpan = match.Groups["keyword"].ValueSpan;
                    for (var start = 0; start < wordSpan.Length; start++)
                    {
                        for (var end = start + this._minLength - 1; end < wordSpan.Length; end++)
                        {
                            if (end - start + 1 > this._maxLength) break;
                            var piece = StringUtils.Intern(wordSpan[start..(end + 1)]);

                            ref var set = ref CollectionsMarshal.GetValueRefOrAddDefault(index, piece, out var exists)!;
                            if (!exists) set = [];
                            set.Add(i);
                        }
                    }
                    match = match.NextMatch();
                }
            }
            return index.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value.ToImmutableArray(), this._comparer);
        }

        /// <summary>Perform a search and return matching items</summary>
        public IEnumerable<T> Search(string keywords)
        {
            foreach (var itemIndex in this.SearchCore(keywords)) yield return this._items[itemIndex];
        }

        /// <summary>Perform a search and return matching item indexes</summary>
        public IEnumerable<int> SearchCore(string keywords)
        {
            var sets = new List<ImmutableArray<int>>();

            var match = KeywordTextIndex.s_tokenizerRegex.Match(keywords);
            while (match.Success)
            {
                foreach (var keyword in this.SearchCoreInternal(match.Groups["keyword"].ValueSpan))
                {
                    if (!this._index.TryGetValue(keyword.ToString(), out var set)) return []; // TODO: AlternativeComparer
                    sets.Add(set);
                }
                match = match.NextMatch();
            }
            if (sets.Count == 0) return Enumerable.Range(0, this._items.Length);

            var result = new HashSet<int>(sets.MinBy(set => set.Length));
            result.RemoveWhere(itemIndex => !sets.All((set => set.Contains(itemIndex))));
            return result;
        }

        [SuppressMessage("Major Code Smell", "S1144", Justification = "foreach")]
        private ref struct SearchCoreInternalEnumerator
        {
            private readonly ReadOnlySpan<char> _keyword;
            private readonly int _length;
            private int _current;

            public SearchCoreInternalEnumerator(ReadOnlySpan<char> keyword, int maxLength)
            {
                this._keyword = keyword;
                this._length = Math.Min(maxLength, keyword.Length);
                this._current = -1;
            }
            public readonly SearchCoreInternalEnumerator GetEnumerator() => this; // workaround to be useable at foreach
            public readonly ReadOnlySpan<char> Current => this._keyword[this._current..(this._current + this._length)];
            public bool MoveNext() => ++this._current + this._length <= this._keyword.Length;
        }

        private SearchCoreInternalEnumerator SearchCoreInternal(ReadOnlySpan<char> keyword)
        {
            return new SearchCoreInternalEnumerator(keyword, this._maxLength);
        }
    }

    public static partial class KeywordTextIndex
    {
        internal static readonly Regex s_tokenizerRegex = GetTokenizerRegex();

        public static KeywordTextIndex<T> Create<T>(IEnumerable<T> items, int minLength = 1, int maxLength = int.MaxValue, Func<T, string>? getter = null, IEqualityComparer<string>? comparer = null)
        {
            return new(items, minLength, maxLength, getter, comparer);
        }

        [GeneratedRegex(@"\b(?<keyword>\S+?)\b", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetTokenizerRegex();
    }
}
