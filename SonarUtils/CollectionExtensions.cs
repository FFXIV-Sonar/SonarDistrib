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

        public static void ForEach<T>(this T[] items, Action<T> action)
        {
            foreach (var item in items) action(item);
        }

        public static void ForEach<T, TState>(this IEnumerable<T> items, TState state, Action<T, TState> action)
        {
            foreach (var item in items) action(item, state);
        }

        public static void ForEach<T, TState>(this ReadOnlySpan<T> items, TState state, Action<T, TState> action)
        {
            foreach (var item in items) action(item, state);
        }

        public static void ForEach<T, TState>(this T[] items, TState state, Action<T, TState> action)
        {
            foreach (var item in items) action(item, state);
        }

        public static int IndexOf<T>(this T[] array, T item) => Array.IndexOf(array, item);

        /// <summary>Converts an enumerable to an <see cref="InternalList{T}"/></summary>
        public static InternalList<T> ToInternalList<T>(this IEnumerable<T> source) => new(source);

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items) => items.ForEach(collection.Add);

        public static void AddRange<T>(this ICollection<T> collection, ReadOnlySpan<T> items) => items.ForEach(collection.Add);

        public static void AddRange<T>(this ICollection<T> collection, params T[] items) => items.ForEach(collection.Add);

        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items) => items.ForEach(item => collection.Remove(item));

        public static void RemoveRange<T>(this ICollection<T> collection, ReadOnlySpan<T> items) => items.ForEach(item => collection.Remove(item));

        public static void RemoveRange<T>(this ICollection<T> collection, params T[] items) => items.ForEach(item => collection.Remove(item));

        public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys) => keys.ForEach(key => dict.Remove(key));
    }
}
