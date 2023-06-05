using SonarUtils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace SonarUtils
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items) action(item);
        }

        public static void ForEach<T>(this ReadOnlySpan<T> items, Action<T> action)
        {
            foreach (var item in items) action(item);
        }

        public static int IndexOf<T>(this T[] array, T item) => Array.IndexOf(array, item);

        public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys) => keys.ForEach(key => dict.Remove(key));

        /// <summary>Converts an enumerable to an <see cref="InternalList{T}"/></summary>
        public static InternalList<T> ToInternalList<T>(this IEnumerable<T> source) => new(source);

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static void AddRange<T>(this ICollection<T> collection, Span<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
