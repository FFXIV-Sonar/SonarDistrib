using Sonar.Relays;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    public sealed partial class ClientModifiers
    {
        private sealed class ContributeDictionary : IDictionary<RelayType, bool>
        {
            private static readonly FrozenSet<RelayType> s_typesCollection = RelayUtils.Types.ToFrozenSet();
            private readonly ClientModifiers _modifiers;
            private readonly HashSet<RelayType> _disabled = new();

            public ContributeDictionary(ClientModifiers modifiers)
            {
                this._modifiers = modifiers;
            }

            public bool this[RelayType type]
            {
                get
                {
                    return (this._modifiers.GlobalContribute ?? true) && !this._disabled.Contains(type);
                }
                set
                {
                    if (value) this._disabled.Remove(type);
                    if (!value) this._disabled.Add(type);
                }
            }

            public ICollection<RelayType> Keys => s_typesCollection;

            public ICollection<bool> Values => throw new NotSupportedException("I hope this is never used");

            public int Count => s_typesCollection.Count;

            public bool IsReadOnly => false;

            public void Add(RelayType key, bool value) => this[key] = value;

            public void Add(KeyValuePair<RelayType, bool> item) => this.Add(item.Key, item.Value);

            public void Clear() => this._disabled.Clear();

            public bool Contains(KeyValuePair<RelayType, bool> item) => this[item.Key] == item.Value;

            public bool ContainsKey(RelayType key) => s_typesCollection.Contains(key);

            public void CopyTo(KeyValuePair<RelayType, bool>[] array, int arrayIndex)
            {
                foreach (var item in this)
                {
                    array[arrayIndex++] = item;
                }
            }

            public IEnumerator<KeyValuePair<RelayType, bool>> GetEnumerator()
            {
                foreach (var type in s_typesCollection)
                {
                    yield return KeyValuePair.Create(type, !this._disabled.Contains(type));
                }
            }

            public bool Remove(RelayType key) => this._disabled.Remove(key); // This simply resets to true

            public bool Remove(KeyValuePair<RelayType, bool> item)
            {
                if (item.Value) return false;
                return this._disabled.Remove(item.Key);
            }

            public bool TryGetValue(RelayType key, [MaybeNullWhen(false)] out bool value)
            {
                value = !this._disabled.Contains(key);
                return s_typesCollection.Contains(key);
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}
