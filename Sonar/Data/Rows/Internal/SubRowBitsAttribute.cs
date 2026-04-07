using System;

namespace Sonar.Data.Rows.Internal
{
    /// <summary>Informs <see cref="EventUtils"/> of the number of bits to reserve for subrows.</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal sealed class SubRowBitsAttribute : Attribute
    {
        /// <summary>Reserve <paramref name="bits"/> for subrows.</summary>
        /// <param name="bits">Bits to reserve.</param>
        public SubRowBitsAttribute(int bits)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(bits, 24);
            this.Bits = bits;
            this.Mask = (1u << bits) - 1;
        }

        /// <summary>Reserve <c>8</c> bits for subrows.</summary>
        public SubRowBitsAttribute() : this(8) { /* Empty */ }

        /// <summary>Number of bits reserved.</summary>
        public int Bits { get; }

        /// <summary>Bit mask with all least significant <see cref="Bits"/> set.</summary>
        public uint Mask { get; }
    }
}
