using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace Sonar.Relays
{
    public static class RelayStatusExtensions
    {
        private static readonly RelayStatusMetaAttribute s_defaultMeta = new(0, 0, 0, 0, 0);
        private static readonly FrozenDictionary<RelayStatus, RelayStatusMetaAttribute> s_metas = GetMetas();

        private static FrozenDictionary<RelayStatus, RelayStatusMetaAttribute> GetMetas()
        {
            var type = typeof(RelayStatus);
            var dict = new Dictionary<RelayStatus, RelayStatusMetaAttribute>();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var status = (RelayStatus)field.GetValue(null)!;
                var attribute = field.GetCustomAttributes<RelayStatusMetaAttribute>().FirstOrDefault();
                if (attribute is null) continue;
                dict[status] = attribute;
            }
            return dict.ToFrozenDictionary();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RelayStatusMetaAttribute GetMeta(this RelayStatus status) => s_metas.GetValueOrDefault(status, s_defaultMeta);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this RelayStatus status) => status.GetMeta().IsAlive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPulled(this RelayStatus status) => status.GetMeta().IsPulled;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDead(this RelayStatus status) => status.GetMeta().IsDead;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStale(this RelayStatus status) => status.GetMeta().IsStale;

        /* TODO: Uncomment once IRelay.Status is implemented
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this IRelay relay) => relay.Status.IsAlive();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPulled(this IRelay relay) => relay.Status.IsPulled();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDead(this IRelay relay) => relay.Status.IsDead();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStale(this IRelay relay) => relay.Status.IsStale();
        */
    }
}
