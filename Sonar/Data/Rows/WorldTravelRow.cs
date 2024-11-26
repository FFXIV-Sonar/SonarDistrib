using AG;
using MessagePack;
using Sonar.Data.Extensions;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Sonar.Data.Rows
{
    [MessagePackObject]
    public sealed class WorldTravelRow : IDataRow, IEquatable<WorldTravelRow>
    {
        [InlineArray(2)]
        [SuppressMessage("Major Code Smell", "S1144", Justification = "Trust me its used")]
        private struct WorldTravelInlineArray
        {
            public uint element0;
        }

        private WorldTravelInlineArray _array = default;
        private int? _hash;

        /// <summary>Flight ID</summary>
        [Key(0)]
        public uint Id { get; init; }

        /// <summary>Departing World ID</summary>
        [Key(1)]
        public uint StartWorldId
        {
            get => this._array[0];
            init => this._array[0] = value;
        }

        /// <summary>Arriving World ID</summary>
        [Key(2)]
        public uint EndWorldId
        {
            get => this._array[1];
            init => this._array[1] = value;
        }

        public bool Equals(WorldTravelRow? other)
        {
            if (ReferenceEquals(this, other) || other is null) return false;
            return this.StartWorldId == other.StartWorldId && this.EndWorldId == other.EndWorldId;
        }

        public override bool Equals(object? obj) => obj is WorldTravelRow other && this.Equals(other);

        [SuppressMessage("Minor Bug", "S2328")]
        public override int GetHashCode() => this._hash ??= this.GetHashCodeCore();

        public int GetHashCodeCore() => SplitHash32.Compute(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<WorldTravelInlineArray, uint>(ref this._array), 2));

        public override string ToString()
        {
            var startingWorld = this.GetStartWorld()?.ToString() ?? $"UNKNOWN ({this.StartWorldId})";
            var endingWorld = this.GetEndWorld()?.ToString() ?? $"UNKNOWN ({this.EndWorldId})";
            return $"{startingWorld} => {endingWorld}";
        }
    }
}
