using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sonar.Collections
{
    public sealed partial class TransformDictionary<TKey, TValue, TSourceValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private sealed class KeyCollection : ICollection<TKey>
        {
            private readonly TransformDictionary<TKey, TValue, TSourceValue> _dictionary;

            public KeyCollection(TransformDictionary<TKey, TValue, TSourceValue> dictionary)
            {
                this._dictionary = dictionary;
            }

            public int Count => this._dictionary.Count;

            public bool IsReadOnly => this._dictionary.IsReadOnly;

            public void Add(TKey item) => ThrowNotSupportedException();

            public void Clear() => this._dictionary.Clear();

            public bool Contains(TKey item) => this._dictionary.ContainsKey(item);

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                foreach (var item in this)
                {
                    array[arrayIndex++] = item;
                    if (arrayIndex == array.Length) return;
                }
            }

            public IEnumerator<TKey> GetEnumerator() => this._dictionary.Select(i => i.Key).GetEnumerator();

            public bool Remove(TKey item) => this._dictionary.Remove(item);

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}
