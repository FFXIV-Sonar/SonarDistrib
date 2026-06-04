using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SonarUtils.Threading
{
    /// <summary>Provides an incrementing sequence id source.</summary>
    public struct SequenceIdProvider : IEquatable<SequenceIdProvider>
    {
        /// <summary>Initializes a new <see cref="SequenceIdProvider"/> starting from <c>-1</c>.</summary>
        public SequenceIdProvider() : this(-1) { /* Empty */ }

        /// <summary>Initializes a new <see cref="SequenceIdProvider"/> starting from <paramref name="current"/>.</summary>
        /// <param name="current">Starting sequence ID.</param>
        public SequenceIdProvider(long current) : this(current, DefaultUpdater) { /* Empty */ }

        /// <summary>Initializes a new <see cref="SequenceIdProvider"/> starting from <paramref name="current"/> and using a custom <paramref name="updater"/>.</summary>
        /// <param name="current">Starting sequence ID.</param>
        /// <param name="updater">Sequence ID Updater. This may be called multiple times per <see cref="GetNext"/> call under contention and therefore computationally expensive updaters should be avoided.</param>
        public SequenceIdProvider(long current, Func<long, long> updater)
        {
            this.Current = current;
            this.Updater = updater;
        }

#pragma warning disable S1104 // Justification = "Intended."
        /// <summary>Current sequence ID.</summary>
        public long Current;
#pragma warning restore S1104

        /// <summary>ID Updater function.</summary>
        public readonly Func<long, long> Updater { get; }

        /// <summary>Atomically get the next sequence ID and update <see cref="Current"/>.</summary>
        /// <returns>Next sequence ID.</returns>
        public long GetNext()
        {
            var spinWait = new SpinWait();
            while (true)
            {
                var current = Volatile.Read(ref this.Current);
                var next = this.Updater(current);
                if (Interlocked.CompareExchange(ref this.Current, next, current) == current) return next;
                spinWait.SpinOnce();
            }
        }

        /// <summary>Get the next sequence ID without updating <see cref="Current"/>.</summary>
        /// <returns>Next sequence ID.</returns>
        public readonly long GetNextWithoutUpdating() => this.Updater(Volatile.Read(in this.Current));

        public static bool Equals(SequenceIdProvider left, SequenceIdProvider right) => left.Current == right.Current && left.Updater == right.Updater;
        public readonly bool Equals(SequenceIdProvider other) => Equals(this, other);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is SequenceIdProvider other && this.Equals(other);
        public override readonly int GetHashCode() => HashCode.Combine(this.Current, this.Updater);
        public static bool operator ==(SequenceIdProvider left, SequenceIdProvider right) => Equals(left, right);
        public static bool operator !=(SequenceIdProvider left, SequenceIdProvider right) => !Equals(left, right);

        /// <summary>Basic incrementing function, incrementing <paramref name="current"/> by <c>1</c>.</summary>
        public static long DefaultUpdater(long current) => current + 1;
    }
}
