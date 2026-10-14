using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace SonarUtils.CustomData
{
    /// <summary>Utility methods for <see cref="ICustomData"/>.</summary>
    public static class CustomDataUtils
    {
        private static readonly ConcurrentBag<ConcurrentDictionary<object, object?>> s_dictionaries = [];
        
        /// <summary>Gets Default key for <typeparamref name="T"/>.</summary>
        public static object GetDefaultKey<T>() => typeof(T);

        /// <summary>Gets or creates a new <see cref="ConcurrentDictionary{TKey, TValue}"/>.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ConcurrentDictionary<object, object?> GetOrCreateCustomData()
        {
            if (s_dictionaries.TryTake(out var result)) return result;
            return new();
        }

        /// <summary>Return a <see cref="ConcurrentDictionary{TKey, TValue}"/> from a lost race.</summary>
        /// <param name="dictionary"><see cref="ConcurrentDictionary{TKey, TValue}"/> to return.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ReturnCustomData(ConcurrentDictionary<object, object?> dictionary)
            => s_dictionaries.Add(dictionary);

        /// <summary>Getter implementation for re-use.</summary>
        /// <remarks>
        ///   This implementation ensures the dictionary is only created whenever needed.
        ///   <list type="bullet">
        ///     <item>
        ///       It does a total of 3 read attempts
        ///       <list type="bullet">
        ///         <item>Direct field access.</item>
        ///         <item>Volatile field access.</item>
        ///         <item>Interlocked.CompareExchange field access.</item>
        ///       </list>
        ///     </item>
        ///     <item>It only does one write through Interlocked.CompareExchange if field is still null after creating the ConcurrentDictionary.</item>
        ///     <item>In case of lost races, the newly created dictionary is stored in a ConcurrentBag.</item>
        ///   </list>
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ConcurrentDictionary<object, object?> FieldGetterImpl(ref ConcurrentDictionary<object, object?>? field)
        {
            // Attempt to retrieve existing dictionary
            var customData = field ?? Volatile.Read(ref field);
            if (customData is not null) return customData;

            // Create a new dictionary then use Interlocked.CompareExchange
            var newCustomData = GetOrCreateCustomData();
            customData = Interlocked.CompareExchange(ref field, newCustomData, null);

            // There's a tiny chance another thread creates the dictionary.
            // If so, return it, or else, return the newly created one.
            if (customData is not null)
            {
                ReturnCustomData(newCustomData);
                Debug.Assert(ReferenceEquals(field, customData));
                return customData;
            }
            else
            {
                Debug.Assert(ReferenceEquals(field, newCustomData));
                return newCustomData;
            }
        }
    }
}
