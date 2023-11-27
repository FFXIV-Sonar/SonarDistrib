using System;
using System.Threading;

namespace Sonar.Numerics
{
    // License for SonarMath: MIT, feel free to use

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3358:Ternary operators should not be nested", Justification = "Intentional")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S4136:Method overloads should be grouped together", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1905:Redundant casts should not be used", Justification = "<Pending>")]
    public static class SonarMath
    {
        private static readonly ThreadLocal<Random> _random = new(() => new Random());

        /// <summary>
        /// Normal thread local random
        /// </summary>
        public static Random Random => _random.Value!;
        
        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static sbyte Clamp(this sbyte value, sbyte min, sbyte max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static sbyte ClampMin(this sbyte value, sbyte min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static sbyte ClampMax(this sbyte value, sbyte max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this sbyte value1, sbyte value2, sbyte tolerance = (sbyte)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static sbyte Min(this sbyte v1, sbyte v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static sbyte Max(this sbyte v1, sbyte v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static byte Clamp(this byte value, byte min, byte max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static byte ClampMin(this byte value, byte min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static byte ClampMax(this byte value, byte max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this byte value1, byte value2, byte tolerance = (byte)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static byte Min(this byte v1, byte v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static byte Max(this byte v1, byte v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static char Clamp(this char value, char min, char max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static char ClampMin(this char value, char min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static char ClampMax(this char value, char max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this char value1, char value2, char tolerance = (char)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static char Min(this char v1, char v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static char Max(this char v1, char v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static short Clamp(this short value, short min, short max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static short ClampMin(this short value, short min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static short ClampMax(this short value, short max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this short value1, short value2, short tolerance = (short)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static short Min(this short v1, short v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static short Max(this short v1, short v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static ushort Clamp(this ushort value, ushort min, ushort max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static ushort ClampMin(this ushort value, ushort min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static ushort ClampMax(this ushort value, ushort max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this ushort value1, ushort value2, ushort tolerance = (ushort)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static ushort Min(this ushort v1, ushort v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static ushort Max(this ushort v1, ushort v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static int ClampMin(this int value, int min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static int ClampMax(this int value, int max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this int value1, int value2, int tolerance = (int)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static int Min(this int v1, int v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static int Max(this int v1, int v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static uint Clamp(this uint value, uint min, uint max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static uint ClampMin(this uint value, uint min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static uint ClampMax(this uint value, uint max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this uint value1, uint value2, uint tolerance = (uint)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static uint Min(this uint v1, uint v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static uint Max(this uint v1, uint v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static long Clamp(this long value, long min, long max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static long ClampMin(this long value, long min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static long ClampMax(this long value, long max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this long value1, long value2, long tolerance = (long)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static long Min(this long v1, long v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static long Max(this long v1, long v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static ulong Clamp(this ulong value, ulong min, ulong max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static ulong ClampMin(this ulong value, ulong min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static ulong ClampMax(this ulong value, ulong max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this ulong value1, ulong value2, ulong tolerance = (ulong)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static ulong Min(this ulong v1, ulong v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static ulong Max(this ulong v1, ulong v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static float Clamp(this float value, float min, float max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static float ClampMin(this float value, float min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static float ClampMax(this float value, float max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this float value1, float value2, float tolerance = (float)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static float Min(this float v1, float v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static float Max(this float v1, float v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static double Clamp(this double value, double min, double max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static double ClampMin(this double value, double min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static double ClampMax(this double value, double max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this double value1, double value2, double tolerance = (double)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static double Min(this double v1, double v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static double Max(this double v1, double v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static decimal Clamp(this decimal value, decimal min, decimal max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static decimal ClampMin(this decimal value, decimal min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static decimal ClampMax(this decimal value, decimal max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this decimal value1, decimal value2, decimal tolerance = (decimal)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static decimal Min(this decimal v1, decimal v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static decimal Max(this decimal v1, decimal v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static nint Clamp(this nint value, nint min, nint max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static nint ClampMin(this nint value, nint min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static nint ClampMax(this nint value, nint max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this nint value1, nint value2, nint tolerance = (nint)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static nint Min(this nint v1, nint v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static nint Max(this nint v1, nint v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Clamps the value between a minimum and maximum value
        /// </summary>
        public static nuint Clamp(this nuint value, nuint min, nuint max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Clamps the value with a minimum value
        /// </summary>
        public static nuint ClampMin(this nuint value, nuint min) => value < min ? min : value;

        /// <summary>
        /// Clamps the value with a maximum value
        /// </summary>
        public static nuint ClampMax(this nuint value, nuint max) => value > max ? max : value;

        /// <summary>
        /// Check if the value roughly equals another value
        /// </summary>
        public static bool RoughlyEquals(this nuint value1, nuint value2, nuint tolerance = (nuint)1) => value1 >= value2 - tolerance && value1 <= value2 + tolerance;

        /// <summary>
        /// Return the lowest of two values
        /// </summary>
        public static nuint Min(this nuint v1, nuint v2) => v1 < v2 ? v1 : v2;

        /// <summary>
        /// Return the highest of two values
        /// </summary>
        public static nuint Max(this nuint v1, nuint v2) => v1 > v2 ? v1 : v2;


        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static sbyte Abs(this sbyte value) => value < (sbyte)0 ? (sbyte)(-value) : (sbyte)value;

        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static short Abs(this short value) => value < (short)0 ? (short)(-value) : (short)value;

        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static int Abs(this int value) => value < (int)0 ? (int)(-value) : (int)value;

        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static long Abs(this long value) => value < (long)0 ? (long)(-value) : (long)value;

        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static float Abs(this float value) => value < (float)0 ? (float)(-value) : (float)value;

        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static double Abs(this double value) => value < (double)0 ? (double)(-value) : (double)value;

        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static decimal Abs(this decimal value) => value < (decimal)0 ? (decimal)(-value) : (decimal)value;

        /// <summary>
        /// Return the absolute value
        /// </summary>
        public static nint Abs(this nint value) => value < (nint)0 ? (nint)(-value) : (nint)value;


        /// <summary>
        /// Return the value's ceiling
        /// </summary>
        public static float Ceil(this float value) => MathF.Ceiling(value);

        /// <summary>
        /// Return the value's floor
        /// </summary>
        public static float Floor(this float value) => MathF.Floor(value);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static float Round(this float value) => MathF.Round(value);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static float Round(this float value, MidpointRounding midpoint) => MathF.Round(value, midpoint);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static float Round(this float value, int digits) => MathF.Round(value, digits);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static float Round(this float value, int digits, MidpointRounding midpoint) => MathF.Round(value, digits, midpoint);

        /// <summary>
        /// Return truncated value
        /// </summary>
        public static float Truncate(this float value) => MathF.Truncate(value);

        /// <summary>
        /// Return the value's ceiling
        /// </summary>
        public static double Ceil(this double value) => Math.Ceiling(value);

        /// <summary>
        /// Return the value's floor
        /// </summary>
        public static double Floor(this double value) => Math.Floor(value);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static double Round(this double value) => Math.Round(value);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static double Round(this double value, MidpointRounding midpoint) => Math.Round(value, midpoint);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static double Round(this double value, int digits) => Math.Round(value, digits);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static double Round(this double value, int digits, MidpointRounding midpoint) => Math.Round(value, digits, midpoint);

        /// <summary>
        /// Return truncated value
        /// </summary>
        public static double Truncate(this double value) => Math.Truncate(value);

        /// <summary>
        /// Return the value's ceiling
        /// </summary>
        public static decimal Ceil(this decimal value) => Math.Ceiling(value);

        /// <summary>
        /// Return the value's floor
        /// </summary>
        public static decimal Floor(this decimal value) => Math.Floor(value);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static decimal Round(this decimal value) => Math.Round(value);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static decimal Round(this decimal value, MidpointRounding midpoint) => Math.Round(value, midpoint);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static decimal Round(this decimal value, int digits) => Math.Round(value, digits);

        /// <summary>
        /// Return rounded value
        /// </summary>
        public static decimal Round(this decimal value, int digits, MidpointRounding midpoint) => Math.Round(value, digits, midpoint);

        /// <summary>
        /// Return truncated value
        /// </summary>
        public static decimal Truncate(this decimal value) => Math.Truncate(value);


        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static sbyte Variate(this sbyte value, sbyte amount) => (sbyte)(value + Random.Next((int)-amount, (int)amount + 1)).Clamp(sbyte.MinValue, sbyte.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static byte Variate(this byte value, byte amount) => (byte)(value + Random.Next((int)-amount, (int)amount + 1)).Clamp(byte.MinValue, byte.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static char Variate(this char value, char amount) => (char)(value + Random.Next((int)-amount, (int)amount + 1)).Clamp(char.MinValue, char.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static short Variate(this short value, short amount) => (short)(value + Random.Next((int)-amount, (int)amount + 1)).Clamp(short.MinValue, short.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static ushort Variate(this ushort value, ushort amount) => (ushort)(value + Random.Next((int)-amount, (int)amount + 1)).Clamp(ushort.MinValue, ushort.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static int Variate(this int value, int amount) => (int)(value + Random.Next((int)-amount, (int)amount + 1)).Clamp(int.MinValue, int.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static uint Variate(this uint value, uint amount) => (uint)(value + Random.Next((int)-amount, (int)amount + 1)).Clamp(uint.MinValue, uint.MaxValue);


        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static long Variate(this long value, long amount) => (long)(value + Random.NextDouble() * amount * 2 - amount).Clamp(long.MinValue, long.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static ulong Variate(this ulong value, ulong amount) => (ulong)(value + Random.NextDouble() * amount * 2 - amount).Clamp(ulong.MinValue, ulong.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static nint Variate(this nint value, nint amount) => (nint)(value + Random.NextDouble() * amount * 2 - amount).Clamp(nint.MinValue, nint.MaxValue);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static nuint Variate(this nuint value, nuint amount) => (nuint)(value + Random.NextDouble() * amount * 2 - amount).Clamp(nuint.MinValue, nuint.MaxValue);


        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static float Variate(this float value, float amount) => (float)((double)value).Variate(amount);

        /// <summary>
        /// Return a value varied by up to the specified amount
        /// </summary>
        public static double Variate(this double value, double amount) => value + Random.NextDouble() * amount * 2 - amount;
    }
}
