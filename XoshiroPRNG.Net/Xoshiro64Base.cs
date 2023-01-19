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

namespace Xoshiro.PRNG64 {
    /// <summary>
    /// The base class for all PRNGs that natively output 64-bit integers.
    /// </summary>
    /// <remarks><para>You'll notice that all internal state variables are also
    /// 64 bits.</para></remarks>
    abstract public class Xoshiro64Base : XoshiroBase, IRandom64U {
        #region Properties

        /* Public Properties */
        /// <summary>
        /// Sets the desired folding method to convert the 64-bit PRNG output to 32 bits.
        /// </summary>
        /// <see cref="Fold64To32Method"/>
        public Fold64To32Method FoldMethod {
            // Not using "Auto-Implemented Property" so we can have a non-anonymous backing field
            get { return _foldmethod; }
            set { _foldmethod = value; }
        }

        /* Private Properties */
        private Fold64To32Method _foldmethod = Fold64To32Method.ChunkMethod;
        private uint nextChunk;
        private bool hasNextChunk = false;

        #endregion Properties

        #region Native Output (64-bit)

        /* Abstract Methods */

        /* Concrete Methods */

        #endregion Native Output (64-bit)

        #region Truncated Native Output (64-bit)

        #endregion Truncated Native Output (64-bit)

        #region Folded Native Output (32-bit)

        /// <summary>
        /// Fetch a folded Unsigned 32-bit integer from the PRNG.
        /// <para>Note: This is full range of [0, uint.MaxValue]</para>
        /// <para>This is</para>
        /// </summary>
        public override uint NextU() {
            if(_foldmethod == Fold64To32Method.XorMethod) {
                ulong n = Next64U();
                uint a = (uint)(n >> 32);
                uint b = (uint)(n & 0xFFFFFFFF);
                return a ^ b;
            }
            else if(_foldmethod == Fold64To32Method.ChunkMethod) {
                if(!hasNextChunk) {
                    ulong n = Next64U();
                    nextChunk = (uint)(n & 0xFFFFFFFF);
                    hasNextChunk = true;
                    return (uint)(n >> 32);
                }
                else {
                    uint r = nextChunk;
                    hasNextChunk = false;
                    return r;
                }
            }
            else throw new NotSupportedException("Unrecognized Fold64To32 method!");
        }

        #endregion Folded Native Output (32-bit)

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
            const int BYTESIZE7 = 8 * 7;
            const ulong TOPMASK = 0xFF00000000000000;

            ulong num = 0;
            ulong mask = 0;
            int shift = -1;

            for (int i = 0; i < buffer.Length; i++)
            {
                if (shift < 0)
                {
                    num = Next64U();
                    mask = TOPMASK;
                    shift = BYTESIZE7;
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
        /// <para>WARNING: Only 53 bits of randomness!</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override double Sample() {
            /* This is the recommended least-surprise method in the xoshiro paper. */
            double r;
            do {
                r = (Next64U() >> 11) * (1.0 / ((ulong)1 << 53));
            } while((r >= 1.0) || (r < 0.0));
            // The check SHOULDN'T be necessary, but we're protecting ourselves from possible
            // bugs in the system's base implementation of floating-point operations, something
            // we have no control of.
            return r;
        }

        /// <summary>
        /// Fetch a non-negative double precision floating-point number, within the range [0, 1.0)
        /// <para>WARNING: Only 53 bits of randomness!</para>
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
                r = (NextU() >> 40) * ((float)1.0 / ((uint)1 << 24));
            } while((r >= 1.0) || (r < 0.0));
            // The check SHOULDN'T be necessary, but we're protecting ourselves from possible
            // bugs in the system's base implementation of floating-point operations, something
            // we have no control of.
            return r;
        }

        /// <summary>
        /// Get a System.Random-compatible interface.
        /// <para>Do note that System.Random-compatible interface does not provide 64-bit values!</para>
        /// </summary>
        public Random GetRandomCompatible() {
            return (Random)this;
            }

        #endregion Additional Unleashed interface
        }

    }
