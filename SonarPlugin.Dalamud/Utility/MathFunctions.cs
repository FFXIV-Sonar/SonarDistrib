using System;

namespace SonarPlugin.Utility
{
    public static class MathFunctions
    {
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            T result = value;
            if (value.CompareTo(min) < 0)
                result = min;
            if (value.CompareTo(max) > 0)
                result = max;
            return result;
        }
    }
}
