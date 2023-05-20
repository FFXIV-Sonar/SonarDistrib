using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Collections
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ISet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items) set.Add(item);
        }

        public static void RemoveRange<T>(this ISet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items) set.Remove(item);
        }
    }
}
