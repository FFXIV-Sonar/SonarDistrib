using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    /// <summary>Concurrent <see cref="HashSet{T}"/> that uses an array of <see cref="HashSet{T}"/> as its internal structure</summary>
    /// <remarks>
    /// <para>Extensive benchmarking and testing were performed in the making of <see cref="ConcurrentHashSetSlim{T}"/> in Sonar indexes. No <see cref="HashSet{T}"/>s were harmed in the process.</para>
    /// <para>Concurrency is achieved by the use of locks on a per <see cref="HashSet{T}"/> basis</para>
    /// <para>Concurrency level increases as this set grows, in prime counts</para>
    /// </remarks>
    public sealed class ConcurrentHashSetSlim<T> : ISet<T>, IReadOnlySet<T> where T : notnull
    {
        private static readonly ConcurrentBag<List<T>> s_enumeratorBuffers = new();
        private HashSet<T>[] _sets = Array.Empty<HashSet<T>>();
        private readonly IEqualityComparer<T> _comparer;
        private ulong _fastModMultiplier;
        private int _count;

        public int Count => this._count;

        public bool IsReadOnly => false;

        public ConcurrentHashSetSlim() : this(EqualityComparer<T>.Default) { /* Empty */ }

        public ConcurrentHashSetSlim(IEqualityComparer<T> comparer)
        {
            this._comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCountTooLarge(int count, int length) => count >= length * length + length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int HashToIndex(int hash, int length, ulong fastModMultiplier) =>
            (int)HashHelpers.FastMod(uint.RotateRight((uint)hash, 16), (uint)length, fastModMultiplier);

        private static void AcquireAllLocks(HashSet<T>[] sets)
        {
            foreach (var set in sets) Monitor.Enter(set);
        }

        private static void ReleaseAllLocks(HashSet<T>[] sets)
        {
            foreach (var set in sets) Monitor.Exit(set);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNeeded()
        {
            if (IsCountTooLarge(this._count, this._sets.Length)) this.ResizeIfNeededSlow();
        }

        private void ResizeIfNeededSlow()
        {
            while (true)
            {
                var count = this._count;
                var sets = this._sets;
                if (!IsCountTooLarge(count, sets.Length)) return;

                var newLength = MathUtils.FindPrime(sets.Length * 2);
                if (newLength < sets.Length) return;

                lock (sets)
                {
                    if (this._sets != sets) continue; // retry
                    try
                    {
                        AcquireAllLocks(sets);
                        count = this._count;
                        if (!IsCountTooLarge(count, sets.Length)) return;

                        newLength = MathUtils.FindPrime(sets.Length * 2);
                        if (newLength <= sets.Length) return;
                        //Console.WriteLine($"sets resize: {sets.Length} => {newLength}");

                        var newSets = new HashSet<T>[newLength];
                        var newCapacity = newLength + (int)Math.Sqrt(newLength);
                        foreach (ref var set in newSets.AsSpan()) set = new(newCapacity, comparer: this._comparer);

                        var fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newLength);
                        foreach (var set in sets)
                        {
                            foreach (var item in set) newSets[HashToIndex(this._comparer.GetHashCode(item), newLength, fastModMultiplier)].Add(item);
                        }
                        this._fastModMultiplier = fastModMultiplier;
                        this._sets = newSets;
                    }
                    finally
                    {
                        ReleaseAllLocks(sets);
                    }
                }
            }
        }

        public bool Add(T item)
        {
            this.ResizeIfNeeded();
            var hash = this._comparer.GetHashCode(item);
            while (true)
            {
                var sets = this._sets;
                var fastModMultiplier = this._fastModMultiplier;
                var index = HashToIndex(hash, sets.Length, fastModMultiplier);
                if (index > sets.Length) continue; // retry
                var set = sets[index];
                bool result;
                lock (set)
                {
                    if (this._sets != sets || this._fastModMultiplier != fastModMultiplier) continue; // retry
                    result = set.Add(item);
                }
                if (result) Interlocked.Increment(ref this._count);
                return result;
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other) this.Remove(item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            foreach (var item in this)
            {
                if (!other.Contains(item)) this.Remove(item);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return this.Count < other.Count() && this.IsSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return other.Count() < this.Count && this.IsSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return this.All(item => other.Contains(item));
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return other.All(this.Contains);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return this.Any(item => other.Contains(item));
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            var otherItems = other.ToHashSet();
            if (this.Count != otherItems.Count) return false;
            return this.All(otherItems.Contains);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var both = this.Intersect(other).ToList();
            foreach (var item in other) this.Add(item);
            foreach (var item in both) this.Remove(item);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other) this.Add(item);
        }

        void ICollection<T>.Add(T item)
        {
            if (!this.Add(item)) ThrowDuplicateException(nameof(item));
        }

        private static void ThrowDuplicateException(string paramName) => throw new ArgumentException($"{paramName} already exists", paramName);

        public void Clear()
        {
            while (true)
            {
                var sets = this._sets;
                lock (sets)
                {
                    if (this._sets != sets) continue; //retry
                    foreach (var set in sets)
                    {
                        int count;
                        lock (set)
                        {
                            count = set.Count;
                            set.Clear();
                        }
                        Interlocked.Add(ref this._count, -count);
                    }
                    return;
                }
            }
        }

        public bool Contains(T item)
        {
            if (this._sets.Length == 0) return false;
            var hash = this._comparer.GetHashCode(item);
            while (true)
            {
                var sets = this._sets;
                var fastModMultiplier = this._fastModMultiplier;
                var index = HashToIndex(hash, sets.Length, fastModMultiplier);
                if (index > sets.Length) continue; // retry
                var set = sets[index];
                lock (set)
                {
                    if (this._sets != sets || this._fastModMultiplier != fastModMultiplier) continue; // retry
                    return set.Contains(item);
                }
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this) array[arrayIndex++] = item;
        }

        public bool Remove(T item)
        {
            if (this._sets.Length == 0) return false;
            var hash = this._comparer.GetHashCode(item);
            while (true)
            {
                var sets = this._sets;
                var fastModMultiplier = this._fastModMultiplier;
                var index = HashToIndex(hash, sets.Length, fastModMultiplier);
                if (index > sets.Length) continue; // retry
                var set = sets[index];
                bool result;
                lock (set)
                {
                    if (this._sets != sets || this._fastModMultiplier != fastModMultiplier) continue; // retry
                    result = set.Remove(item);
                }
                if (result) Interlocked.Decrement(ref this._count);
                return result;
            }
        }

        public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
        {
            if (this._sets.Length == 0)
            {
                actualValue = default;
                return false;
            }
            var hash = this._comparer.GetHashCode(equalValue);
            while (true)
            {
                var sets = this._sets;
                var fastModMultiplier = this._fastModMultiplier;
                var index = HashToIndex(hash, sets.Length, fastModMultiplier);
                if (index >= sets.Length) continue; // retry
                var set = sets[index];
                lock (set)
                {
                    if (this._sets != sets || this._fastModMultiplier != fastModMultiplier) continue; // retry
                    return set.TryGetValue(equalValue, out actualValue);
                }
            }
        }

        public bool TryGetValueUnsafe(T equalValue, [MaybeNullWhen(false)] out T actualValue)
        {
            if (this._sets.Length == 0)
            {
                actualValue = default;
                return false;
            }
            var hash = this._comparer.GetHashCode(equalValue);

            var sets = this._sets;
            var fastModMultiplier = this._fastModMultiplier;

            var index = HashToIndex(hash, sets.Length, fastModMultiplier);
            if (index >= sets.Length) return this.TryGetValue(equalValue, out actualValue); // retry
            var set = sets[index];

            return set.TryGetValue(equalValue, out actualValue);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!s_enumeratorBuffers.TryTake(out var items)) items = new();
            foreach (var set in this._sets)
            {
                lock (set) items.AddRange(set);
                foreach (var item in items) yield return item;
                items.Clear();
            }
            s_enumeratorBuffers.Add(items);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
