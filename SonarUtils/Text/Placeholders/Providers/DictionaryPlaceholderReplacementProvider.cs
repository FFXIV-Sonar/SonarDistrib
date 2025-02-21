using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SonarUtils.Text.Placeholders.Providers
{
    public sealed class DictionaryPlaceholderReplacementProvider<T>(IReadOnlyDictionary<string, T> dictionary) : IPlaceholderReplacementProvider
    {
        public bool TryGetValue(ReadOnlySpan<char> name, [MaybeNullWhen(false)] out ReadOnlySpan<char> value)
        {
            if (dictionary.TryGetValue(name.ToString(), out var item)) // TODO: Use AlternativeLookup at .NET 9
            {
                value = item?.ToString() ?? string.Empty;
                return true;
            }
            value = default;
            return false;
        }
    }
}
