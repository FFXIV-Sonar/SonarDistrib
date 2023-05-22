using System;

namespace SonarUtils
{
    public static class MathUtils
    {
        public static int AlignPowerOfTwo(int value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        public static uint AlignPowerOfTwo(uint value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        public static long AlignPowerOfTwo(long value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return value + 1;
        }

        public static ulong AlignPowerOfTwo(ulong value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return value + 1;
        }

        public static int AddReturnOriginal(ref int value, int add)
        {
            var ret = value;
            value += add;
            return ret;
        }

        public static int SubstractReturnOriginal(ref int value, int add)
        {
            var ret = value;
            value -= add;
            return ret;
        }
    }
}
