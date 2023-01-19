using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NonBlocking
{
    public partial class ConcurrentDictionary<TKey, TValue>
    {
        public sealed class KeyCollection : ICollection<TKey>, ICollection
        {
            private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
            public KeyCollection(ConcurrentDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
            }

            public int Count => _dictionary.Count;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => this._dictionary;

            public void Add(TKey item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TKey item) => _dictionary.ContainsKey(item);

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                foreach (var item in this)
                {
                    array[arrayIndex++] = item;
                    if (arrayIndex == array.Length) return;
                }
            }

            public void CopyTo(Array array, int index) => throw new NotSupportedException();

            public IEnumerator<TKey> GetEnumerator() => _dictionary.Select(kvp => kvp.Key).GetEnumerator();

            public bool Remove(TKey item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
