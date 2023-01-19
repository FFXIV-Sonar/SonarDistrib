/* This code is released by Pandu POLUAN into the Public Domain, OR using
 * one of the following licenses according to your preference:
 *   - The Unlicense
 *     https://spdx.org/licenses/Unlicense.html
 *   - Creative Commons Zero v1.0 Universal
 *     https://spdx.org/licenses/CC0-1.0.html
 *   - Do What The F*ck You Want Public License
 *     https://spdx.org/licenses/WTFPL.html
 *   - MIT-0 License
 *     https://spdx.org/licenses/MIT-0.html
 *   - BSD 3-Clause License
 *     https://spdx.org/licenses/BSD-3-Clause.html
 */
using System;
using System.Runtime.CompilerServices;

using Xoshiro.Base;

namespace Xoshiro.PRNG32 {
    /// <summary>
    /// The base class for all PRNGs that natively output 32-bit integers.
    /// </summary>
    /// <remarks><para>You'll notice that all internal state variables are also
    /// 32 bits.</para></remarks>
    abstract public class Xoshiro32Base : XoshiroBase, IRandomU {

        #region System.Random-compatible interface

        /// <summary>
        /// Fill an array of bytes with the next numbers from the PRNG.
        /// </summary>
        /// <param name="buffer">Cannot be null</param>
        public override void NextBytes(byte[] buffer) => NextBytes(buffer.AsSpan());

        /// <summary>
        /// Fill an array of bytes with the next numbers from the PRNG.
        /// </summary>
        /// <param name="buffer">Cannot be null</param>
        public override void NextBytes(Span<byte> buffer)
        {
            const int BYTESIZE = 8;
            const int BYTESIZE3 = 8 * 3;
            const uint TOPMASK = 0xFF000000;

            uint num = 0;
            uint mask = 0;
            int shift = -1;

            for (int i = 0; i < buffer.Length; i++)
            {
                if (shift < 0)
                {
                    num = NextU();
                    mask = TOPMASK;
                    shift = BYTESIZE3;
                }
                buffer[i] = (byte)((num & mask) >> shift);
                mask >>= BYTESIZE;
                shift -= BYTESIZE;
            }
        }

        /* The implementation of Sample() and NextDouble() that simply returns Sample() is
         * based on how System.Random implemented the two. I don't really understand MSFT's
         * justification for this strategy, but we'll follow it just in case.
         */

        /// <summary>
        /// Fetch a non-negative double precision floating-point number, within the range [0, 1.0)
        /// <para>WARNING: Only 24 bits of randomness!</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override double Sample() {
            /* This adapted from the recommended least-surprise method for double in the xoshiro paper. */
            double r;
            do {
                r = (NextU() >> 8) * (1.0 / ((uint)1 << 24));
            } while((r >= 1.0) || (r < 0.0));
            // The check SHOULDN'T be necessary, but we're protecting ourselves from possible
            // bugs in the system's base implementation of floating-point operations, something
            // we have no control of.
            return r;
        }

        /// <summary>
        /// Fetch a non-negative double precision floating-point number, within the range [0, 1.0)
        /// <para>WARNING: Only 24 bits of randomness!</para>
        /// </summary>
        public override double NextDouble() {
            return Sample();
        }

        #endregion System.Random-compatible interface

        #region Additional Unleashed interface

        /// <summary>
        /// Fetch a non-negative single precision floating-point number, within the range [0, 1.0)
        /// <para>WARNING: Only 24 bits of randomness!</para>
        /// </summary>
        public float NextFloat() {
            /* This adapted from the recommended least-surprise method for double in the xoshiro paper. */
            float r;
            do {
                // We need only top 24 bits.
                r = (NextU() >> 8) * (1.0F / ((uint)1 << 24));
            } while((r >= 1.0) || (r < 0.0));
            // The check SHOULDN'T be necessary, but we're protecting ourselves from possible
            // bugs in the system's base implementation of floating-point operations, something
            // we have no control of.
            return r;
        }

        /// <summary>
        /// Fetch an Unsigned 64-bit integer by appending two consecutive 32-bit integers from the PRNG.
        /// <para>Note: This is full range of [0, ulong.MaxValue]</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong Next64U() {
            ulong upper = (ulong)NextU() << 32;
            return upper | NextU();
        }

        /// <summary>
        /// Get a System.Random-compatible interface
        /// </summary>
        public Random GetRandomCompatible() {
            return (Random)this;
            }

        #endregion Additional Unleashed interface
    }
}
