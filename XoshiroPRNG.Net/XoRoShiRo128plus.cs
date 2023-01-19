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
 * This code is based on the source code for SplitMix64 as publicized in:
 * http://xoshiro.di.unimi.it/xoshiro128plus.c
 * 
 */
using System;
using System.Runtime.CompilerServices;
using Xoshiro.Base;

namespace Xoshiro.PRNG64 {
    /* Comment from original source:
     * 
     * This is xoroshiro128+ 1.0, our best and fastest small-state generator
     * for floating-point numbers. We suggest to use its upper bits for
     * floating-point generation, as it is slightly faster than
     * xoroshiro128**. It passes all tests we are aware of except for the four
     * lower bits, which might fail linearity tests (and just those), so if
     * low linear complexity is not considered an issue (as it is usually the
     * case) it can be used to generate 64-bit outputs, too; moreover, this
     * generator has a very mild Hamming-weight dependency making our test
     * (http://prng.di.unimi.it/hwd.php) fail after 5 TB of output; we believe
     * this slight bias cannot affect any application. If you are concerned,
     * use xoroshiro128** or xoshiro256+.
     * 
     * We suggest to use a sign test to extract a random Boolean value, and
     * right shifts to extract subsets of bits.
     * 
     * The state must be seeded so that it is not everywhere zero. If you have
     * a 64-bit seed, we suggest to seed a splitmix64 generator and use its
     * output to fill s. 
     * 
     * NOTE: the parameters (a=24, b=16, b=37) of this version give slightly
     * better results in our test than the 2016 version (a=55, b=14, c=36).
     * 
     */
    /// <summary>
    /// A very fast PRNG with 128-bit state, suitable for generating random floating-point numbers.
    /// </summary>
    /// <remarks>
    /// <para>Due to some apparent weaknesses of the output's lower-orded bits, it is NOT
    /// recommended to use this PRNG to generate random 64-bit integers. (Double precision
    /// floating-point numbers only use the upper 53 bits.)</para>
    /// </remarks>
    public sealed class XoRoShiRo128plus : Xoshiro64Base {
        #region Internal States

        private ulong s0;
        private ulong s1;

        private const int NUM_STATES = 2;

        #endregion Internal States

        #region Constructors

        /// <summary>
        /// Default (Null) Constructor with seed taken from DateTime.UtcNow.Ticks
        /// </summary>
        public XoRoShiRo128plus() : this(DateTime.UtcNow.Ticks) { }

        /// <summary>
        /// Constructor with custom seed, which will be expanded into states
        /// by the SplitMix64 algorithm.
        /// </summary>
        /// <param name="seed">seed to initialize the state of SplitMix64</param>
        public XoRoShiRo128plus(long seed) {
            Span<ulong> s = stackalloc ulong[NUM_STATES];
            SplitMix64 sm64 = new SplitMix64(seed);
            sm64.Fill(s);
            s0 = s[0];
            s1 = s[1];
        }

        /// <summary>
        /// Constructor with custom initial state.
        /// <para>WARNING: DO NOT use this constructor unless you know exactly
        /// what you're doing!</para>
        /// </summary>
        /// <param name="initialStates">Array (minimum of 2 elements) containing the
        /// initial state</param>
        public XoRoShiRo128plus(ulong[] initialStates) : this(initialStates.AsSpan()) { }

        /// <summary>
        /// Constructor with custom initial state.
        /// <para>WARNING: DO NOT use this constructor unless you know exactly
        /// what you're doing!</para>
        /// </summary>
        /// <param name="initialStates">Span (minimum of 2 elements) containing the
        /// initial state</param>
        public XoRoShiRo128plus(ReadOnlySpan<ulong> initialStates)
        {
            if (initialStates.Length < NUM_STATES) throw new ArgumentException(
               $"initialStates must have at least {NUM_STATES} elements!",
               nameof(initialStates));
            s0 = initialStates[0];
            s1 = initialStates[1];
        }

        #endregion Constructors

        /* Overrides */

        /// <summary>
        /// Fetch an Unsigned 64-bit integer from the PRNG.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ulong Next64U() {
            // rotl := (x << k) | (x >> (64 - k));

            ulong _s0 = s0;
            ulong _s1 = s1;
            ulong rslt = _s0 + _s1;

            _s1 ^= _s0;
            // rotl(s0, 24) ^ s1 ^ (s1 << 16); // a, b
            s0 = ((_s0 << 24) | (_s0 >> 40)) ^ _s1 ^ (_s1 << 16);
            // rotl(s1, 37); // c
            s1 = (_s1 << 37) | (_s1 >> 27);

            return rslt;
        }
    }
}
