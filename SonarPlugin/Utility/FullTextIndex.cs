using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SonarPlugin.Utility
{
    /// <summary>
    /// High performance keyword based full text index
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public sealed class FullTextIndex<T>
    {
        private readonly Dictionary<string, HashSet<T>> _index;
        private readonly IEnumerable<T> _items;
        private readonly Func<T, string> _getter;
        private readonly int _minLength;
        private readonly int _maxLength;

        public IDictionary<string, HashSet<T>> Index => this._index;

        public FullTextIndex(IEnumerable<T> items, int minLength = 1, int maxLength = int.MaxValue, Func<T, string>? getter = null, IEqualityComparer<string>? comparer = null)
        {
            this._index = new(comparer: comparer ?? StringComparer.InvariantCultureIgnoreCase);
            this._getter = getter ?? (item => item?.ToString() ?? string.Empty);
            this._minLength = Math.Max(minLength, 1);
            this._maxLength = Math.Max(maxLength, minLength);
            this._items = items.ToArray();
            this.GenerateIndex();
        }

        private void GenerateIndex()
        {
            foreach (var item in this._items)
            {
                var text = this._getter(item);
                for (var start = 0; start < text.Length; start++)
                {
                    var whiteSpaceFound = false;
                    for (var end = 0; !whiteSpaceFound && end < text.Length; end++)
                    {
                        if (start > end) continue;
                        var length = end - start + 1;
                        if (length < this._minLength || length > this._maxLength) continue;
                        var piece = text[start..(end + 1)];
                        whiteSpaceFound = piece.Any(char.IsWhiteSpace);
                        if (whiteSpaceFound) continue;
                        if (!this._index.TryGetValue(piece, out var set))
                        {
                            this._index[piece] = set = new();
                        }
                        set.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Perform a search and return matching items
        /// </summary>
        public IEnumerable<T> Search(string keywords)
        {
            var match = FullTextIndex.s_tokenizerRegex.Match(keywords);
            var sets = new List<HashSet<T>>();
            while (match.Success)
            {
                if (!this._index.TryGetValue(match.Groups["keyword"].Value, out var set)) return Enumerable.Empty<T>();
                sets.Add(set);
                match = match.NextMatch();
            }

            if (sets.Count == 0) return this._items;
            var result = sets[0].AsEnumerable();
            for (var i = 1; i < sets.Count; i++)
            {
                result = result.Intersect(sets[i]);
            }
            return result;
        }
    }

    public static class FullTextIndex
    {
        internal static readonly Regex s_whiteSpaceRegex = new(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        internal static readonly Regex s_tokenizerRegex = new(@"(?<keyword>\S+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public static FullTextIndex<T> Create<T>(IEnumerable<T> items, int minLength = 1, int maxLength = int.MaxValue, Func<T, string>? getter = null, IEqualityComparer<string>? comparer = null)
        {
            return new(items, minLength, maxLength, getter, comparer);
        }
    }
}
