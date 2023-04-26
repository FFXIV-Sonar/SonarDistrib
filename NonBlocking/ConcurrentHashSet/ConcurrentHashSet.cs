using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NonBlocking
{
    /// <summary>
    /// <see cref="ConcurrentHashSet{T}"/> forwarded onto a backing <see cref="ConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ConcurrentHashSet<T> : ISet<T>, IReadOnlySet<T> where T : notnull
    {
        private readonly ConcurrentDictionary<T, object?> _backingDictionary;

        public ConcurrentHashSet()
        {
            _backingDictionary = new();
        }

        public ConcurrentHashSet(IEnumerable<T> items)
        {
            _backingDictionary = new(items.Select(item => KeyValuePair.Create(item, (object?)null)));
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            _backingDictionary = new(comparer);
        }

        public ConcurrentHashSet(int concurrencyLevel, int capacity)
        {
            _backingDictionary = new(concurrencyLevel, capacity);
        }

        public ConcurrentHashSet(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            _backingDictionary = new(items.Select(item => KeyValuePair.Create(item, (object?)null)), comparer);
        }

        public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer)
        {
            _backingDictionary = new(concurrencyLevel, capacity, comparer);
        }

        public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            _backingDictionary = new(concurrencyLevel, items.Select(item => KeyValuePair.Create(item, (object?)null)), comparer);
        }

        public int Count => _backingDictionary.Count;
        public int EstimatedCount => _backingDictionary.EstimatedCount;

        public bool IsReadOnly => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item) => _backingDictionary.TryAdd(item, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item) => _backingDictionary.TryRemove(item, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _backingDictionary.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => _backingDictionary.ContainsKey(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in _backingDictionary.Keys)
            {
                array[arrayIndex++] = item;
                if (arrayIndex == array.Length) break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator() => _backingDictionary.Keys.GetEnumerator();

        public void ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();
        public void IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();
        public bool IsProperSubsetOf(IEnumerable<T> other) => throw new NotSupportedException();
        public bool IsProperSupersetOf(IEnumerable<T> other) => throw new NotSupportedException();
        public bool IsSubsetOf(IEnumerable<T> other) => throw new NotSupportedException();
        public bool IsSupersetOf(IEnumerable<T> other) => throw new NotSupportedException();
        public bool Overlaps(IEnumerable<T> other) => throw new NotSupportedException();
        public bool SetEquals(IEnumerable<T> other) => throw new NotSupportedException();
        public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();
        public void UnionWith(IEnumerable<T> other) => throw new NotSupportedException();
        void ICollection<T>.Add(T item) => Add(item);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
