using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace SonarUtils.Secrets
{
    public static class SecretUtils
    {
        private static readonly ConcurrentDictionary<Assembly, SecretMetaAttribute?> s_secretMetaAttributes = new();

        public static SecretMetaAttribute? GetSecretMeta(this Assembly assembly)
            => s_secretMetaAttributes.GetOrAdd(assembly, GetSecretMetaCore);

        public static ImmutableArray<byte>? GetSecretMetaBytes(this Assembly assembly)
            => assembly.GetSecretMeta()?.Bytes;

        private static SecretMetaAttribute? GetSecretMetaCore(Assembly assembly)
            => assembly.GetCustomAttribute<SecretMetaAttribute>();
    }
}
