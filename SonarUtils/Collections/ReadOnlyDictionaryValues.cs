using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    /// <summary>Non-snapshotting dictionary Values</summary>
    public sealed class ReadOnlyDictionaryValues<TKey, TValue> : ICollection<TValue>, IReadOnlyCollection<TValue>
    {
        private readonly IReadOnlyDictionary<TKey, TValue> _backingDictionary;

        public ReadOnlyDictionaryValues(IReadOnlyDictionary<TKey, TValue> backingDictionary)
        {
            this._backingDictionary = backingDictionary;
        }

        public int Count => this._backingDictionary.Count;

        public bool IsReadOnly => true;

        public void Add(TValue item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(TValue item) => this._backingDictionary.Select(kvp => kvp.Value).Contains(item);

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            foreach (var item in this) array[arrayIndex++] = item;
        }

        public IEnumerator<TValue> GetEnumerator() => this._backingDictionary.Select(kvp => kvp.Value).GetEnumerator();

        public bool Remove(TValue item) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
