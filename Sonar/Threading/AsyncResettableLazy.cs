using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sonar.Threading
{
    // License for AsyncResettableLazy: MIT, feel free to use

    /// <summary>
    /// Very similar to Lazy <typeparamref name="T"/>, except that its resettable. Additionally, uses the old value while initializing a new one.
    /// </summary>
    /// <typeparam name="T">Object type to hold</typeparam>
    public sealed class AsyncResettableLazy<T> : IDisposable
    {
        private readonly object _valueLock = new();
        private bool _valueCreated;
        private T _value = default!;
        private Func<T> _valueFactory;
        private Task? _createValueTask; // <-- Not to be confused with ValueTask, _createValue is in reference to this.CreateValue()

        public bool IsValueCreated => this._valueCreated;
        public T Value
        {
            get
            {
                if (!this._valueCreated || this._createValueTask is null)
                {
                    this._createValueTask ??= Task.Run(() => { this.CreateValue(); });
                    if (!this._valueCreated) this._createValueTask.Wait();
                }
                return this._value;
            }
            set
            {
                lock (this._valueLock)
                {
                    this._value = value;
                    this._valueCreated = true;
                }
            }
        }

        private void CreateValue()
        {
            var newValue = this._valueFactory();
            bool wasCreated;
            T oldValue;
            lock (this._valueLock)
            {
                oldValue = this._value;
                this._value = newValue;
                wasCreated = this._valueCreated;
                this._valueCreated = true;
            }
            if (wasCreated && oldValue is IDisposable disposable) disposable.Dispose();
        }

        private static T DefaultValueFactory()
        {
            var instance = Activator.CreateInstance<T>();
            if (instance is null) throw new InvalidOperationException($"Unable to create object of type {typeof(T)} using the default constructor");
            return instance;
        }

        private static T DisposedValueFactory() => throw new ObjectDisposedException(nameof(AsyncResettableLazy<T>));

        public AsyncResettableLazy() : this(DefaultValueFactory) { }

        public AsyncResettableLazy(T value, Func<T> factory) : this(factory)
        {
            this._valueCreated = true;
            this._value = value;
        }

        public AsyncResettableLazy(Func<T> factory)
        {
            this._valueFactory = factory;
        }

        public AsyncResettableLazy(T value) : this()
        {
            this._valueCreated = true;
            this._value = value;
        }

        public void Reset()
        {
            this._createValueTask = null;
        }

        public void Dispose()
        {
            this._valueFactory = DisposedValueFactory;
            if (this._value is IDisposable disposable) disposable.Dispose();
        }
    }
}
