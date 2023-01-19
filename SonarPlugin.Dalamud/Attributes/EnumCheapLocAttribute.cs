using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheapLoc;
using System.Reflection;
using System.Runtime.CompilerServices;
using SonarPlugin.Config;

namespace SonarPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EnumCheapLocAttribute : Attribute
    {
        public string Key { get; }
        public string Fallback { get; }

        public EnumCheapLocAttribute(string key, string fallback)
        {
            this.Key = key;
            this.Fallback = fallback;
        }

        internal string ToString(Assembly assembly) => Loc.Localize(this.Key, this.Fallback, assembly);
    }

    public static class EnumCheapLocExtensions
    {
        /// <summary>
        /// Get all enum values CheapLocalized
        /// </summary>
        public static Dictionary<T, string> CheapLoc<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(v => v, v => v.CheapLoc());
        
        /// <summary>
        /// CheapLoc enum value
        /// </summary>
        public static string CheapLoc<T>(this T value) where T : Enum
        {
            Type type = typeof(T);
            
            var name = Enum.GetName(type, value);
            if (name is null) return value.ToString(); // Undefined enum values will be the underlying type value

            return type
                .GetMember(name)
                .First(m => m.DeclaringType == type)
                .GetCustomAttribute<EnumCheapLocAttribute>(true)?

                // CheapLocalize
                .ToString(type.Assembly)

                // Enum values without EnumCheapLocAttribute will be the name of the enum value
                ?? value.ToString();
        }
    }
}
