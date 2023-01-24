using Sonar.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Game
{
    public static class PositionExtensions
    {
        /// <summary>
        /// Swap the Y and Z coordinates
        /// </summary>
        public static Vector3 SwapYZ(this Vector3 vec) => new(vec.X, vec.Z, vec.Y);

        /// <summary>
        /// Swap the Y and Z coordinates
        /// </summary>
        public static SonarVector3 SwapYZ(this SonarVector3 vec) => new(vec.X, vec.Z, vec.Y);
    }
}
