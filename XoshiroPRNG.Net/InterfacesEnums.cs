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

namespace Xoshiro.Base {
    /// <summary>
    /// The "Unleashed" interface for 32-bit PRNGs: This interface enables
    /// fetching of higher-strength full-range random numbers direct from the 
    /// PRNG's algorithm.
    /// </summary>
    public interface IRandomU {
        #region System.Random-compatible interface

        /// <summary>
        /// Returns a non-negative 32-bit integer from the PRNG, in the range of [0, int.MaxValue)
        /// </summary>
        int Next();

        /// <summary>
        /// Returns a non-negative 32-bit integer from the PRNG, in the range of [0, maxValue)
        /// </summary>
        /// <param name="maxValue">Must be &gt;0</param>
        int Next(int maxValue);

        /// <summary>
        /// Returns the next 32-bit integer from the PRNG, in the range of [minValue, maxValue).
        /// Can be negative.
        /// </summary>
        /// <param name="minValue">Must be &lt;= maxValue. Can be negative.</param>
        /// <param name="maxValue">Must be &gt;= minValue. Can be negative</param>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Fills an array of bytes with the octets of the next number(s) from the PRNG
        /// </summary>
        /// <param name="buffer">Must NOT be null</param>
        void NextBytes(byte[] buffer);

        /// <summary>
        /// Fills an array of bytes with the octets of the next number(s) from the PRNG
        /// </summary>
        /// <param name="buffer">Must NOT be null</param>
        void NextBytes(Span<byte> buffer);

        /// <summary>
        /// Returns a double precision floating-point number from the PRNG.
        /// Range is [0, 1.0)
        /// </summary>
        double NextDouble();

        #endregion System.Random-compatible interface

        #region Unleashed interface

        /// <summary>
        /// Returns a single precision floating-point number by using 24 of the 32 bits
        /// of the next number from the PRNG. Range is [0, 1.0)
        /// </summary>
        float NextFloat();

        /// <summary>
        /// Returns an unsigned 32-bit integer in the range of [0, uint.MaxValue]
        /// inclusive at both ends ("full range")
        /// </summary>
        uint NextU();

        /// <summary>
        /// Returns an unsigned 32-bit integer in the range of [0, maxValue)
        /// </summary>
        /// <param name="maxValue">Must be &gt;1</param>
        uint NextU(uint maxValue);

        /// <summary>
        /// Returns an unsigned 32-bit integer in the range of [minValue, maxValue)
        /// </summary>
        /// <param name="minValue">Must be &lt; maxValue-1</param>
        /// <param name="maxValue">Must be &gt; minValue+1</param>
        uint NextU(uint minValue, uint maxValue);

        /// <summary>
        /// Returns a signed 64-bit integer (63 bits of randomness) in the range of
        /// [0, long.MaxValue] inclusive at both ends ("full range")
        /// </summary>
        long Next64();

        /// <summary>
        /// Returns a signed 64-bit integer (63 bits of randomness) in the range of
        /// [0, long.MaxValue)
        /// </summary>
        /// <param name="maxValue">Must be &gt;1</param>
        long Next64(long maxValue);

        /// <summary>
        /// Returns a signed 64-bit integer (63 bits of randomness) in the range of
        /// [minValue, maxValue), can be negative.
        /// </summary>
        /// <param name="minValue">Must be &lt; maxValue-1; can be negative</param>
        /// <param name="maxValue">Must be &gt; minValue+1; can be negative</param>
        long Next64(long minValue, long maxValue);

        /// <summary>
        /// Returns an unsigned 64-bit integer in the range of [0, ulong.MaxValue]
        /// inclusive at both ends.
        /// </summary>
        ulong Next64U();

        /// <summary>
        /// Returns an unsigned 64-bit integer in the range of [0, maxValue)
        /// </summary>
        /// <param name="maxValue">Must be &gt;1</param>
        ulong Next64U(ulong maxValue);

        /// <summary>
        /// Returns an unsigned 64-bit integer in the range of [minValue, maxValue)
        /// </summary>
        ulong Next64U(ulong minValue, ulong maxValue);

        /// <summary>
        /// Get a System.Random-compatible interface
        /// </summary>
        /// <returns></returns>
        Random GetRandomCompatible();

        #endregion Unleashed interface
    }

    /// <summary>
    /// The "Unleashed" interface for 64-bit PRNGs: This interface enables
    /// fetching of higher-strength full-range random numbers direct from the 
    /// PRNG's algorithm.
    /// </summary>
    public interface IRandom64U : IRandomU {
        #region Unleashed interface

        /// <summary>
        /// Specifies how to 'fold' 64-bit numbers generated by the PRNG into
        /// 32-bit numbers. <para>See <see cref="Fold64To32Method"/> for an explanation
        /// of the methods</para>
        /// </summary>
        /// <see cref="Fold64To32Method"/>
        Fold64To32Method FoldMethod { get; set; }

        #endregion Unleashed interface
    }

    /// <summary>
    /// Methods of folding a 64-bit value into a 32-bit value
    /// </summary>
    public enum Fold64To32Method {
        /// <summary>
        /// XOR the upper and lower parts of the 64-bit value into a 32-bit value
        /// </summary>
        XorMethod,
        /// <summary>
        /// Returns the upper 32-bit part first, then the next iteration return the lower part
        /// </summary>
        ChunkMethod,
        /// <summary>
        /// Returns the lower 32-bit part first, then the next iteration return the upper part
        /// </summary>
        ChunkMethod2
    }
}
