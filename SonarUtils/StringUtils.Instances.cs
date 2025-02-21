using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SonarUtils
{
    public static partial class StringUtils
    {
        private static readonly Regex s_instanceRegex = GetGeneratedInstanceRegex();

        // https://github.com/goatcorp/Dalamud/blob/5a2473293d28a076835d02e9ec1f446924109952/Dalamud/Game/Text/SeIconChar.cs#L660-L703
        private static readonly FrozenDictionary<uint, string> s_instanceSymbolsGame = new Dictionary<uint, string>()
        {
            { 1, "\uE0B1" }, { 2, "\uE0B2" }, { 3, "\uE0B3" },
            { 4, "\uE0B4" }, { 5, "\uE0B5" }, { 6, "\uE0B6" },
            { 7, "\uE0B7" }, { 8, "\uE0B8" }, { 9, "\uE0B9" },
        }.ToFrozenDictionary();

        // https://unicode-explorer.com/search/ 
        private static readonly FrozenDictionary<uint, string> s_instanceSymbolsCircled = new Dictionary<uint, string>()
        {
            // Range 0-20
            { 1, "\u2460" }, { 2, "\u2461" }, { 3, "\u2462" }, { 4, "\u2463" }, { 5, "\u2464" },
            { 6, "\u2465" }, { 7, "\u2466" }, { 8, "\u2467" }, { 9, "\u2468" }, { 10, "\u2469" },
            { 11, "\u246A" }, { 12, "\u246B" }, { 13, "\u246C" }, { 14, "\u246D" }, { 15, "\u246E" },
            { 16, "\u246F" }, { 17, "\u2470" }, { 18, "\u2471" }, { 19, "\u2472" }, { 20, "\u2473" },

            // Range 21-35
            { 21, "\u3251" }, { 22, "\u3252" }, { 23, "\u3253" }, { 24, "\u3254" }, { 25, "\u3255" },
            { 26, "\u3256" }, { 27, "\u3257" }, { 28, "\u3258" }, { 29, "\u3259" }, { 30, "\u325A" },
            { 31, "\u325B" }, { 32, "\u325C" }, { 33, "\u325D" }, { 34, "\u325E" }, { 35, "\u325F" },

            // Range 36-50
            { 36, "\u32B1" }, { 37, "\u32B2" }, { 38, "\u32B3" }, { 39, "\u32B4" }, { 40, "\u32B5" },
            { 41, "\u32B6" }, { 42, "\u32B7" }, { 43, "\u32B8" }, { 44, "\u32B9" }, { 45, "\u32BA" },
            { 46, "\u32BB" }, { 47, "\u32BC" }, { 48, "\u32BD" }, { 49, "\u32BE" }, { 50, "\u32BF" },
        }.ToFrozenDictionary();

        private static readonly FrozenDictionary<string, uint> s_reverseInstanceSymbolsGame = s_instanceSymbolsGame.ToFrozenDictionary(kvp => kvp.Value, kvp => kvp.Key);
        private static readonly FrozenDictionary<string, uint> s_reverseInstanceSymbolsCircled = s_instanceSymbolsCircled.ToFrozenDictionary(kvp => kvp.Value, kvp => kvp.Key);

        /// <summary>Get an instance symbol based on the instance ID of a specified <paramref name="kind"/>.</summary>
        /// <param name="instanceId">Instance ID to get the symbol from.</param>
        /// <param name="kind">Instance Symbol kind.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><c>0</c>: Empty <see cref="string"/>.</item>
        /// <item>Symbols in range will return its specified symbol.</item>
        /// <item><c>_</c>: <c>$"i{<paramref name="instanceId"/>}</c>" interned fallback.</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentException">Invalid instance symbol <paramref name="kind"/>.</exception>
        public static string GetInstanceSymbol(uint instanceId, InstanceSymbolKind kind = InstanceSymbolKind.Game)
        {
            if (instanceId is 0) return string.Empty;
            var dict = GetInstanceSymbolDictionary(kind);
            if (dict.TryGetValue(instanceId, out var symbol)) return symbol;
            return $"i{instanceId}";
        }

        /// <summary>Try to parse the instance ID from <paramref name="symbol"/> of a specified <paramref name="kind"/>.</summary>
        /// <param name="symbol">Symbol to parse.</param>
        /// <param name="kind">Instance symbol kind.</param>
        /// <returns>Instance ID or <see langword="null"/> if unable to parse.</returns>
        /// <exception cref="ArgumentException">Invalid instance symbol <paramref name="kind"/>.</exception>
        public static uint? GetInstanceIdFromSymbol(string symbol, InstanceSymbolKind kind)
        {
            if (string.IsNullOrEmpty(symbol)) return 0;
            var match = s_instanceRegex.Match(symbol);
            if (match.Success) return uint.Parse(match.Groups["instance_id"].ValueSpan, CultureInfo.InvariantCulture);
            return GetInstanceIdFromSymbolCore(symbol, kind);
        }

        /// <summary>Try to parse the instance ID from <paramref name="symbol"/>.</summary>
        /// <param name="symbol">Symbol to parse.</param>
        /// <returns>Instance ID or <see langword="null"/> if unable to parse.</returns>
        public static uint? GetInstanceIdFromSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return 0;
            var match = s_instanceRegex.Match(symbol);
            if (match.Success) return uint.Parse(match.Groups["instance_id"].ValueSpan, CultureInfo.InvariantCulture);
            foreach (var kind in Enum.GetValues<InstanceSymbolKind>())
            {
                var instanceId = GetInstanceIdFromSymbolCore(symbol, kind);
                if (instanceId is not null) return instanceId;
            }
            return null;
        }

        /// <summary>Get a dictionary which contains the symbol <paramref name="kind"/> for the corresponding instance IDs.</summary>
        /// <param name="kind">Instance symbol kind.</param>
        /// <returns>Dictionary containing instance ID to symbols.</returns>
        /// <exception cref="ArgumentException">Invalid instance symbol <paramref name="kind"/>.</exception>
        public static FrozenDictionary<uint, string> GetInstanceSymbolDictionary(InstanceSymbolKind kind)
        {
            return kind switch
            {
                InstanceSymbolKind.None => FrozenDictionary<uint, string>.Empty,
                InstanceSymbolKind.Game => s_instanceSymbolsGame,
                InstanceSymbolKind.Circled => s_instanceSymbolsCircled,
                _ => throw new ArgumentException($"Invalid instance symbol {nameof(kind)}", nameof(kind))
            };
        }

        /// <summary>Get a dictionary which contains the instance IDs for the corresponding symbol <paramref name="kind"/>.</summary>
        /// <param name="kind">Instance symbol kind.</param>
        /// <returns>Dictionary containing symbols to instance IDs.</returns>
        /// <exception cref="ArgumentException">Invalid instance symbol <paramref name="kind"/>.</exception>
        public static FrozenDictionary<string, uint> GetInstanceSymbolReverseDictionary(InstanceSymbolKind kind)
        {
            return kind switch
            {
                InstanceSymbolKind.None => FrozenDictionary<string, uint>.Empty,
                InstanceSymbolKind.Game => s_reverseInstanceSymbolsGame,
                InstanceSymbolKind.Circled => s_reverseInstanceSymbolsCircled,
                _ => throw new ArgumentException($"Invalid instance symbol {nameof(kind)}", nameof(kind))
            };
        }

        private static uint? GetInstanceIdFromSymbolCore(string symbol, InstanceSymbolKind kind)
        {
            var dict = GetInstanceSymbolReverseDictionary(kind);
            if (dict.TryGetValue(symbol, out var instanceId)) return instanceId;
            return null;
        }

        [GeneratedRegex(@"^i(?<instance_id>\d+?)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetGeneratedInstanceRegex();
    }
}
