using MessagePack;
using Sonar.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sonar.Data.Rows
{
    [MessagePackObject]
    public sealed class PositionData
    {
        /// <summary>Raw coordinates.</summary>
        [Key(0)]
        public SonarVector3 Coords { get; init; }

        /// <summary>Radius of this flag.</summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><c>0</c>: Pinpoint flag.</item>
        /// <item><c>&gt;0</c>: Fate or any other region radius.</item>
        /// <item><c>&lt;0</c>: (Unused).</item>
        /// </list>
        /// </remarks>
        [Key(1)]
        public SonarVector3 Scale { get; init; }

        [Key(2)]
        public RadiusType RadiusType { get; init; }
    }
}
