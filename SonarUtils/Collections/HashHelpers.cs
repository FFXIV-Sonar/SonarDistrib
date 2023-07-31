using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils.Collections
{
    // Taken from: https://source.dot.net/#System.Collections/src/libraries/System.Private.CoreLib/src/System/Collections/HashHelpers.cs
    internal static class HashHelpers
    {
        /// <summary>Returns approximate reciprocal of the divisor: ceil(2**64 / divisor).</summary>
        /// <remarks>This should only be used on 64-bit.</remarks>
        public static ulong GetFastModMultiplier(uint divisor) => ulong.MaxValue / divisor + 1;


        /// <summary>Performs a mod operation using the multiplier pre-computed with <see cref="GetFastModMultiplier"/>.</summary>
        /// <remarks>This should only be used on 64-bit.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Major Code Smell", "S125")]
        public static uint FastMod(uint value, uint divisor, ulong multiplier)
        {
            // We use modified Daniel Lemire's fastmod algorithm (https://github.com/dotnet/runtime/pull/406),
            // which allows to avoid the long multiplication if the divisor is less than 2**31.
            Debug.Assert(divisor <= int.MaxValue);

            // This is equivalent of (uint)Math.BigMul(multiplier * value, divisor, out _). This version
            // is faster than BigMul currently because we only need the high bits.
            uint highbits = (uint)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);

            //Debug.Assert(highbits == value % divisor);
            // This assert can fail at ConcurrentHashSetSlim as a result of the sets array and its fastModMultiplier
            // being modified without synchronization. This error condition is handled over there.

            return highbits;
        }
    }
}
