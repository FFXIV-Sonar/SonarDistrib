using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SonarUtils.CustomData
{
    // - Lots of Unsafe.As usage here
    //   - The custom state pools needs to be able to consider different T and TArg.
    //   - The non-factory AddOrRemoveCustomData method reuses the fields without TArg.
    //     - object? used as placeholder
    // - All exposed methods ends with CustomData for easy identification.
    //   - It is a ConcurrentDictionary<object, object?>.
    //   - All type conversions are however done with normal casting instead of Unsafe.As
    //   - Keyless variant use typeof(T) as key, allowing for easier calls.

    /// <summary>Extension methods for <see cref="ICustomData"/>.</summary>
    public static class CustomDataExtensions
    {
        private static readonly Dictionary<Type, object> s_customStatePools = [];
        private static SpinLock s_lock = new(false);

        extension(ICustomData data)
        {
            public bool TryGetCustomData<T>(object key, [MaybeNullWhen(false)] out T value)
            {
                if (!data.CustomData.TryGetValue(key, out var result))
                {
                    value = default;
                    return false;
                }
                value = (T)result!;
                return true;
            }

            public bool TryGetCustomData<T>([MaybeNullWhen(false)] out T value)
                => data.TryGetCustomData(CustomDataUtils.GetDefaultKey<T>(), out value);

            public bool TryAddCustomData<T>(object key, T value)
                => data.CustomData.TryAdd(key, value);

            public bool TryAddCustomData<T>(T value)
                => data.TryAddCustomData(CustomDataUtils.GetDefaultKey<T>(), value);

            public bool TryUpdateCustomData<T>(object key, T newValue, T comparisonValue)
                => data.CustomData.TryUpdate(key, newValue, comparisonValue);

            public bool TryUpdateCustomData<T>(T newValue, T comparisonValue)
                => data.TryUpdateCustomData(CustomDataUtils.GetDefaultKey<T>(), newValue, comparisonValue);

            public bool TryRemoveCustomData<T>(object key, out T? value)
            {
                var result = data.CustomData.TryRemove(key, out var resultValue);
                value = (T)resultValue!;
                return result;
            }

            public bool TryRemoveCustomData<T>(out T? value)
                => data.TryRemoveCustomData(CustomDataUtils.GetDefaultKey<T>(), out value);

            public bool TryRemoveCustomData<T>(KeyValuePair<object, T> item)
                => data.CustomData.TryRemove(KeyValuePair.Create(item.Key, (object?)item.Value));

            public T GetOrAddCustomData<T, TArg>(object key, Func<object, TArg, T> valueFactory, TArg state)
            {
                var customState = RentOrCreateCustomState<T, TArg>();
                try
                {
                    customState.AddValueFactory = valueFactory;
                    customState.FactoryArgument = state;
                    return (T)data.CustomData.GetOrAdd(key, static (key, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, TArg>>(state);
                        return customState.AddValueFactory(key, customState.FactoryArgument);
                    }, customState)!;
                }
                finally
                {
                    ReturnCustomState(customState);
                }
            }

            public T GetOrAddCustomData<T, TArg>(Func<object, TArg, T> valueFactory, TArg state)
                => data.GetOrAddCustomData(CustomDataUtils.GetDefaultKey<T>(), valueFactory, state);

            public T GetOrAddCustomData<T>(object key, Func<object, T> valueFactory)
                => data.GetOrAddCustomData(key, static (key, valueFactory) => Unsafe.As<Func<object, T>>(valueFactory)(key), valueFactory);

            public T GetOrAddCustomData<T>(Func<object, T> valueFactory)
                => data.GetOrAddCustomData(CustomDataUtils.GetDefaultKey<T>(), valueFactory);

            public T AddOrUpdateCustomData<T, TArg>(object key, Func<object, TArg, T> addValueFactory, Func<object, T, TArg, T> updateValueFactory, TArg factoryArgument)
            {
                var customState = RentOrCreateCustomState<T, TArg>();
                try
                {
                    customState.AddValueFactory = addValueFactory;
                    customState.UpdateValueFactory = updateValueFactory;
                    customState.FactoryArgument = factoryArgument;
                    return (T)data.CustomData.AddOrUpdate(key, (key, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, TArg>>(state);
                        return customState.AddValueFactory(key, customState.FactoryArgument);
                    }, (key, oldValue, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, TArg>>(state);
                        return customState.UpdateValueFactory(key, (T)oldValue!, customState.FactoryArgument);
                    }, customState)!;
                }
                finally
                {
                    ReturnCustomState(customState);
                }
            }

            public T AddOrUpdateCustomData<T, TArg>(Func<object, TArg, T> addValueFactory, Func<object, T, TArg, T> updateValueFactory, TArg factoryArgument)
                => data.AddOrUpdateCustomData(CustomDataUtils.GetDefaultKey<T>(), addValueFactory, updateValueFactory, factoryArgument);

            public T AddOrUpdateCustomData<T>(object key, Func<object, T> addValueFactory, Func<object, T, T> updateValueFactory)
            {
                var customState = RentOrCreateCustomState<T, object>();
                try
                {
                    customState.AddValueFactory = Unsafe.As<Func<object, object, T>>(addValueFactory);
                    customState.UpdateValueFactory = Unsafe.As<Func<object, T, object, T>>(updateValueFactory);
                    return (T)data.CustomData.AddOrUpdate(key, (key, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, object>>(state);
                        return Unsafe.As<Func<object, T>>(customState.AddValueFactory)(key);
                    }, (key, oldValue, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, object>>(state);
                        return Unsafe.As<Func<object, T, T>>(customState.UpdateValueFactory)(key, (T)oldValue!);
                    }, customState)!;
                }
                finally
                {
                    ReturnCustomState(customState);
                }
            }

            public T AddOrUpdateCustomData<T>(Func<object, T> addValueFactory, Func<object, T, T> updateValueFactory)
                => data.AddOrUpdateCustomData(CustomDataUtils.GetDefaultKey<T>(), addValueFactory, updateValueFactory);

            public T AddOrUpdateCustomData<T, TArg>(object key, T addValue, Func<object, T, TArg, T> updateValueFactory, TArg factoryArgument)
            {
                var customState = RentOrCreateCustomState<T, TArg>();
                try
                {
                    customState.AddValue = addValue;
                    customState.UpdateValueFactory = Unsafe.As<Func<object, T, TArg, T>>(updateValueFactory);
                    customState.FactoryArgument = factoryArgument;
                    return (T)data.CustomData.AddOrUpdate(key, (_, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, TArg>>(state);
                        return customState.AddValue;
                    }, (key, oldValue, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, TArg>>(state);
                        return customState.UpdateValueFactory(key, (T)oldValue!, customState.FactoryArgument);
                    }, customState)!;
                }
                finally
                {
                    ReturnCustomState(customState);
                }
            }
            public T AddOrUpdateCustomData<T, TArg>(T addValue, Func<object, T, TArg, T> updateValueFactory, TArg factoryArgument)
                => data.AddOrUpdateCustomData(CustomDataUtils.GetDefaultKey<T>(), addValue, updateValueFactory, factoryArgument);

            public T AddOrUpdateCustomData<T>(object key, T addValue, Func<object, T, T> updateValueFactory)
            {
                var customState = RentOrCreateCustomState<T, object>();
                try
                {
                    customState.AddValue = addValue;
                    customState.UpdateValueFactory = Unsafe.As<Func<object, T, object, T>>(updateValueFactory);
                    return (T)data.CustomData.AddOrUpdate(key, (_, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, object>>(state);
                        return customState.AddValue;
                    }, (key, oldValue, state) =>
                    {
                        var customState = Unsafe.As<CustomState<T, object>>(state);
                        return Unsafe.As<Func<object, T, T>>(customState.UpdateValueFactory)(key, (T)oldValue!);
                    }, customState)!;
                }
                finally
                {
                    ReturnCustomState(customState);
                }
            }

            public T AddOrUpdateCustomData<T>(T addValue, Func<object, T, T> updateValueFactory)
                => data.AddOrUpdateCustomData(CustomDataUtils.GetDefaultKey<T>(), addValue, updateValueFactory);
        }

        #region CustomState Pool
        private static ConcurrentBag<CustomState<T, TArg>> GetOrCreatePool<T, TArg>()
        {
            CustomStatePoolContainer<T> container;
            var taken = false;
            s_lock.Enter(ref taken);
            Debug.Assert(taken);
            try
            {
                ref var containerRef = ref CollectionsMarshal.GetValueRefOrAddDefault(s_customStatePools, typeof(T), out var exists);
                if (!exists) containerRef = new CustomStatePoolContainer<T>();
                container = Unsafe.As<CustomStatePoolContainer<T>>(containerRef)!;
            }
            finally
            {
                s_lock.Exit(false);
            }

            taken = false;
            container.Lock.Enter(ref taken);
            Debug.Assert(taken);
            try
            {
                ref var poolRef = ref CollectionsMarshal.GetValueRefOrAddDefault(container.Pools, typeof(TArg), out var exists);
                if (!exists) poolRef = new ConcurrentBag<CustomState<T, TArg>>();
                return Unsafe.As<ConcurrentBag<CustomState<T, TArg>>>(poolRef)!;
            }
            finally
            {
                container.Lock.Exit(false);
            }
        }

        private static CustomState<T, TArg> RentOrCreateCustomState<T, TArg>()
        {
            if (GetOrCreatePool<T, TArg>().TryTake(out var customState)) return customState;
            return new();
        }

        private static void ReturnCustomState<T, TArg>(CustomState<T, TArg> customState)
        {
            customState.Clear();
            GetOrCreatePool<T, TArg>().Add(customState);
        }

        private sealed class CustomState<T, TState>()
        {
            public Func<object, TState, T> AddValueFactory = default!;
            public Func<object, T, TState, T> UpdateValueFactory = default!;
            public TState FactoryArgument = default!;
            public T AddValue = default!;

            public void Clear()
            {
                this.AddValueFactory = default!;
                this.UpdateValueFactory = default!;
                this.FactoryArgument = default!;
                this.AddValue = default!;
            }
        }

        private sealed class CustomStatePoolContainer<T>
        {
            public SpinLock Lock = new(false);
            public Dictionary<Type, object> Pools = [];
        }
        #endregion
    }
}
