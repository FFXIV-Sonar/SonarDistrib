using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sonar.Collections
{
    public sealed partial class TransformDictionary<TKey, TValue, TSourceValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private sealed class ValueCollection : ICollection<TValue>
        {
            private readonly TransformDictionary<TKey, TValue, TSourceValue> _dictionary;

            public ValueCollection(TransformDictionary<TKey, TValue, TSourceValue> dictionary)
            {
                this._dictionary = dictionary;
            }

            public int Count => this._dictionary.Count;

            public bool IsReadOnly => this._dictionary.IsReadOnly;

            public void Add(TValue item) => ThrowNotSupportedException();

            public void Clear() => this._dictionary.Clear();

            public bool Contains(TValue item)
            {
                ThrowNotSupportedException();
                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                foreach (var item in this)
                {
                    array[arrayIndex++] = item;
                    if (arrayIndex == array.Length) return;
                }
            }

            public IEnumerator<TValue> GetEnumerator() => this._dictionary.Select(i => i.Value).GetEnumerator();

            public bool Remove(TValue item)
            {
                ThrowNotSupportedException();
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}
