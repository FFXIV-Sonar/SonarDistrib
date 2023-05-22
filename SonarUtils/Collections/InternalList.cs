using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SonarUtils.Collections
{
    public struct InternalList<T> : IEnumerable<T>, ICloneable
    {
        public T[] InternalArray { get; set; } = Array.Empty<T>();
        private int _count;

        public int Count
        {
            get => this._count;
            set
            {
                this.EnsureCapacity(value);
                this._count = value;
            }
        }

        public int Capacity
        {
            get => this.InternalArray.Length;
            set
            {
                try
                {
                    if (value < this._count) throw new ArgumentOutOfRangeException(nameof(this.Capacity), $"{nameof(this.Capacity)} < {nameof(this.Count)}");
                    if (value == this.InternalArray.Length) return;
                    var newArray = value > 0 ? new T[value] : Array.Empty<T>();
                    this.InternalArray.AsSpan(0, this._count).CopyTo(newArray);
                    this.InternalArray = newArray;
                }
                catch (Exception ex) { Debugger.Break(); }
            }
        }

        public InternalList() { }
        public InternalList(int capacity) { this.Capacity = capacity; }
        public InternalList(IEnumerable<T> items) { this.AddRange(items); }
        public InternalList(IEnumerable<T> items, int capacity) { this.Capacity = capacity;  this.AddRange(items); }

        public void Add(T item)
        {
            this.EnsureCapacity(this._count + 1);
            this.InternalArray[this._count++] = item;
        }
        
        public void AddRange(IEnumerable<T> items)
        {
            if (items.TryGetNonEnumeratedCount(out var count)) this.EnsureCapacity(count);
            foreach (var item in items) this.Add(item);
        }

        public void AddRange(T[] items)
        {
            this.AddRange(items.AsSpan());
        }

        public void AddRange(ReadOnlySpan<T> items)
        {
            var span = this.AddRangeHelper(items.Length);
            items.CopyTo(span);
        }

        public Span<T> AddRangeHelper(int count)
        {
            this.EnsureCapacity(this._count + count);
            return this.InternalArray.AsSpan(MathUtils.AddReturnOriginal(ref this._count, count), count);
        }

        public void Insert(int index, T item)
        {
            this.InsertRangeHelper(index, 1)[0] = item;
        }

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


        public Span<T> InsertRangeHelper(int index, int count)
        {
            this.EnsureCapacity(this._count + count);
            var srcSpan = this.InternalArray.AsSpan(index, this.Count - index);
            var dstSpan = this.InternalArray.AsSpan(index + count, this.Count - index);
            srcSpan.CopyTo(dstSpan); // Overlaps checked internally using Buffer.Memmove
            this._count += count;
            return this.InternalArray.AsSpan(index, count);
        }

        public void CopyTo(T[] array)
        {
            this.InternalArray.AsSpan(0, this.Count).CopyTo(array);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.InternalArray.AsSpan(0, this.Count).CopyTo(array.AsSpan(arrayIndex));
        }

        public T? Find(Predicate<T> predicate)
        {
            var index = this.FindIndex(predicate);
            if (index == -1) return default;
            return this.InternalArray[index];
        }

        public int FindIndex(Predicate<T> predicate)
        {
            for (var index = 0; index < this._count; index++)
            {
                if (predicate(this.InternalArray[index])) return index;
            }
            return -1;
        }

        public IEnumerable<int> FindAll(Predicate<T> predicate)
        {
            var count = this._count;
            for (var index = 0; index < count; index++)
            {
                if (predicate(this.InternalArray[index])) yield return index;
            }
        }

        public int IndexOf(T item) => this.FindIndex(i => EqualityComparer<T>.Default.Equals(i, item));

        public bool Contains(T item) => this.IndexOf(item) != -1;

        public bool Remove(T item)
        {
            var index = this.FindIndex(i => EqualityComparer<T>.Default.Equals(i, item));
            if (index == -1) return false;
            this.RemoveAt(index);
            return true;
        }

        public Span<T> AsSpan() => this.AsSpan(0, this.Count);

        public Span<T> AsSpan(int index, int length) => this.InternalArray.AsSpan(index, length);

        public void RemoveAt(int index) => this.RemoveRange(index, 1);

        public void RemoveRange(int index, int length) => this.RemoveRange(index, length, true);

        public void RemoveRange(int index, int length, bool cleanUp)
        {
            var span = this.InternalArray.AsSpan(index + length, this._count - length);
            span.CopyTo(this.InternalArray.AsSpan(index));
            this._count -= length;
            if (cleanUp) this.CleanUp(length);
        }

        public ref T this[int index] => ref this.InternalArray[index];

        public void EnsureCapacity(int count)
        {
            if (count > this.InternalArray.Length)
            {
                this.Capacity = MathUtils.AlignPowerOfTwo(count);
            }
        }

        public void CleanUp()
        {
            var span = this.InternalArray.AsSpan(this._count);
            span.Clear();
        }

        public void CleanUp(int count)
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
                var capacity = MathUtils.AlignPowerOfTwo(count);
                if (capacity < this.Capacity) this.Capacity = capacity;
            }
            this.Count = count;
        }

        public InternalList<T> Clone()
        {
            var list = new InternalList<T>(this.Capacity);
            this.InternalArray.AsSpan(0, this.Count).CopyTo(list.InternalArray);
            return list;
        }

        object ICloneable.Clone() => this.Clone();

        public InternalList<T> CloneAndTrim()
        {
            var list = new InternalList<T>(this.Count);
            this.InternalArray.AsSpan(0, this.Count).CopyTo(list.InternalArray);
            return list;
        }

        public Span<T>.Enumerator GetEnumerator()
        {
            return this.InternalArray.AsSpan(0, this.Count).GetEnumerator();
        }

        public IEnumerable<T> AsEnumerable() => this;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (var index = 0; index < this._count; index++) yield return this.InternalArray[index];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
    }
}
