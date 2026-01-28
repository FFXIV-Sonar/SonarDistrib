using SonarUtils.Greek;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SonarUtils.Greek
{
    public static class GreekSymbolExtensions
    {
        private static readonly FrozenDictionary<GreekSymbol, GreekSymbolAttribute> s_attributes = typeof(GreekSymbol).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => KeyValuePair.Create((GreekSymbol)field.GetValue(null)!, field.GetCustomAttribute<GreekSymbolAttribute>()!)).Where(kvp => kvp.Key is not GreekSymbol.Invalid).ToFrozenDictionary();

        extension (GreekSymbol symbol)
        {
            /// <summary>Upper case <see langword="char"/> for this <see cref="GreekSymbol"/>.</summary>
            /// <remarks>Returns <c>\0</c> if not valid.</remarks>
            public char UpperChar => s_attributes.TryGetValue(symbol, out var attribute) ? attribute.UpperChar : '\0';

            /// <summary>Lower case <see langword="char"/> for this <see cref="GreekSymbol"/>.</summary>
            /// <remarks>Returns <c>\0</c> if not valid.</remarks>
            public char LowerChar => s_attributes.TryGetValue(symbol, out var attribute) ? attribute.LowerChar : '\0';

            /// <summary>End of words lower case <see langword="char"/> for this <see cref="GreekSymbol"/>.</summary>
            /// <remarks>Returns <c>\0</c> if not valid.</remarks>
            public char FinalChar => s_attributes.TryGetValue(symbol, out var attribute) ? attribute.FinalChar : '\0';

            /// <summary>Upper case <see langword="string"/> for this <see cref="GreekSymbol"/>.</summary>
            /// <remarks>Returns <see cref="string.Empty"/> if not valid.</remarks>
            public string UpperString => s_attributes.TryGetValue(symbol, out var attribute) ? attribute.UpperString : string.Empty;

            /// <summary>Lower case <see langword="string"/> for this <see cref="GreekSymbol"/>.</summary>
            /// <remarks>Returns <see cref="string.Empty"/> if not valid.</remarks>
            public string LowerString => s_attributes.TryGetValue(symbol, out var attribute) ? attribute.LowerString : string.Empty;

            /// <summary>End of words lower case <see langword="string"/> for this <see cref="GreekSymbol"/>.</summary>
            /// <remarks>Returns <see cref="string.Empty"/> if not valid.</remarks>
            public string FinalString => s_attributes.TryGetValue(symbol, out var attribute) ? attribute.FinalString : string.Empty;

            /// <summary>Upper and lower case characters (<c>"Aa"</c>) for this <see cref="GreekSymbol"/> as a <see langword="string"/>.</summary>
            /// <remarks>Returns <see cref="string.Empty"/> if not valid.</remarks>
            public string Chars => s_attributes.TryGetValue(symbol, out var attribute) ? attribute.Chars : string.Empty;
        }
    }
}
