using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xoshiro.PRNG64;

namespace SonarUtils.Random
{
    public static class RandomUtils
    {
        private static readonly ThreadLocal<XoShiRo256starstar> s_random = new(CreateRandom);
        private static readonly ThreadLocal<RandomNumberGenerator> s_cryptoRandom = new(CreateCryptoRandom);
        private static readonly ThreadLocal<ulong[]> s_stateArray = new(() => new ulong[4]);

        public static XoShiRo256starstar CreateRandom()
        {
            var state = s_stateArray.Value!;
            GetThreadCryptoRandom().GetBytes(MemoryMarshal.AsBytes(state.AsSpan()));
            return new(state);
        }

        public static RandomNumberGenerator CreateCryptoRandom()
        {
            return RandomNumberGenerator.Create();
        }

        public static XoShiRo256starstar GetThreadRandom() => s_random.Value!;
        public static RandomNumberGenerator GetThreadCryptoRandom() => s_cryptoRandom.Value!;
    }
}
