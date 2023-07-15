using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    /// <summary>Non-snapshotting dictionary Keys</summary>
    public sealed class DictionaryKeys<TKey, TValue> : ICollection<TKey>
    {
        private readonly IDictionary<TKey, TValue> _backingDictionary;

        public DictionaryKeys(IDictionary<TKey, TValue> backingDictionary)
        {
            this._backingDictionary = backingDictionary;
        }

        public int Count => this._backingDictionary.Count;

        public bool IsReadOnly => this._backingDictionary.IsReadOnly;

        public void Add(TKey item) => this._backingDictionary.Add(item, default!);

        public void Clear() => this._backingDictionary.Clear();

        public bool Contains(TKey item) => this._backingDictionary.ContainsKey(item);

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            foreach (var (key, value) in this._backingDictionary)
            {
                if (arrayIndex >= array.Length) return;
                array[arrayIndex++] = key;
            }
        }

        public IEnumerator<TKey> GetEnumerator() => this._backingDictionary.Select(kvp => kvp.Key).GetEnumerator();

        public bool Remove(TKey item) => this._backingDictionary.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
