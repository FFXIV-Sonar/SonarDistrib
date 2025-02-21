using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SonarUtils.Collections
{
    public struct InternalList<T> : IEnumerable<T>, ICloneable // NOTE: Do not add IList<T>
    {
        /// <summary>Internal <see cref="Array"/> used by this <see cref="InternalList{T}"/></summary>
        /// <remarks>Setting an array smaller than <see cref="Count"/> will result in undefined behavior</remarks>
        public T[] InternalArray { get; set; } = Array.Empty<T>();
        private int _count;

        /// <summary>Item count</summary>
        public int Count
        {
            readonly get => this._count;
            set
            {
                this.EnsureCapacity(value);
                this._count = value;
            }
        }

        /// <summary><see cref="InternalArray"/> Length</summary>
        public int Capacity
        {
            readonly get => this.InternalArray.Length;
            set
            {
                if (value < this._count) throw new ArgumentOutOfRangeException(nameof(this.Capacity), $"{nameof(this.Capacity)} < {nameof(this.Count)}");
                if (value == this.InternalArray.Length) return;
                var newArray = value > 0 ? new T[value] : Array.Empty<T>();
                this.InternalArray.AsSpan(0, this._count).CopyTo(newArray);
                this.InternalArray = newArray;
            }
        }

        /// <summary>Construct an empty list</summary>
        public InternalList() { }

        /// <summary>Construct an empty list with an initial <see cref="Capacity"/></summary>
        /// <param name="capacity">Initial capacity</param>
        public InternalList(int capacity) { this.Capacity = capacity; }
        
        /// <summary>Construct a list with an initial set of <paramref name="items"/></summary>
        /// <param name="items">Items</param>
        public InternalList(IEnumerable<T> items) { this.AddRange(items); }

        /// <summary>Construct a list with an initial set of <paramref name="items"/> and <paramref name="capacity"/></summary>
        /// <param name="items">Items</param>
        /// <param name="capacity">Initial capacity</param>
        public InternalList(IEnumerable<T> items, int capacity) { this.Capacity = capacity;  this.AddRange(items); }

        /// <summary>Adds an <paramref name="item"/></summary>
        /// <param name="item">Item to add</param>
        public void Add(T item)
        {
            this.EnsureCapacity(this._count + 1);
            this.InternalArray[this._count++] = item;
        }

        /// <summary>Adds <paramref name="items"/> to the list</summary>
        /// <param name="items">Items to add</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items.TryGetNonEnumeratedCount(out var count)) this.EnsureCapacity(count);
            foreach (var item in items) this.Add(item);
        }

        /// <summary>Adds <paramref name="items"/> to the list</summary>
        /// <param name="items">Items to add</param>
        public void AddRange(T[] items)
        {
            this.AddRange(items.AsSpan());
        }

        /// <summary>Adds <paramref name="items"/> to the list</summary>
        /// <param name="items">Items to add</param>
        public void AddRange(ReadOnlySpan<T> items)
        {
            var span = this.AddRangeHelper(items.Length);
            items.CopyTo(span);
        }

        /// <summary>Adds space for <paramref name="count"/> items and return it as a <see cref="Span{T}"/></summary>
        /// <param name="count">Item count</param>
        public Span<T> AddRangeHelper(int count)
        {
            this.EnsureCapacity(this._count + count);
            return this.InternalArray.AsSpan(MathUtils.AddReturnOriginal(ref this._count, count), count);
        }

        /// <summary>Insert an <paramref name="item"/> at a specified <paramref name="index"/></summary>
        /// <param name="index">Index to add the item into</param>
        /// <param name="item">Item to add</param>
        public void Insert(int index, T item)
        {
            this.InsertRangeHelper(index, 1)[0] = item;
        }

        /// <summary>Insert a <paramref name="items"/> at a specified <paramref name="index"/></summary>
        /// <param name="index">Index to add the items into</param>
        /// <param name="item">Item to add</param>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            if (!items.TryGetNonEnumeratedCount(out var count))
            {
                this.InsertRange(index, items.ToList());
                return;
            }

            var span = this.InsertRangeHelper(index, count);
            index = 0; // Some trickery
            foreach (var item in items)
            {
                span[index++] = item;
            }
        }


        /// <summary>Insert space for <paramref name="count"/> items and return it as a <see cref="Span{T}"/></summary>
        /// <param name="index">Index to add the items into</param>
        /// <param name="count">Item count</param>
        public Span<T> InsertRangeHelper(int index, int count)
        {
            this.EnsureCapacity(this._count + count);
            var srcSpan = this.InternalArray.AsSpan(index, this.Count - index);
            var dstSpan = this.InternalArray.AsSpan(index + count, this.Count - index);
            srcSpan.CopyTo(dstSpan); // Overlaps checked internally using Buffer.Memmove
            this._count += count;
            return this.InternalArray.AsSpan(index, count);
        }

        public readonly void CopyTo(T[] array)
        {
            this.InternalArray.AsSpan(0, this.Count).CopyTo(array);
        }
        public readonly void CopyTo(T[] array, int arrayIndex)
        {
            this.InternalArray.AsSpan(0, this.Count).CopyTo(array.AsSpan(arrayIndex));
        }

        public readonly T? Find(Predicate<T> predicate)
        {
            var index = this.FindIndex(predicate);
            if (index == -1) return default;
            return this.InternalArray[index];
        }

        public readonly int FindIndex(Predicate<T> predicate)
        {
            for (var index = 0; index < this._count; index++)
            {
                if (predicate(this.InternalArray[index])) return index;
            }
            return -1;
        }

        public readonly IEnumerable<int> FindAll(Predicate<T> predicate)
        {
            var count = this._count;
            for (var index = 0; index < count; index++)
            {
                if (predicate(this.InternalArray[index])) yield return index;
            }
        }

        public readonly int IndexOf(T item) => this.FindIndex(i => EqualityComparer<T>.Default.Equals(i, item));

        public readonly bool Contains(T item) => this.IndexOf(item) != -1;

        public bool Remove(T item)
        {
            var index = this.FindIndex(i => EqualityComparer<T>.Default.Equals(i, item));
            if (index == -1) return false;
            this.RemoveAt(index);
            return true;
        }

        public readonly Span<T> AsSpan() => this.AsSpan(0, this.Count);

        public readonly Span<T> AsSpan(int index, int length) => this.InternalArray.AsSpan(index, length);

        public void RemoveAt(int index) => this.RemoveRange(index, 1);

        public void RemoveRange(int index, int length) => this.RemoveRange(index, length, true);

        public void RemoveRange(int index, int length, bool cleanUp)
        {
            var span = this.InternalArray.AsSpan(index + length, this._count - length);
            span.CopyTo(this.InternalArray.AsSpan(index));
            this._count -= length;
            if (cleanUp) this.CleanUp(length);
        }

        public readonly ref T this[int index] => ref this.InternalArray[index];

        public void EnsureCapacity(int count)
        {
            if (count > this.InternalArray.Length)
            {
                this.Capacity = (int)BitOperations.RoundUpToPowerOf2((uint)count);
            }
        }

        public readonly void CleanUp()
        {
            var span = this.InternalArray.AsSpan(this._count);
            span.Clear();
        }

        public readonly void CleanUp(int count)
        {
            var span = this.InternalArray.AsSpan(this._count, count);
            span.Clear();
        }

        public void Trim()
        {
            this.Capacity = this.Count;
        }

        public void Clear(bool reset = false) => this.Resize(0, reset);

        public void Resize(int count, bool allowReduceCapacity = false)
        {
            if (count < this._count && allowReduceCapacity)
            {
                var capacity = (int)BitOperations.RoundUpToPowerOf2((uint)count);
                if (capacity < this.Capacity) this.Capacity = capacity;
            }
            this.Count = count;
        }

        public readonly override int GetHashCode()
        {
            var count = this.Count;
            var hash = count.GetHashCode();
            for (var index = 0; index < count; index++)
            {
                hash ^= this[index]?.GetHashCode() ?? 0;
            }
            return hash;
        }

        public readonly bool Equals(InternalList<T> other)
        {
            var count = this.Count;
            if (count != other.Count) return false;
            var comparer = EqualityComparer<T>.Default;
            for (var index = 0; index < count; index++)
            {
                if (!comparer.Equals(this[index], other[index])) return false;
            }
            return true;
        }

        public readonly override bool Equals(object? obj) => obj is InternalList<T> list && this.Equals(list);

        public readonly InternalList<T> Clone()
        {
            var list = new InternalList<T>(this.Capacity);
            this.InternalArray.AsSpan(0, this.Count).CopyTo(list.InternalArray);
            list.Count = this.Count;
            return list;
        }

        readonly object ICloneable.Clone() => this.Clone();

        public readonly InternalList<T> CloneAndTrim()
        {
            var list = new InternalList<T>(this.Count);
            this.InternalArray.AsSpan(0, this.Count).CopyTo(list.InternalArray);
            list.Count = this.Count;
            return list;
        }

        public readonly Span<T>.Enumerator GetEnumerator()
        {
            return this.InternalArray.AsSpan(0, this.Count).GetEnumerator();
        }

        public readonly IEnumerable<T> AsEnumerable() => this;

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (var index = 0; index < this._count; index++) yield return this.InternalArray[index];
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public static bool operator ==(InternalList<T> left, InternalList<T> right) => left.Equals(right);

        public static bool operator !=(InternalList<T> left, InternalList<T> right) => !(left == right);
    }
}
