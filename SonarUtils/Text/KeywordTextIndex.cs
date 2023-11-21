using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace SonarUtils.Text
{
    /// <summary>
    /// High performance keyword based full text index
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class KeywordTextIndex<T>
    {
        private readonly IReadOnlyDictionary<string, IEnumerable<T>> _index;
        private readonly IEqualityComparer<string> _comparer;
        private readonly IEnumerable<T> _items;
        private readonly Func<T, string> _getter;
        private readonly int _minLength;
        private readonly int _maxLength;

        public IReadOnlyDictionary<string, IEnumerable<T>> Index => this._index;

        public KeywordTextIndex(IEnumerable<T> items, int minLength = 1, int maxLength = int.MaxValue, Func<T, string>? getter = null, IEqualityComparer<string>? comparer = null)
        {
            this._getter = getter ?? (item => item?.ToString() ?? string.Empty);
            this._minLength = Math.Max(minLength, 1);
            this._maxLength = Math.Max(maxLength, minLength);
            this._items = items.ToArray();
            this._comparer = comparer ?? StringComparer.InvariantCultureIgnoreCase;
            this._index = this.GenerateIndex();
        }

        private IReadOnlyDictionary<string, IEnumerable<T>> GenerateIndex()
        {
            var index = new Dictionary<string, HashSet<T>>(this._items.Count(), this._comparer);
            foreach (var item in this._items)
            {
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
                            if (!exists) set = new();
                            set.Add(item);
                        }
                    }
                    match = match.NextMatch();
                }
            }
#if NET8_0_OR_GREATER
            return index.ToFrozenDictionary(kvp => kvp.Key, kvp => (IEnumerable<T>)[..kvp.Value], this._comparer);
#else
            return index.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<T>)kvp.Value.ToArray(), this._comparer).AsReadOnly();
#endif
        }

        /// <summary>
        /// Perform a search and return matching items
        /// </summary>
        public IEnumerable<T> Search(string keywords)
        {
            var sets = new List<IEnumerable<T>>();

            var match = KeywordTextIndex.s_tokenizerRegex.Match(keywords);
            while (match.Success)
            {
                foreach (var keyword in this.SearchCore(match.Groups["keyword"].Value))
                {
                    if (!this._index.TryGetValue(keyword, out var set)) return Enumerable.Empty<T>();
                    sets.Add(set);
                }
                match = match.NextMatch();
            }
            if (sets.Count == 0) return this._items;

            var result = sets[0];
            for (var i = 1; i < sets.Count; i++)
            {
                result = result.Intersect(sets[i]);
            }
            return result;
        }

        private IEnumerable<string> SearchCore(string keyword)
        {
            if (keyword.Length < this._minLength) yield break;
            else if (keyword.Length > this._maxLength)
            {
                for (var start = 0; start < keyword.Length - this._maxLength + 1; start++)
                {
                    yield return keyword.Substring(start, this._maxLength);
                }
            }
            else yield return keyword;
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
