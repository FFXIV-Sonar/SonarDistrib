using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NonBlocking
{
    public partial class ConcurrentDictionary<TKey, TValue>
    {
        public sealed class ValueCollection : ICollection<TValue>, ICollection
        {
            private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
            public ValueCollection(ConcurrentDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public int Count => _dictionary.Count;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => ((ICollection)this._dictionary).SyncRoot;

            public void Add(TValue item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TValue item) => this.Any(i => EqualityComparer<TValue>.Default.Equals(item, i));

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                foreach (var item in this)
                {
                    array[arrayIndex++] = item;
                    if (arrayIndex == array.Length) return;
                }
            }

            public void CopyTo(Array array, int index) => throw new NotSupportedException();

            public IEnumerator<TValue> GetEnumerator() => _dictionary.Select(kvp => kvp.Value).GetEnumerator();

            public bool Remove(TValue item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
