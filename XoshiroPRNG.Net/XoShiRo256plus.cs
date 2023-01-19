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
 * This code is based on the source code for xoshiro256+ as publicized in:
 * http://xoshiro.di.unimi.it/xoshiro256plus.c
 * 
 */
using System;
using System.Runtime.CompilerServices;
using Xoshiro.Base;

namespace Xoshiro.PRNG64 {
    /* Comment from original source:
     * 
     * This is xoshiro256+ 1.0, our best and fastest generator for floating-point
     * numbers. We suggest to use its upper bits for floating-point
     * generation, as it is slightly faster than xoshiro256**. It passes all
     * tests we are aware of except for the lowest three bits, which might
     * fail linearity tests (and just those), so if low linear complexity is
     * not considered an issue (as it is usually the case) it can be used to
     * generate 64-bit outputs, too.
     * 
     * We suggest to use a sign test to extract a random Boolean value, and
     * right shifts to extract subsets of bits.
     * 
     * The state must be seeded so that it is not everywhere zero. If you have
     * a 64-bit seed, we suggest to seed a splitmix64 generator and use its
     * output to fill s.
     */
    /// <summary>
    /// A fast PRNG with 256-bit state, suitable for generating random double precision
    /// floating-point numbers.
    /// </summary>
    /// <remarks>
    /// <para>Due to some apparent weaknesses of the output's lower-orded bits, it is NOT
    /// recommended to use this PRNG to generate random 64-bit integers. (Double precision
    /// floating-point numbers only use the upper 53 bits.)</para>
    /// </remarks>
    public sealed class XoShiRo256plus : Xoshiro64Base {
        #region Internal States

        private ulong s0;
        private ulong s1;
        private ulong s2;
        private ulong s3;

        private const int NUM_STATES = 4;

        #endregion Internal States

        #region Constructors

        /// <summary>
        /// Default (Null) Constructor with seed taken from DateTime.UtcNow.Ticks
        /// </summary>
        public XoShiRo256plus() : this(DateTime.UtcNow.Ticks) { }

        /// <summary>
        /// Constructor with custom seed, which will be expanded into states
        /// by the SplitMix64 algorithm.
        /// </summary>
        /// <param name="seed">seed to initialize the state of SplitMix64</param>
        public XoShiRo256plus(long seed) {
            Span<ulong> s = stackalloc ulong[NUM_STATES];
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
        public XoShiRo256plus(ulong[] initialStates) : this(initialStates.AsSpan()) { }

        /// <summary>
        /// Constructor with custom initial state.
        /// <para>WARNING: DO NOT use this constructor unless you know exactly
        /// what you're doing!</para>
        /// </summary>
        /// <param name="initialStates">Span (minimum of 4 elements) containing the
        /// initial state</param>
        public XoShiRo256plus(ReadOnlySpan<ulong> initialStates)
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

        /* Overrides */

        /// <summary>
        /// Fetch an Unsigned 64-bit integer from the PRNG.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong Next64U() {
            ulong rslt = s0 + s3;

            ulong t = s1 << 17;
            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;
            // rotl := (x << k) | (x >> (64 - k))
            s3 = (s3 << 45) | (s3 >> 19); // rotl(s[3], 45);

            return rslt;
        }

    }
}
