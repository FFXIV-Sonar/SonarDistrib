using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonar.Enums;

namespace Sonar
{
    public static class SonarConstants
    {
        /// <summary>
        /// Sonar.NET Protocol Version
        /// </summary>
        public const int SonarVersion = 5;

        /// <summary>
        /// Lowest instance ID
        /// </summary>
        public const uint LowestInstanceId = 0;

        /// <summary>
        /// Highest instance ID
        /// </summary>
        public const uint HighestInstanceId = 9;

        /// <summary>
        /// Sonar.NET Compilation Environment
        /// </summary>
#if DEBUG
        public const SonarBuildEnvironment Environment = SonarBuildEnvironment.Debug;
#else
        public const SonarBuildEnvironment Environment = SonarBuildEnvironment.Release;
#endif

        /// <summary>
        /// Is this a debug release?
        /// </summary>
        public const bool DebugBuild = Environment == SonarBuildEnvironment.Debug;

        /// <summary>
        /// Represents an invalid Actor ID
        /// </summary>
        public const uint InvalidActorId = 0xE0000000;

        /// <summary>
        /// Represent a Sonar Tick in milliseconds
        /// </summary>
        public const double SonarTick = 400;

        /// <summary>
        /// Minimum valid coordinate
        /// </summary>
        public const float MinCoord = -2048;

        /// <summary>
        /// Maximum valid coordinate
        /// </summary>
        public const float MaxCoord = 2048;

        /// <summary>
        /// Rough distance squared
        /// </summary>
        public const double RoughDistanceSquared = RoughDistance * RoughDistance;

        /// <summary>
        /// Rough distance
        /// </summary>
        public const double RoughDistance = 5;

        /// <summary>
        /// Rough FATE Progress
        /// </summary>
        public const byte RoughFateProgress = 10;

        /// <summary>
        /// Rough time
        /// </summary>
        public const double RoughTime = 5 * EarthSecond;

        /// <summary>
        /// Rough HP Percent
        /// </summary>
        public const float RoughHpPercent = 1;

        #region Earth time
        /// <summary>
        /// Represents an earth second in milliseconds
        /// </summary>
        public const double EarthSecond = 1000;

        /// <summary>
        /// Represents an earth minute in milliseconds
        /// </summary>
        public const double EarthMinute = EarthSecond * 60;

        /// <summary>
        /// Represents an earth hour in milliseconds
        /// </summary>
        public const double EarthHour = EarthMinute * 60;

        /// <summary>
        /// Represents an earth day in milliseconds
        /// </summary>
        public const double EarthDay = EarthHour * 24;

        /// <summary>
        /// Represents an earth week in milliseconds
        /// </summary>
        public const double EarthWeek = EarthDay * 7;

        /// <summary>
        /// Represents an earth year (approximately 365.2425 days) in milliseconds
        /// </summary>
        public const double EarthYear = EarthDay * 365.2425;

        /// <summary>
        /// Represents an earth month (EarthYear / 12) in milliseconds
        /// </summary>
        public const double EarthMonth = EarthYear / 12;
#endregion

#region Eorzean time
        /// <summary>
        /// Represents an eorzean minute (2 11/12 seconds) in milliseconds
        /// </summary>
        public const double EorzeanMinute = EarthSecond * (2f + 1f * (11f / 12f));

        /// <summary>
        /// Represents an eorzean second in milliseconds
        /// </summary>
        public const double EorzeanSecond = EorzeanMinute / 60;

        /// <summary>
        /// Represents an eorzean hour (bell) in milliseconds
        /// </summary>
        public const double EorzeanHour = EorzeanMinute * 60;

        /// <summary>
        /// Represents an eorzean day (sun) in milliseconds
        /// </summary>
        public const double EorzeanDay = EorzeanHour * 24;

        /// <summary>
        /// Represents an eorzean week in milliseconds
        /// </summary>
        public const double EorzeanWeek = EorzeanDay * 8;

        /// <summary>
        /// Represents an eorzean month (moon) in milliseconds
        /// </summary>
        public const double EorzeanMonth = EorzeanWeek * 4;

        /// <summary>
        /// Represents an eorzean year in milliseconds
        /// </summary>
        public const double EorzeanYear = EorzeanMonth * 12;

        /// <summary>
        /// Represents an eorzean bell (hour) in milliseconds
        /// </summary>
        public const double EorzeanBell = EorzeanHour;

        /// <summary>
        /// Repressents an eorzean day (sun) in milliseconds
        /// </summary>
        public const double EorzeanSun = EorzeanDay;

        /// <summary>
        /// Represents an eorzean moon (month) in milliseconds
        /// </summary>
        public const double EorzeanMoon = EorzeanMonth;
#endregion
    }
}
