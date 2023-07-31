using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    /// <summary>Non-snapshotting dictionary Keys</summary>
    public sealed class ReadOnlyDictionaryKeys<TKey, TValue> : ISet<TKey>, IReadOnlySet<TKey>
    {
        private readonly IReadOnlyDictionary<TKey, TValue> _backingDictionary;

        public ReadOnlyDictionaryKeys(IReadOnlyDictionary<TKey, TValue> backingDictionary)
        {
            this._backingDictionary = backingDictionary;
        }

        public int Count => this._backingDictionary.Count;

        public bool IsReadOnly => true;

        public void Add(TKey item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(TKey item) => this._backingDictionary.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            foreach (var item in this) array[arrayIndex++] = item;
        }

        public void ExceptWith(IEnumerable<TKey> other) => throw new NotSupportedException();

        public IEnumerator<TKey> GetEnumerator() => this._backingDictionary.Select(kvp => kvp.Key).GetEnumerator();

        public void IntersectWith(IEnumerable<TKey> other) => throw new NotSupportedException();

        public bool IsProperSubsetOf(IEnumerable<TKey> other)
        {
            return this.Count < other.Count() && this.IsSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<TKey> other)
        {
            return other.Count() < this.Count && this.IsSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<TKey> other)
        {
            return this.All(item => other.Contains(item));
        }

        public bool IsSupersetOf(IEnumerable<TKey> other)
        {
            return other.All(this.Contains);
        }

        public bool Overlaps(IEnumerable<TKey> other)
        {
            return this.Any(item => other.Contains(item));
        }

        public bool Remove(TKey item) => throw new NotSupportedException();

        public bool SetEquals(IEnumerable<TKey> other)
        {
            var otherItems = other.ToHashSet();
            if (this.Count != otherItems.Count) return false;
            return this.All(otherItems.Contains);
        }

        public void SymmetricExceptWith(IEnumerable<TKey> other) => throw new NotSupportedException();

        public void UnionWith(IEnumerable<TKey> other) => throw new NotSupportedException();

        bool ISet<TKey>.Add(TKey item) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
