using Sonar.Data.Rows;
using Sonar.Data;
using Sonar.Indexes;
using Sonar.Trackers;
using SonarUtils;
using SonarUtils.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sonar.Relays
{
    public static partial class RelayUtils
    {
        private static readonly ConcurrentDictionary<Type, RelayType> s_types = new();

        public static readonly IEnumerable<RelayType> Types = Enum.GetValues<RelayType>().Except([RelayType.Unknown]);

        /// <summary><see cref="Regex"/> for relay keys.</summary>
        /// <remarks>
        /// <para>Contain the following capturing groups: <c>worldId</c>, <c>relayId</c>, <c>instanceId</c>, <c>extendedKey</c></para>
        /// </remarks>
        public static Regex KeyRegex { get; } = GetKeyRegex();

        /// <summary>Gets a <see cref="RelayType"/> for a specified <paramref name="type"/></summary>
        /// <remarks>This actually accepts any <paramref name="type"/> however only <see cref="IRelay"/> types are expected to contain a <see cref="RelayTypeAttribute"/></remarks>
        public static RelayType GetRelayType(Type type)
        {
            return s_types.GetOrAdd(type, static type =>
            {
                Debug.Assert(type.GetAllTypes().Any(type => type == typeof(IRelay)));
                return type.GetCustomAttributes(true).OfType<RelayTypeAttribute>()
                    .FirstOrDefault()?.Type ?? RelayType.Unknown;
            });
        }

        /// <summary>Gets a <see cref="RelayType"/> for a specified <typeparamref name="T"/></summary>
        public static RelayType GetRelayType<T>() where T : IRelay
        {
            return GetRelayType(typeof(T));
        }

        /// <summary>Gets a <see cref="RelayType"/> for a specified <paramref name="relay"/></summary>
        public static RelayType GetRelayType(this IRelay relay) => GetRelayType(relay.GetType());

        /// <summary>Gets a <see cref="RelayType"/> for a specified <paramref name="state"/></summary>
        public static RelayType GetRelayType(this RelayState state) => GetRelayType(state.Relay.GetType());

        /// <summary>Extracts meta information from key.</summary>
        /// <param name="key">Relay key</param>
        public static bool TryGetKeyMeta(string key, [MaybeNullWhen(false)] out RelayKeyMeta meta)
        {
            var match = KeyRegex.Match(key);
            if (match.Success &&
                uint.TryParse(match.Groups["worldId"].ValueSpan, CultureInfo.InvariantCulture, out var worldId) &&
                uint.TryParse(match.Groups["relayId"].ValueSpan, CultureInfo.InvariantCulture, out var relayId) &&
                uint.TryParse(match.Groups["instanceId"].ValueSpan, CultureInfo.InvariantCulture, out var instanceId))
            {
                var extendedGroup = match.Groups["extendedKey"];
                var extendedKey = extendedGroup.Success ? extendedGroup.Value : null;
                meta = new(worldId, relayId, instanceId, extendedKey);
                return true;
            }
            meta = default;
            return false;
        }

        public static string GetRelayKey(uint worldId, uint relayId, uint instanceId, ReadOnlySpan<char> extendedKey = default)
        {
            return extendedKey.Length == 0 ? $"{StringUtils.GetNumber(worldId)}_{StringUtils.GetNumber(relayId)}_{StringUtils.GetNumber(instanceId)}" : $"{StringUtils.GetNumber(worldId)}_{StringUtils.GetNumber(relayId)}_{StringUtils.GetNumber(instanceId)}_{extendedKey}";
        }

        public static IRelayDataRow? GetRelayDataRow(RelayType type, uint relayId)
        {
            return type switch
            {
                RelayType.Hunt => Database.Hunts.GetValueOrDefault(relayId),
                RelayType.Fate => Database.Fates.GetValueOrDefault(relayId),
                _ => null,
            };
        }

        /// <summary>Clears <see cref="RelayType"/> cache</summary>
        public static void Reset() => s_types.Clear();

        [GeneratedRegex(@"^(?<worldId>\d+?)_(?<relayId>\d+?)_(?<instanceId>\d+?)(_(?<extendedKey>.*))?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetKeyRegex();
    }
}
