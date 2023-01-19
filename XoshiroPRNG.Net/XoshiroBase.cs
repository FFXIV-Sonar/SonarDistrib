using System;
using System.Runtime.CompilerServices;

namespace Xoshiro.Base {

    /// <summary>
    /// Base class of all Xoshiro/Xoroshiro PRNG Family
    /// </summary>
    abstract public class XoshiroBase : Random {

        /***** Abstract Methods *****/

        /// <summary>
        /// Fetch an Unsigned 32-bit integer from the PRNG.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        abstract public uint NextU();

        /// <summary>
        /// Fetch an Unsigned 64-bit integer from the PRNG.
        /// <para>Note: This is full range of [0, ulong.MaxValue]</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        abstract public ulong Next64U();

        /***** Concrete Methods *****/

        #region ulong Methods

        /// <summary>
        /// Fetch an Unsigned 64-bit integer from the PRNG, within the range [0, maxValue)
        /// </summary>
        /// <param name="maxValue">Must be &gt;1</param>
        public ulong Next64U(ulong maxValue) {
            if(maxValue <= 1) throw new ArgumentOutOfRangeException(
               nameof(maxValue), maxValue, "maxValue must be > 1");
            // Debiasing
            ulong r = ulong.MaxValue / maxValue;
            ulong tooLarge = r * maxValue;
            do {
                r = this.Next64U();
            } while(r >= tooLarge);
            return r % maxValue;
        }

        /// <summary>
        /// Fetch an Unsigned 64-bit integer from the PRNG, within the range [minValue, maxValue)
        /// </summary>
        /// <param name="minValue">Must be &lt; maxValue-1</param>
        /// <param name="maxValue">Must be &gt; minValue+1</param>
        public ulong Next64U(ulong minValue, ulong maxValue) {
            ulong rsize = maxValue - minValue;
            if(rsize < 2)
                throw new ArgumentException("minValue must be < maxValue-1!");
            return Next64U(rsize) + minValue;
        }

        #endregion ulong Methods

        #region long Methods

        /// <summary>
        /// Fetch the next Signed 64-bit integer from the PRNG.
        /// <para>Note: This is full range of [0, long.MaxValue]</para>
        /// <para>WARNING: Only 63 bits of randomness!</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Next64() {
            // chop off sign bit. this actually reduces randomness to 63 bit, but
            // if we don't do this, casting to long might result in negative numbers.
            return (long)(this.Next64U() & 0x7FFFFFFFFFFFFFFF);
        }

        /// <summary>
        /// Fetch a non-negative Signed 64-bit integer from the PRNG, within the range [0, maxValue).
        /// <para>WARNING: Only 63 bits of randomness!</para>
        /// </summary>
        /// <param name="maxValue">Must be &gt;1</param>
        public long Next64(long maxValue) {
            if(maxValue <= 1) throw new ArgumentOutOfRangeException(
               nameof(maxValue), maxValue, "maxValue must be > 1");
            // Debiasing
            long r = long.MaxValue / maxValue;
            long tooLarge = r * maxValue;
            do {
                r = this.Next64();
            } while(r >= tooLarge);
            return r % maxValue;
        }

        /// <summary>
        /// Fetch a Signed 64-bit integer from the PRNG, within the range [minValue, maxValue)
        /// <para>Note: (maxValue-minValue) must be within [0, long.MaxValue]</para>
        /// <para>WARNING: Only 63 bits of randomness!</para>
        /// </summary>
        /// <param name="minValue">Must be &lt; maxValue-1; can be negative</param>
        /// <param name="maxValue">Must be &gt; minValue+1; can be negative</param>
        public long Next64(long minValue, long maxValue) {
            if(minValue + 1 >= maxValue)
                throw new ArgumentException("minValue must be < maxValue-1!");
            ulong rsz = (ulong)(maxValue - minValue);
            if(rsz > long.MaxValue)
                throw new ArgumentException("(maxValue - minValue) must be <= long.MaxValue");
            return Next64((long)rsz) + minValue;
        }

        #endregion long Methods

        #region uint Methods

        /// <summary>
        /// Fetch an Unsigned 32-bit integer from the PRNG, within the range [0, maxValue)
        /// </summary>
        /// <param name="maxValue">Must be &gt; 0</param>
        public uint NextU(uint maxValue) {
            // I'm emulating the behavior of Random.Next(int), which accepts 1 as a parameter
            if (maxValue < 1) throw new ArgumentOutOfRangeException(
               nameof(maxValue), maxValue, "maxValue must be > 0");
            // Debiasing
            uint r = uint.MaxValue / maxValue;
            uint tooLarge = r * maxValue;
            do {
                r = NextU();
                if (maxValue == 1) return 0;
            } while (r >= tooLarge);
            return r % maxValue;
        }

        /// <summary>
        /// Fetch an Unsigned 32-bit integer from the PRNG, within the range [minValue, maxValue)
        /// </summary>
        /// <param name="minValue">Must be &lt; maxValue-1</param>
        /// <param name="maxValue">Must be &gt; minValue+1</param>
        /// <returns></returns>
        public uint NextU(uint minValue, uint maxValue) {
            uint rsize = maxValue - minValue;
            if(rsize < 2)
                throw new ArgumentException("minValue must be < maxValue-1!");
            return NextU(rsize) + minValue;
        }

        #endregion uint Methods

        #region System.Random-compatible (int with notes)

        /// <summary>
        /// Fetch a non-negative Signed 32-bit integer from the PRNG.
        /// <para>WARNING: This method will NOT return int.MaxValue,
        /// similar to System.Random.Next()</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Next() {
            int result;
            do
                // chop off sign bit so we won't get negative numbers.
                // of course this reduces randomness to 31 bits, but since
                // we must ensure that the number is positive, it's a
                // necessary evil.
                result = (int)(NextU() & 0x7FFFFFFF);
            while(result == int.MaxValue);
            // System.Random.Next() has a range of [0, int.MaxValue)
            // Why MSFT thought it's a good idea to discard int.MaxValue,
            // I have no idea.
            return result;
        }

        /// <summary>
        /// Fetch a non-negative Signed 32-bit integer from the PRNG, within the range [0, maxValue)
        /// </summary>
        /// <param name="maxValue">Numbers returned must be less than this. Must be &gt;1</param>
        public override int Next(int maxValue) {
            // I'm emulating the behavior of Random.Next(int), which despite the error
            // message, actually accepts zero as a parameter, treating it similarly
            // to one.
            if(maxValue < 0) throw new ArgumentOutOfRangeException(
               nameof(maxValue), maxValue, "'maxValue' must be greater than zero.");
            if(maxValue == 0) maxValue = 1;
            // Debiasing
            int r = int.MaxValue / maxValue;
            int tooLarge = r * maxValue;
            do
                r = (int)(NextU() & 0x7FFFFFFF);
            while(r >= tooLarge);
            return r % maxValue;
        }

        /// <summary>
        /// Fetch a Signed 32-bit integer from the PRNG, within the range [minValue, maxValue).
        /// </summary>
        /// <param name="minValue">Must be &lt;= maxValue. Can be negative.</param>
        /// <param name="maxValue">Must be &gt;= minValue. Can be negative.</param>
        public override int Next(int minValue, int maxValue) {
            // This emulates the behavior of System.Random.Next(int, int)
            // which accepts minValue == maxValue
            if(minValue > maxValue) throw new ArgumentOutOfRangeException(
               nameof(minValue), "'minValue' cannot be greater than maxValue.");
            if(minValue == maxValue) maxValue++;
            uint raw = NextU((uint)(maxValue - minValue));
            return (int)(raw + minValue);
        }

        // NOTE: We do NOT commonalize Sample() and NextFloat() because of the weakness of the least
        // significant bits of XoShiRo128plus might negatively affect the output of those two methods,
        // even if we merge together two consecutive reads of NextU() -- this will cause several bits
        // in the middle of the fraction part to be weak.

        #endregion System.Random-compatible (int with notes)

        /// <summary>
        /// Fetch a Signed 32-bit integer from the PRNG.
        /// <para>Note: This is full range of [int.MinValue, int.MaxValue], inclusive at both ends.</para>
        /// </summary>
        public int NextInt() {
            return (int)NextU();
        }

        /// <summary>
        /// Fetch a non-negative, Signed 32-bit integer from the PRNG.
        /// <para>Note: This is full non-negative range of [0, int.MaxValue], inclusive at both ends.</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextIntNonNegative() {
            return (int)(NextU() & 0x7FFFFFFF);
        }

    }
}
