using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SonarUtils.Collections
{
    public sealed class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public TValue DefaultValue { get; set; }

        public ICollection<TKey> Keys => this._dictionary.Keys;

        public ICollection<TValue> Values => this._dictionary.Values;

        public int Count => this._dictionary.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get
            {
                if (this._dictionary.TryGetValue(key, out var result)) return result;
                return this.DefaultValue;
            }
            set => this._dictionary[key] = value;
        }

        public DefaultDictionary(in TValue defaultValue) : this(defaultValue, new Dictionary<TKey, TValue>()) { }

        public DefaultDictionary(in TValue defaultValue, IEnumerable<KeyValuePair<TKey, TValue>> items) : this(defaultValue, new Dictionary<TKey, TValue>(items)) { }

        public DefaultDictionary(in TValue defaultValue, IEqualityComparer<TKey>? comparer) : this(defaultValue, new Dictionary<TKey, TValue>(comparer)) { }

        public DefaultDictionary(in TValue defaultValue, IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey>? comparer) : this(defaultValue, new Dictionary<TKey, TValue>(items, comparer)) { }

        public DefaultDictionary(in TValue defaultValue, int capacity) : this(defaultValue, new Dictionary<TKey, TValue>(capacity)) { }

        public DefaultDictionary(in TValue defaultValue, int capacity, IEqualityComparer<TKey>? comparer) : this(defaultValue, new Dictionary<TKey, TValue>(capacity, comparer)) { }

        internal DefaultDictionary(in TValue defaultValue, Dictionary<TKey, TValue> dictionary)
        {
            this._dictionary = dictionary;
            this.DefaultValue = defaultValue;
        }

        public void Add(TKey key, TValue value) => this._dictionary.Add(key, value);

        public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

        public bool Remove(TKey key) => this._dictionary.Remove(key);

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            var result = !this._dictionary.TryGetValue(key, out value);
            if (!result) value = this.DefaultValue;
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
