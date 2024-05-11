using SonarUtils;
using SonarUtils.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Relays
{
    public static class RelayUtils
    {
        private static readonly ConcurrentDictionary<Type, RelayType> s_types = new();

        public static readonly IEnumerable<RelayType> Types = Enum.GetValues<RelayType>().Except(new RelayType[] { RelayType.Unknown });

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

        /// <summary>Clears <see cref="RelayType"/> cache</summary>
        public static void Reset() => s_types.Clear();
    }
}
