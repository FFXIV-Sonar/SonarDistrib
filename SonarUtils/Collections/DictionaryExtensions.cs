using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    public static class DictionaryExtensions
    {
        /// <summary>Gets a non-snapshotting version of dictionary keys</summary>
        public static ICollection<TKey> GetNonSnapshottingKeys<TKey, TValue>(this IDictionary<TKey, TValue> source) => new DictionaryKeys<TKey, TValue>(source);

        /// <summary>Gets a non-snapshotting version of dictionary values</summary>
        public static ICollection<TValue> GetNonSnapshottingValues<TKey, TValue>(this IDictionary<TKey, TValue> source) => new DictionaryValues<TKey, TValue>(source);

        /// <summary>Gets an inverted dictionary (copy)</summary>
        public static IDictionary<TValue, TKey> GetInverseDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source) where TValue : notnull => source.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }
}
