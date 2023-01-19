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
 * 
 * This code is based on the source code for xoshiro128+ as publicized in:
 * http://xoshiro.di.unimi.it/xoshiro128plus.c
 * 
 */
using System;
using System.Runtime.CompilerServices;
using Xoshiro.Base;

namespace Xoshiro.PRNG32 {
    /* Comment from original source:
     * 
     * This is xoshiro128+ 1.0, our best and fastest 32-bit generator for 32-bit
     * floating-point numbers. We suggest to use its upper bits for
     * floating-point generation, as it is slightly faster than xoshiro128**.
     * 
     * It passes all tests we are aware of except for
     * linearity tests, as the lowest four bits have low linear complexity, so
     * if low linear complexity is not considered an issue (as it is usually
     * the case) it can be used to generate 32-bit outputs, too.
     * 
     * We suggest to use a sign test to extract a random Boolean value, and
     * right shifts to extract subsets of bits.
     * 
     * The state must be seeded so that it is not everywhere zero.
     * 
     */
    /// <summary>
    /// A very fast PRNG with 128-bit state, suitable for generating random
    /// single precision floating-point numbers.
    /// </summary>
    /// <remarks>
    /// <para>Due to some apparent weaknesses of the output's lower-orded bits, it is NOT
    /// recommended to use this PRNG to generate random 32-bit integers. (Single precision
    /// floating-point numbers only use the upper 23 bits.)</para>
    /// </remarks>
    public sealed class XoShiRo128plus : Xoshiro32Base {
        #region Internal States

        private uint s0;
        private uint s1;
        private uint s2;
        private uint s3;

        private const int NUM_STATES = 4;

        #endregion Internal States

        #region Constructors

        /// <summary>
        /// Default (Null) Constructor with seed taken from DateTime.UtcNow.Ticks
        /// </summary>
        public XoShiRo128plus() : this(DateTime.UtcNow.Ticks) { }

        /// <summary>
        /// Constructor with custom seed, which will be expanded into states
        /// by the SplitMix64 algorithm.
        /// </summary>
        /// <param name="seed">seed to initialize the state of SplitMix64</param>
        public XoShiRo128plus(long seed) {
            Span<uint> s = stackalloc uint[NUM_STATES];
            SplitMix64 sm64 = new SplitMix64(seed);
            sm64.Fill(s);
            s0 = s[0];
            s1 = s[1];
            s2 = s[2];
            s3 = s[3];
        }

        /// <summary>
        /// Constructor with custom initial state.
        /// <para>WARNING: DO NOT use this constructor unless you know exactly
        /// what you're doing!</para>
        /// </summary>
        /// <param name="initialStates">Array (minimum of 4 elements) containing the
        /// initial state</param>
        public XoShiRo128plus(uint[] initialStates) : this(initialStates.AsSpan()) { }

        /// <summary>
        /// Constructor with custom initial state.
        /// <para>WARNING: DO NOT use this constructor unless you know exactly
        /// what you're doing!</para>
        /// </summary>
        /// <param name="initialStates">Span (minimum of 4 elements) containing the
        /// initial state</param>
        public XoShiRo128plus(ReadOnlySpan<uint> initialStates)
        {
            if (initialStates.Length < NUM_STATES) throw new ArgumentException(
               $"initialStates must have at least {NUM_STATES} elements!",
               nameof(initialStates));
            s0 = initialStates[0];
            s1 = initialStates[1];
            s2 = initialStates[2];
            s3 = initialStates[3];
        }

        #endregion Constructors

        /// <summary>
        /// Fetch an Unsigned 32-bit integer from the PRNG.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint NextU() {
            // rotl := (x << k) | (x >> (32 - k))
            uint rslt = s0 + s3;

            uint t = s1 << 9;
            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;
            // rotl(s[3], 11)
            s3 = (s3 << 11) | (s3 >> 21);

            return rslt;
        }
    }
}
