using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils.Threading
{
    /// <summary>
    /// Very similar to <see cref="Lazy{T}"/>, except that its resettable.
    /// </summary>
    /// <remarks>
    /// Dispose is only needed if the value is IDisposable
    /// </remarks>
    public sealed class ResettableLazy<T> : IDisposable
    {
        private sealed class ValueHolder { public T Value { get; init; } = default!; }

        private readonly object _holderLock = new();
        private ValueHolder? _holder;
        private Func<T> _valueFactory;

        public bool ShouldDispose { get; set; } = true;

        public bool IsValueCreated => this._holder is not null;
        public T Value
        {
            get
            {
                return (this._holder ?? this.CreateValue()).Value; // I hope this works!
            }
            set
            {
                lock (this._holderLock)
                {
                    if (this.ShouldDispose)
                    {
                        var holder = Interlocked.Exchange(ref this._holder, null);
                        if (holder is null) return;
                        if (holder.Value is IDisposable disposable) disposable.Dispose();
                    }
                    this._holder = new() { Value = value };
                }
            }
        }

        private ValueHolder CreateValue()
        {
            lock (this._holderLock)
            {
                return this._holder ??= new() { Value = this._valueFactory() };
            }
        }

        private static T DefaultValueFactory()
        {
            return Activator.CreateInstance<T>() ?? throw new InvalidOperationException($"Unable to create object of type {typeof(T)} using the default constructor");
        }

        [DoesNotReturn]
        private static T DisposedValueFactory() => throw new ObjectDisposedException(nameof(ResettableLazy<T>));

        public ResettableLazy() : this(DefaultValueFactory) { }

        public ResettableLazy(T value, Func<T> factory)
        {
            this._holder = new() { Value = value };
            this._valueFactory = factory;
        }

        public ResettableLazy(Func<T> factory)
        {
            this._valueFactory = factory;
        }

        public ResettableLazy(T value) : this()
        {
            this._holder = new() { Value = value };
        }

        public void Reset()
        {
            if (this._holder is not null)
            {
                if (this.ShouldDispose)
                {
                    var holder = Interlocked.Exchange(ref this._holder, null);
                    if (holder is null) return;
                    if (holder.Value is IDisposable disposable) disposable.Dispose();
                }
                else
                {
                    this._holder = null;
                }
            }
        }

        public void Dispose()
        {
            this._valueFactory = DisposedValueFactory;
            this.Reset();
        }
    }
}
