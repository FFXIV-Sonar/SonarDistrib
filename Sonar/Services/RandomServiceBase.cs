using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sonar.Services
{
    public abstract class RandomServiceBase
    {
        public const string AllCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public const string VowelCharacters = "AEIOUaeiou";
        public const string ConsonantCharacters = "BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz";
        public const string HexCharacters = "0123456789ABCDEF";
        public const string Base64UriCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        public static readonly double u32Max = Math.Pow(2, 32);
        public static readonly float u32Maxf = (float)u32Max;
        public static readonly double u64Max = Math.Pow(2, 64);

        public abstract uint[] GetUints(int count);

        public uint GetUint()
        {
            return this.GetUints(1)[0];
        }

        public int[] GetInts(int count)
        {
            return this.GetUints(count).Select(u => unchecked((int)u)).ToArray();
        }

        public int GetInt()
        {
            return this.GetInts(1)[0];
        }

        public abstract ulong[] GetUlongs(int count);

        public ulong GetUlong()
        {
            return this.GetUlongs(1)[0];
        }

        public long[] GetLongs(int count)
        {
            return this.GetUlongs(count).Select(u => unchecked((long)u)).ToArray();
        }

        public long GetLong()
        {
            return this.GetLongs(1)[0];
        }

        public float[] GetFloats(int count)
        {
            return this.GetUints(count).Select(u => (float)u / u32Maxf).ToArray();
        }

        public float GetFloat()
        {
            return this.GetFloats(1)[0];
        }

        public double[] GetDoubles(int count)
        {
            return this.GetUlongs(count).Select(u => (double)u / u64Max).ToArray();
        }

        public double GetDouble()
        {
            return this.GetDoubles(1)[0];
        }
        
        public bool[] GetBools(int count, double chance = 0.5)
        {
            return this.GetDoubles(count).Select(d => d < chance).ToArray();
        }

        public bool GetBool(double chance = 0.5)
        {
            return this.GetBools(1, chance)[0];
        }

        public int[] GetRanges(int count, int max)
        {
            return this.GetFloats(count).Select(f => (int)(f * max)).ToArray();
        }

        public int[] GetRanges(int count, int min, int max)
        {
            return this.GetRanges(count, max - min).Select(i => i + min).ToArray();
        }

        public int GetRange(int max)
        {
            return this.GetRanges(1, max)[0];
        }

        public int GetRange(int min, int max)
        {
            return this.GetRanges(1, min, max)[0];
        }

        public byte[] GetBytes(int count)
        {
            int size = count / sizeof(uint);
            if (count % sizeof(uint) > 0) size++;
            uint[] src = this.GetUints(size);

            byte[] ret = new byte[size * sizeof(uint)];
            for (int index = 0; index < size; index++)
            {
                BitConverter.GetBytes(src[index]).CopyTo(ret, index * sizeof(uint));
            }
            Array.Resize(ref ret, count);
            return ret;
        }

        public string GetString(int count, string chars = AllCharacters)
        {
            int[] indexes = this.GetRanges(count, chars.Length);
            Span<char> ret = count < 64 ? stackalloc char[count] : new char[count];
            for (int index = 0; index < count; index++)
            {
                ret[index] = chars[indexes[index]];
            }
            return new string(ret);
        }
    }
}
