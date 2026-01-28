using SonarUtils.Greek;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace SonarUtils.Greek
{
    public static class GreekSymbolUtils
    {
        private static readonly FrozenDictionary<char, GreekSymbol> s_symbols = Enum.GetValues<GreekSymbol>().SelectMany(symbol => symbol.Chars.Select(ch => KeyValuePair.Create(ch, symbol))).ToFrozenDictionary();

        /// <summary>Returns the <see cref="GreekSymbol"/> corresponding to <paramref name="ch"/>.</summary>
        public static GreekSymbol ToGreekSymbol(char ch)
        {
            if (s_symbols.TryGetValue(ch, out var symbol)) return symbol;
            return GreekSymbol.Invalid;
        }
    }
}
