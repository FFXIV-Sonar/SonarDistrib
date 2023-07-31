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
    public sealed class TransformReadOnlySet<T, TSource> : ISet<T>, IReadOnlySet<T>
    {
        private readonly IReadOnlySet<TSource> _source;
        private readonly Func<TSource, T> _outputTransform;
        private readonly Func<T, TSource> _inputTransform;

        public TransformReadOnlySet(IReadOnlySet<TSource> source, Func<TSource, T> outputTransform, Func<T, TSource> inputTransform)
        {
            this._source = source;
            this._outputTransform = outputTransform;
            this._inputTransform = inputTransform;
        }

        public int Count => this._source.Count;

        public bool IsReadOnly => true;

        public bool Add(T item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(T item) => this._source.Contains(this._inputTransform(item));

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this) array[arrayIndex++] = item;
        }

        public void ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

        public IEnumerator<T> GetEnumerator() => this._source.Select(i => this._outputTransform(i)).GetEnumerator();

        public void IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();

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

        public bool Remove(T item) => throw new NotSupportedException();

        public bool SetEquals(IEnumerable<T> other)
        {
            var otherItems = other.ToHashSet();
            if (this.Count != otherItems.Count) return false;
            return this.All(otherItems.Contains);
        }

        public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

        public void UnionWith(IEnumerable<T> other) => throw new NotSupportedException();

        void ICollection<T>.Add(T item) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public static class TransformReadOnlySet
    {
        public static TransformReadOnlySet<T, TSource> Create<T, TSource>(IReadOnlySet<TSource> source, Func<TSource, T> outputTransform, Func<T, TSource> inputTransform)
        {
            return new TransformReadOnlySet<T, TSource>(source, outputTransform, inputTransform);
        }
    }
}
