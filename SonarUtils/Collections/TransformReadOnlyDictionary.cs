using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SonarUtils.Collections
{
    /// <summary>
    /// A dictionary that automatically transform values
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <typeparam name="TSourceValue">Backing dictionary's value type</typeparam>
    public sealed partial class TransformReadOnlyDictionary<TKey, TValue, TSourceValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly IReadOnlyDictionary<TKey, TSourceValue> _backingDictionary;
        private readonly Func<TSourceValue, TValue> _getterTransformFunc;
        private DictionaryKeys<TKey, TValue>? _keyCollection;
        private DictionaryValues<TKey, TValue>? _valueCollection;

        public TransformReadOnlyDictionary(IReadOnlyDictionary<TKey, TSourceValue> backingDictionary, Func<TSourceValue, TValue> getterTransformFunc)
        {
            this._backingDictionary = backingDictionary;
            this._getterTransformFunc = getterTransformFunc;
        }

        public TValue this[TKey key]
        {
            get => this._getterTransformFunc(this._backingDictionary[key]);
            set => throw new NotSupportedException();
        }

        public ICollection<TKey> Keys => this._keyCollection ??= new(this);

        public ICollection<TValue> Values => this._valueCollection ??= new(this);

        public int Count => this._backingDictionary.Count;

        public bool IsReadOnly => true;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        public void Add(TKey key, TValue value) => throw new NotSupportedException();

        public void Add(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(item.Value, value);
        }

        public bool ContainsKey(TKey key) => this._backingDictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this) array[arrayIndex++] = item;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in this._backingDictionary)
            {
                yield return KeyValuePair.Create(item.Key, this._getterTransformFunc(item.Value));
            }
        }

        public bool Remove(TKey key) => throw new NotSupportedException();

        public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (this._backingDictionary.TryGetValue(key, out var result))
            {
                value = this._getterTransformFunc(result);
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public static class TransformReadOnlyDictionary
    {
        public static TransformReadOnlyDictionary<TKey, TValue, TSourceValue> Create<TKey, TValue, TSourceValue>(IReadOnlyDictionary<TKey, TSourceValue> backingDictionary, Func<TSourceValue, TValue> getterTransformFunc) where TKey : notnull => new(backingDictionary, getterTransformFunc);
    }

}
