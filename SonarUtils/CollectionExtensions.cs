using SonarUtils.Collections;
using System;
using System.Collections.Generic;

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



        // Who's idea was to implement this only for IReadOnlyDictionary<TKey, TValue>?
        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            if (!dict.TryGetValue(key, out var value)) return default;
            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            if (!dict.TryGetValue(key, out var value)) return defaultValue;
            return value;
        }
    }
}
