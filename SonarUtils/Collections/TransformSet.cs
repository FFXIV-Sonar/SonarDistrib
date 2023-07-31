using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    public sealed class TransformSet<T, TSource> : ISet<T>, IReadOnlySet<T>
    {
        private readonly ISet<TSource> _source;
        private readonly Func<TSource, T> _outputTransform;
        private readonly Func<T, TSource> _inputTransform;
        private readonly bool _readOnly;

        public TransformSet(ISet<TSource> source, Func<TSource, T> outputTransform, Func<T, TSource> inputTransform, bool isReadOnly = false) 
        {
            this._source = source;
            this._outputTransform = outputTransform;
            this._inputTransform = inputTransform;
            this._readOnly = isReadOnly;
        }

        public int Count => this._source.Count;

        public bool IsReadOnly => this._readOnly || this._source.IsReadOnly;

        private void ThrowNotSupportedIfReadOnly()
        {
            if (this.IsReadOnly) ThrowNotSupported();
        }

        [DoesNotReturn]
        private static void ThrowNotSupported() => throw new NotSupportedException();

        public bool Add(T item)
        {
            this.ThrowNotSupportedIfReadOnly();
            return this._source.Add(this._inputTransform(item));
        }

        public void Clear()
        {
            this.ThrowNotSupportedIfReadOnly();
            this._source.Clear();
        }

        public bool Contains(T item) => this._source.Contains(this._inputTransform(item));

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this) array[arrayIndex++]= item;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other) this.Remove(item);
        }

        public IEnumerator<T> GetEnumerator() => this._source.Select(i => this._outputTransform(i)).GetEnumerator();

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
            return other.All(item => this.Contains(item));
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return this.Any(item => other.Contains(item));
        }

        public bool Remove(T item)
        {
            this.ThrowNotSupportedIfReadOnly();
            return this._source.Remove(this._inputTransform(item));
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            var otherItems = other.ToHashSet();
            if (this.Count != otherItems.Count) return false;
            return this.All(otherItems.Contains);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            this.ThrowNotSupportedIfReadOnly(); // avoid heavy operations
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
            if (!this.Add(item)) throw new ArgumentException($"{nameof(item)} already exists");
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public static class TransformSet
    {
        public static TransformSet<T, TSource> Create<T, TSource>(ISet<TSource> source, Func<TSource, T> outputTransform, Func<T, TSource> inputTransform, bool readOnly = false)
        {
            return new TransformSet<T, TSource>(source, outputTransform, inputTransform, readOnly);
        }
    }
}
