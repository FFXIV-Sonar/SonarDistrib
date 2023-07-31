using System.Collections.Generic;
using System.Linq;

namespace SonarUtils.Collections
{
    public static class DictionaryExtensions
    {
        /// <summary>Gets a non-snapshotting version of dictionary keys</summary>
        public static DictionaryKeys<TKey, TValue> GetNonSnapshottingKeys<TKey, TValue>(this IDictionary<TKey, TValue> source) => new(source);

        /// <summary>Gets a non-snapshotting version of dictionary keys</summary>
        public static ReadOnlyDictionaryKeys<TKey, TValue> GetNonSnapshottingKeys<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source) => new(source);

        /// <summary>Gets a non-snapshotting version of dictionary values</summary>
        public static DictionaryValues<TKey, TValue> GetNonSnapshottingValues<TKey, TValue>(this IDictionary<TKey, TValue> source) => new(source);

        /// <summary>Gets a non-snapshotting version of dictionary values</summary>
        public static ReadOnlyDictionaryValues<TKey, TValue> GetNonSnapshottingValues<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source) => new(source);

        /// <summary>Gets an inverted dictionary (copy)</summary>
        public static IDictionary<TValue, TKey> GetInverseDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source) where TValue : notnull => source.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }
}
