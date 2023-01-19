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
 * http://xoshiro.di.unimi.it/splitmix64.c
 * 
 */
using System;

namespace Xoshiro.Base {

    /* Comment from original source:
     * 
     * This is a fixed-increment version of Java 8's SplittableRandom generator
     * See http://dx.doi.org/10.1145/2714064.2660195 and 
     * http://docs.oracle.com/javase/8/docs/api/java/util/SplittableRandom.html
     *
     * It is a very fast generator passing BigCrush, and it can be useful if
     * for some reason you absolutely want 64 bits of state; otherwise, we
     * rather suggest to use a xoroshiro128+ (for moderately parallel
     * computations) or xorshift1024* (for massively parallel computations)
     * generator.
     */

    /// <summary>
    /// A generator to quickly generate initial states for the PRNGs.
    /// DO NOT USE THIS CLASS AS A PRNG.
    /// </summary>
    public sealed class SplitMix64
    {

        /* Primitive Properties */
        /// <summary>
        /// Specifies the method used to fold 64-bit integers into 32-bit integers.
        /// </summary>
        public Fold64To32Method FoldMethod { get; set; }

        /* State */
        private ulong x;

        /* Constructor - NO Null Constructor */
        /// <summary>
        /// Instantiates a SplitMix64 object with a given initial state.
        /// </summary>
        /// <param name="seed">Initial State</param>
        /// <param name="foldMethod"><see cref="Fold64To32Method"/></param>
        public SplitMix64(long seed,
            Fold64To32Method foldMethod = Fold64To32Method.ChunkMethod)
        {
            x = (ulong)seed;
            this.FoldMethod = foldMethod;
        }

        /* Public Methods */

        /// <summary>
        /// Get one ulong number from the SplitMix64 generator
        /// </summary>
        public ulong Next()
        {
            ulong z = (x += 0x9e3779b97f4a7c15);
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
            z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// Fill an array of ulong with the next numbers from the SplitMix64 generator
        /// </summary>
        /// <param name="arr">Must not be null</param>
        public void Fill(ulong[] arr) => Fill(arr.AsSpan());

        /// <summary>
        /// Fill a span of ulong with the next numbers from the SplitMix64 generator
        /// </summary>
        /// <param name="span">Must not be null</param>
        public void Fill(Span<ulong> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = Next();
            }
        }

        /// <summary>
        /// Fill an array of uint by folding the next numbers from the SplitMix64 generator
        /// </summary>
        /// <param name="arr">Must not be null</param>
        public void Fill(uint[] arr) => Fill(arr.AsSpan());

        /// <summary>
        /// Fill a span of uint by folding the next numbers from the SplitMix64 generator
        /// </summary>
        /// <param name="span">Must not be null</param>
        public void Fill(Span<uint> span)
        {
            if (FoldMethod == Fold64To32Method.XorMethod) FillXorMethod(span);
            else if (FoldMethod == Fold64To32Method.ChunkMethod) FillChunkMethod(span);
            else throw new NotSupportedException("Unrecognized Fold64To32 method!");
        }

        /// <summary>
        /// Fill an array of uint by folding the next numbers from the SplitMix64 generator using the Xor method
        /// </summary>
        /// <param name="arr">Must not be null</param>
        public void FillXorMethod(Span<uint> arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                ulong n = Next();
                arr[i] = (uint)(n & 0xFFFFFFFF) ^ (uint)(n >> 32);
            }
        }

        /// <summary>
        /// Fill an array of uint by folding the next numbers from the SplitMix64 generator using the Chunked method
        /// </summary>
        /// <param name="arr">Must not be null</param>
        public void FillChunkMethod(Span<uint> arr)
        {
            var arrLengthMinus1 = arr.Length - 1;
            for (int i = 0; i < arrLengthMinus1; i += 2)
            {
                ulong n = Next();
                arr[i] = (uint)(n >> 32);
                arr[i + 1] = (uint)(n & 0xFFFFFFFF);
            }
            if (arr.Length % 2 != 0)
            {
                ulong n = Next();
                arr[arr.Length - 1] = (uint)(n >> 32);
            }
        }

        /// <summary>
        /// Fill an array of uint by folding the next numbers from the SplitMix64 generator using the Chunked method
        /// </summary>
        /// <param name="arr">Must not be null</param>
        public void FillChunkMethod2(Span<uint> arr)
        {
            var arrLengthMinus1 = arr.Length - 1;
            for (int i = 0; i < arrLengthMinus1; i += 2)
            {
                ulong n = Next();
                arr[i] = (uint)(n & 0xFFFFFFFF);
                arr[i + 1] = (uint)(n >> 32);
            }
            if (arr.Length % 2 != 0)
            {
                ulong n = Next();
                arr[arr.Length - 1] = (uint)(n & 0xFFFFFFFF);
            }
        }
    }
}
