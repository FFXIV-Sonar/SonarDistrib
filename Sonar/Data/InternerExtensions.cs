using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data
{
    public static class InternerExtensions
    {
        public static T Intern<T>(this HashSet<T> set, T item)
        {
            if (!set.TryGetValue(item, out var result))
            {
                set.Add(item);
                return item;
            }
            return result;
        }

        public static IEnumerable<T> InternRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items) yield return set.Intern(item);
        }
    }
}
