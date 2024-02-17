using SonarUtils.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils
{
    public static class TypeUtils
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<Type>> s_derivedTypes = new();
        private static readonly ConcurrentDictionary<Type, IEnumerable<Type>> s_allTypes = new();
        
        public static IEnumerable<Type> GetDerivedTypes(this Type type) => s_derivedTypes.GetOrAdd(type, GetDerivedTypesCore);
        public static IEnumerable<Type> GetDerivedTypes(this object obj) => obj.GetType().GetDerivedTypes();
        private static IEnumerable<Type> GetDerivedTypesCore(Type type)
        {
            var types = new HashSet<Type>();
            var baseType = type.BaseType;
            if (baseType is not null && baseType != typeof(object))
            {
                types.Add(baseType);
                types.UnionWith(baseType.GetDerivedTypes());
            }
            types.UnionWith(type.GetInterfaces());
            return types.ToArray();
        }
        public static IEnumerable<Type> GetAllTypes(this Type type) => s_allTypes.GetOrAdd(type, GetAllTypesCore);
        public static IEnumerable<Type> GetAllTypes(this object obj) => obj.GetType().GetAllTypes();
        private static IEnumerable<Type> GetAllTypesCore(Type type) => type.GetDerivedTypes().Prepend(type);
    }
}
