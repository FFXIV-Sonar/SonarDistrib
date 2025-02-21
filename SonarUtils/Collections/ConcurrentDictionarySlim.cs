using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SonarUtils.Collections
{
    public sealed class ConcurrentDictionarySlim<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private static readonly ConcurrentBag<List<KeyValuePair<TKey, TValue>>> s_enumeratorBuffers = new();
        private Dictionary<TKey, TValue>[] _dicts = Array.Empty<Dictionary<TKey, TValue>>();
        private readonly IEqualityComparer<TKey> _comparer;
        private ulong _fastModMultiplier;
        
        private int _count;

        private DictionaryKeys<TKey, TValue>? _keys;
        private DictionaryValues<TKey, TValue>? _values;

        public int Count => this._count;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => this._keys ??= new(this);

        public ICollection<TValue> Values => this._values ??= new(this);

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        public ConcurrentDictionarySlim() : this(EqualityComparer<TKey>.Default) { /* Empty */ }

        public ConcurrentDictionarySlim(IEqualityComparer<TKey> comparer)
        {
            this._comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCountTooLarge(int count, int length) => count >= length * length + length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int HashToIndex(int hash, int length, ulong fastModMultiplier) =>
            (int)HashHelpers.FastMod(uint.RotateRight((uint)hash, 16), (uint)length, fastModMultiplier);

        private static void AcquireAllLocks(Dictionary<TKey, TValue>[] dicts)
        {
            foreach (var dict in dicts) Monitor.Enter(dict);
        }

        private static void ReleaseAllLocks(Dictionary<TKey, TValue>[] dicts)
        {
            foreach (var dict in dicts) Monitor.Exit(dict);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNeeded()
        {
            if (IsCountTooLarge(this._count, this._dicts.Length)) this.ResizeIfNeededSlow();
        }

        private void ResizeIfNeededSlow()
        {
            while (true)
            {
                var count = this._count;
                var dicts = this._dicts;
                if (!IsCountTooLarge(count, dicts.Length)) return;

                var newLength = AG.PrimeUtils.FindNext(dicts.Length * 2);
                if (newLength < dicts.Length) return;
                lock (dicts)
                {
                    if (this._dicts != dicts) continue; // retry
                    try
                    {
                        AcquireAllLocks(dicts);
                        count = this._count;
                        if (!IsCountTooLarge(count, dicts.Length)) return;

                        newLength = AG.PrimeUtils.FindNext(dicts.Length * 2);
                        if (newLength <= dicts.Length) return;
                        //Console.WriteLine($"dict resize: {dicts.Length} => {newLength}");

                        var newDicts = new Dictionary<TKey, TValue>[newLength];
                        var newCapacity = newLength + (int)Math.Sqrt(newLength);
                        foreach (ref var dict in newDicts.AsSpan()) dict = new(newCapacity, comparer: this._comparer);

                        var fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newLength);
                        foreach (var dict in dicts)
                        {
                            foreach (var item in dict) newDicts[HashToIndex(this._comparer.GetHashCode(item.Key), newLength, fastModMultiplier)].Add(item.Key, item.Value);
                        }
                        this._fastModMultiplier = fastModMultiplier;
                        this._dicts = newDicts;
                    }
                    finally
                    {
                        ReleaseAllLocks(dicts);
                    }
                }
            }
        }

        private ref TValue Find(TKey key)
        {
            if (this._dicts.Length == 0) return ref Unsafe.NullRef<TValue>();
            var hash = this._comparer.GetHashCode(key);
            while (true)
            {
                var dicts = this._dicts;
                var fastModMultiplier = this._fastModMultiplier;

                var index = HashToIndex(hash, dicts.Length, fastModMultiplier);
                if (index > dicts.Length) continue; // retry
                var dict = dicts[index];
                lock (dict)
                {
                    if (dicts != this._dicts || fastModMultiplier != this._fastModMultiplier) continue; // retry
                    return ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);
                }
            }
        }

        private ref TValue FindUnsafe(TKey key)
        {
            if (this._dicts.Length == 0) return ref Unsafe.NullRef<TValue>();
            var hash = this._comparer.GetHashCode(key);

            var dicts = this._dicts;
            var fastModMultiplier = this._fastModMultiplier;

            var index = HashToIndex(hash, dicts.Length, fastModMultiplier);
            if (index > dicts.Length) return ref this.Find(key); // retry
            var dict = dicts[index];
            return ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);
        }

        /// <returns>true if added, false if duplicate or replaced</returns>
        private bool AddOrReplace(TKey key, TValue value, bool replace)
        {
            var hash = this._comparer.GetHashCode(key);
            this.ResizeIfNeeded();
            while (true)
            {
                var dicts = this._dicts;
                var fastModMultiplier = this._fastModMultiplier;

                var index = HashToIndex(hash, dicts.Length, fastModMultiplier);
                if (index > dicts.Length) continue; // retry
                var dict = dicts[index];
                bool exists;
                lock (dict)
                {
                    if (dicts != this._dicts || fastModMultiplier != this._fastModMultiplier) continue; // retry
                    ref var dictValue = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out exists);
                    if (!exists || replace) dictValue = value;
                }
                if (!exists) Interlocked.Increment(ref this._count);
                return !exists;
            }
        }


        public void Add(TKey key, TValue value)
        {
            if (!this.TryAdd(key, value)) throw new ArgumentException($"{nameof(key)} already exists");
        }

        public bool TryAdd(TKey key, TValue value) => this.AddOrReplace(key, value, false);

        public TValue this[TKey key]
        {
            get
            {
                ref var value = ref this.Find(key);
                if (Unsafe.IsNullRef(ref value)) throw new KeyNotFoundException();
                return value;
            }
            set => this.AddOrReplace(key, value, true);
        }

        public void Clear()
        {
            while (true)
            {
                var dicts = this._dicts;
                lock (dicts)
                {
                    if (this._dicts != dicts) continue; //retry
                    foreach (var dict in dicts)
                    {
                        int count;
                        lock (dict)
                        {
                            count = dict.Count;
                            dict.Clear();
                        }
                        Interlocked.Add(ref this._count, -count);
                    }
                    return;
                }
            }
        }

        public bool ContainsKey(TKey key) => !Unsafe.IsNullRef(ref this.Find(key));

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this) array[arrayIndex++] = item;
        }

        public bool Remove(TKey key) => this.Remove(key, out _);

        public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (this._dicts.Length == 0)
            {
                value = default;
                return false;
            }
            var hash = this._comparer.GetHashCode(key);
            while (true)
            {
                var dicts = this._dicts;
                var fastModMultiplier = this._fastModMultiplier;
                var index = HashToIndex(hash, dicts.Length, fastModMultiplier);
                if (index > dicts.Length) continue; // retry
                var dict = dicts[index];

                bool result;
                lock (dict)
                {
                    if (this._dicts != dicts || this._fastModMultiplier != fastModMultiplier) continue; // retry
                    result = dict.Remove(key, out value);
                }
                if (result) Interlocked.Decrement(ref this._count);
                return result;
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            ref var dictValue = ref this.Find(key);
            if (Unsafe.IsNullRef(ref dictValue))
            {
                value = default;
                return false;
            }
            else
            {
                value = dictValue;
                return true;
            }
        }

        public bool TryGetValueUnsafe(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            ref var dictValue = ref this.FindUnsafe(key);
            if (Unsafe.IsNullRef(ref dictValue))
            {
                value = default;
                return false;
            }
            else
            {
                value = dictValue;
                return true;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (!s_enumeratorBuffers.TryTake(out var items)) items = new();
            foreach (var dict in this._dicts)
            {
                lock (dict) items.AddRange(dict);
                foreach (var item in items) yield return item;
                items.Clear();
            }
            s_enumeratorBuffers.Add(items);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            ref var dictValue = ref this.Find(item.Key);
            return !Unsafe.IsNullRef(ref dictValue) && EqualityComparer<TValue>.Default.Equals(item.Value, dictValue);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this._dicts.Length == 0) return false;
            var hash = this._comparer.GetHashCode(item.Key);
            while (true)
            {
                var dicts = this._dicts;
                var fastModMultiplier = this._fastModMultiplier;

                var index = HashToIndex(hash, dicts.Length, fastModMultiplier);
                if (index > dicts.Length) continue; // retry
                var dict = dicts[index];
                bool result;
                lock (dict)
                {
                    if (this._dicts != dicts || this._fastModMultiplier != fastModMultiplier) continue; // retry
                    result = ((ICollection<KeyValuePair<TKey, TValue>>)dict).Remove(item);
                }
                if (result) Interlocked.Decrement(ref this._count);
                return result;
            }
        }
    }
}
