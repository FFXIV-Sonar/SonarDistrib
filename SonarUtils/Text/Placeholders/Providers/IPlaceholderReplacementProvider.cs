using System;
using System.Diagnostics.CodeAnalysis;

namespace SonarUtils.Text.Placeholders.Providers
{
    public interface IPlaceholderReplacementProvider
    {
        public bool TryGetValue(ReadOnlySpan<char> name, [MaybeNullWhen(false)] out ReadOnlySpan<char> value);
    }
}
