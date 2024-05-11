using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SonarUtils
{
    public static class TypeUtils
    {
        private static readonly ConcurrentDictionary<Type, ImmutableArray<Type>> s_derivedTypes = new();
        private static readonly ConcurrentDictionary<Type, ImmutableArray<Type>> s_allTypes = new();
        
        public static ImmutableArray<Type> GetDerivedTypes(this Type type) => s_derivedTypes.GetOrAdd(type, GetDerivedTypesCore);
        public static ImmutableArray<Type> GetDerivedTypes(this object obj) => obj.GetType().GetDerivedTypes();
        private static ImmutableArray<Type> GetDerivedTypesCore(Type type)
        {
            var types = new HashSet<Type>();
            var baseType = type.BaseType;
            if (baseType is not null && baseType != typeof(object))
            {
                types.Add(baseType);
                types.UnionWith(baseType.GetDerivedTypes());
            }
            types.UnionWith(type.GetInterfaces());
            return types.ToImmutableArray();
        }
        public static ImmutableArray<Type> GetAllTypes(this Type type) => s_allTypes.GetOrAdd(type, GetAllTypesCore);
        public static ImmutableArray<Type> GetAllTypes(this object obj) => obj.GetType().GetAllTypes();
        private static ImmutableArray<Type> GetAllTypesCore(Type type) => type.GetDerivedTypes().Prepend(type).ToImmutableArray();

        public static void ResetCache()
        {
            s_allTypes.Clear();
            s_derivedTypes.Clear();
        }
    }
}
