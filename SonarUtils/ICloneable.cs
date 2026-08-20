using System;

namespace SonarUtils
{
    public interface ICloneable<T> : ICloneable
    {
        /// <summary>Creates a copy of this instance.</summary>
        /// <returns>Copy of this instance.</returns>
        public new T Clone();
    }
}
