using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    public sealed class DefaultDelegatedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> DefaultDelegate { get; set; }

        public ICollection<TKey> Keys => this._dictionary.Keys;

        public ICollection<TValue> Values => this._dictionary.Values;

        public int Count => this._dictionary.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get
            {
                if (this._dictionary.TryGetValue(key, out var result)) return result;
                return this.DefaultDelegate(this, key);
            }
            set => this._dictionary[key] = value;
        }

        public DefaultDelegatedDictionary(in Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> defaultDelegate) : this(defaultDelegate, new Dictionary<TKey, TValue>()) { }

        public DefaultDelegatedDictionary(in Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> defaultDelegate, IEnumerable<KeyValuePair<TKey, TValue>> items) : this(defaultDelegate, new Dictionary<TKey, TValue>(items)) { }

        public DefaultDelegatedDictionary(in Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> defaultDelegate, IEqualityComparer<TKey>? comparer) : this(defaultDelegate, new Dictionary<TKey, TValue>(comparer)) { }

        public DefaultDelegatedDictionary(in Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> defaultDelegate, IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey>? comparer) : this(defaultDelegate, new Dictionary<TKey, TValue>(items, comparer)) { }

        public DefaultDelegatedDictionary(in Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> defaultDelegate, int capacity) : this(defaultDelegate, new Dictionary<TKey, TValue>(capacity)) { }

        public DefaultDelegatedDictionary(in Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> defaultDelegate, int capacity, IEqualityComparer<TKey>? comparer) : this(defaultDelegate, new Dictionary<TKey, TValue>(capacity, comparer)) { }

        internal DefaultDelegatedDictionary(in Func<DefaultDelegatedDictionary<TKey, TValue>, TKey, TValue> defaultDelegate, Dictionary<TKey, TValue> dictionary)
        {
            this._dictionary = dictionary;
            this.DefaultDelegate = defaultDelegate;
        }

        public void Add(TKey key, TValue value) => this._dictionary.Add(key, value);

        public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

        public bool Remove(TKey key) => this._dictionary.Remove(key);

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            var result = !this._dictionary.TryGetValue(key, out value);
            if (!result) value = this.DefaultDelegate(this, key);
            return result;
        }

        public void Add(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Add(item);

        public void Clear() => this._dictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => this._dictionary.Contains(item);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Remove(item);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this._dictionary.GetEnumerator();
    }
}
