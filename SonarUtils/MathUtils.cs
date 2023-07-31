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

        public static bool IsPrime(int number)
        {
            if (number is 2 or 3) return true;
            if (number <= 1 || number % 2 == 0 || number % 3 == 0) return false;

            var limit = (int)Math.Sqrt(number);
            for (var i = 5L; i * i <= limit; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0) return false;
            }
            return true;
        }

        public static int FindPrime(int number)
        {
            if (number is 2 or 3) return number;
            if (number <= 1) return 2;
            number |= 1; // Make sure its odd
            for (; number < int.MaxValue; number += 2)
            {
                if (IsPrime(number)) return number;
            }
            throw new OverflowException();
        }
    }
}
