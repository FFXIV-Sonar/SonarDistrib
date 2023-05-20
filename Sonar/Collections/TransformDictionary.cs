using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Sonar.Collections
{
    /// <summary>
    /// A dictionary that automatically transform values
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <typeparam name="TSourceValue">Backing dictionary's value type</typeparam>
    public sealed partial class TransformDictionary<TKey, TValue, TSourceValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly IDictionary<TKey, TSourceValue> _backingDictionary;
        private readonly Func<TSourceValue, TValue> _getterTransformFunc;
        private readonly Func<TValue, TSourceValue> _setterTransformFunc = default!; // Not used if read-only
        private KeyCollection? _keyCollection;
        private ValueCollection? _valueCollection;

        public TransformDictionary(IDictionary<TKey, TSourceValue> backingDictionary, Func<TSourceValue, TValue> getterTransformFunc)
        {
            this._backingDictionary = backingDictionary;
            this._getterTransformFunc = getterTransformFunc;
            this.IsReadOnly = true;
        }

        public TransformDictionary(IDictionary<TKey, TSourceValue> backingDictionary, Func<TSourceValue, TValue> getterTransformFunc, Func<TValue, TSourceValue> setterTransformFunc) : this(backingDictionary, getterTransformFunc)
        {
            this._setterTransformFunc = setterTransformFunc;
            this.IsReadOnly = backingDictionary.IsReadOnly; // If for some reason a read only dictionary is used with a setter.... uh?... what does this constructor do again?
        }

        /// <summary>Makes this dictionary read only.</summary>
        /// <remarks>You can still modify the backing dictionary and changes will be seen in this dictionary still.</remarks>
        public void MakeReadOnly()
        {
            if (!this.IsReadOnly) this.IsReadOnly = true;
        }

        [DoesNotReturn]
        private static void ThrowNotSupportedException() => throw new NotSupportedException();

        public TValue this[TKey key]
        {
            get => this._getterTransformFunc(this._backingDictionary[key]);
            set
            {
                if (this.IsReadOnly) ThrowNotSupportedException();
                this._backingDictionary[key] = this._setterTransformFunc(value);
            }
        }

        public ICollection<TKey> Keys => this._keyCollection ??= new(this);

        public ICollection<TValue> Values => this._valueCollection ??= new(this);

        public int Count => this._backingDictionary.Count;

        public bool IsReadOnly { get; private set; }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        public void Add(TKey key, TValue value)
        {
            if (this.IsReadOnly) ThrowNotSupportedException();
            this._backingDictionary.Add(key, this._setterTransformFunc(value));
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (this.IsReadOnly) ThrowNotSupportedException();
            this._backingDictionary.Add(KeyValuePair.Create(item.Key, this._setterTransformFunc(item.Value)));
        }

        public void Clear()
        {
            if (this.IsReadOnly) ThrowNotSupportedException();
            this._backingDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            // This is a weird case but I'll follow
            if (this.IsReadOnly) return this.ContainsKey(item.Key);
            return this._backingDictionary.Contains(KeyValuePair.Create(item.Key, this._setterTransformFunc(item.Value)));
        }

        public bool ContainsKey(TKey key) => this._backingDictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
                if (arrayIndex == array.Length) return;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in this._backingDictionary)
            {
                yield return KeyValuePair.Create(item.Key, this._getterTransformFunc(item.Value));
            }
        }

        public bool Remove(TKey key)
        {
            if (this.IsReadOnly) ThrowNotSupportedException();
            return this._backingDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            // This is a weird case but I'll follow
            if (this.IsReadOnly) return this.Remove(item.Key);
            return this._backingDictionary.Remove(KeyValuePair.Create(item.Key, this._setterTransformFunc(item.Value)));
        }

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

    public static class TransformDictionary
    {
        public static TransformDictionary<TKey, TValue, TSourceValue> Create<TKey, TValue, TSourceValue>(IDictionary<TKey, TSourceValue> backingDictionary, Func<TSourceValue, TValue> getterTransformFunc) where TKey : notnull => new(backingDictionary, getterTransformFunc);
        public static TransformDictionary<TKey, TValue, TSourceValue> Create<TKey, TValue, TSourceValue>(IDictionary<TKey, TSourceValue> backingDictionary, Func<TSourceValue, TValue> getterTransformFunc, Func<TValue, TSourceValue> setterTransformFunc) where TKey : notnull => new(backingDictionary, getterTransformFunc, setterTransformFunc);
    }

}
