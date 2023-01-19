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
 * This code is based on the source code for xoshiro128** as publicized in:
 * http://xoshiro.di.unimi.it/xoshiro128starstar.c
 * 
 */
using System;
using System.Runtime.CompilerServices;
using Xoshiro.Base;

namespace Xoshiro.PRNG32 {
    /* Comment from original source:
     * This is xoshiro128** 1.1, one of our 32-bit all-purpose, rock-solid generators.
     * It has excellent speed, a state size (128 bits) that is large enough for mild parallelism,
     * and it passes all tests we are aware of.
     * 
     * Note that version 1.0 had mistakenly s[0] instead of s[1] as state word passed to the scrambler.
     * 
     * For generating just single-precision (i.e., 32-bit) floating-point
     * numbers, xoshiro128+ is even faster.
     * 
     * The state must be seeded so that it is not everywhere zero.
     * 
     */
    /// <summary>
    /// A very fast PRNG with 128-bit state, suitable for generating random
    /// 32-bit integers and single precision floating-point numbers.
    /// </summary>
    /// <remarks>
    /// If, however, one has to generate only 32-bit floating-point numbers (by extracting
    /// the upper 23 bits) xoshiro128+ might be slightly faster, with analogous
    /// statistical properties. For general usage, one has to consider that [the plus version's]
    /// lowest bits have low linear complexity and will fail linearity tests.
    /// </remarks>
    public sealed class XoShiRo128starstar : Xoshiro32Base {
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
        public XoShiRo128starstar() : this(DateTime.UtcNow.Ticks) { }

        /// <summary>
        /// Constructor with custom seed, which will be expanded into states
        /// by the SplitMix64 algorithm.
        /// </summary>
        /// <param name="seed">seed to initialize the state of SplitMix64</param>
        public XoShiRo128starstar(long seed) {
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
        public XoShiRo128starstar(uint[] initialStates) : this(initialStates.AsSpan()) { }

        /// <summary>
        /// Constructor with custom initial state.
        /// <para>WARNING: DO NOT use this constructor unless you know exactly
        /// what you're doing!</para>
        /// </summary>
        /// <param name="initialStates">Span (minimum of 4 elements) containing the
        /// initial state</param>
        public XoShiRo128starstar(ReadOnlySpan<uint> initialStates)
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
        /// Fetch an Unsigned 32-bit integer from the PRNG.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint NextU() {
            // rotl := (x << k) | (x >> (32 - k))

            // rotl(s[1] * 5, 7) * 9 ==> decomposed into 3 steps:
            uint r1 = (s1 << 2) + s1;          // * 5 step
            uint r2 = (r1 << 7) | (r1 >> 25);  // shift left | shift right step
            uint rslt = (r2 << 3) + r2;        // * 9 step

            uint t = s1 << 9;

            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;

            // rotl(s[3], 11);
            s3 = (s3 << 11) | (s3 >> 21);

            return rslt;
        }
    }
}
