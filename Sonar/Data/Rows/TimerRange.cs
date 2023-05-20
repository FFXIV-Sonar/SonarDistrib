using MessagePack;
using System;
using static Sonar.SonarConstants;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Timer range
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public struct TimerRange
    {
        /// <summary>
        /// Converts a time value to a time string (HH:MM)
        /// </summary>
        public static string TimeToString(double time)
        {
            int hours = (int)(time / EarthHour);
            int minutes = (int)(time / EarthMinute) % 60;
            return $"{hours}:{minutes:D2}";
        }

        /// <summary>
        /// Construct a new <see cref="TimerRange"/> with a specified constant time
        /// </summary>
        public TimerRange(double constant) : this(constant, constant) { }

        /// <summary>
        /// Construct a new <see cref="TimerRange"/> with a specified minimum and maximum time
        /// </summary>
        public TimerRange(double minimum, double maximum)
        {
            if (minimum < 0) throw new ArgumentOutOfRangeException(nameof(minimum), $"{nameof(minimum)} must be positive");
            if (maximum < 0) throw new ArgumentOutOfRangeException(nameof(maximum), $"{nameof(maximum)} must be positive");
            if (minimum > maximum) (minimum, maximum) = (maximum, minimum);
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        /// <summary>
        /// Minimum time
        /// </summary>
        [Key(0)]
        public double Minimum { get; }

        /// <summary>
        /// Maximum / forced time
        /// </summary>
        [Key(1)]
        public double Maximum { get; }

        /// <summary>
        /// Window time (<see cref="Maximum"/> - <see cref="Minimum"/>)
        /// </summary>
        [IgnoreMember]
        public double Window => this.Maximum - this.Minimum;

        /// <summary>
        /// Calculates progress before window opening
        /// </summary>
        public double GetProgress(double time)
        {
            if (this.Minimum == 0) return time > 0 ? 1 : 0;
            return Math.Clamp(time / this.Minimum, 0, 1);
        }

        /// <summary>
        /// Calculates forced progress
        /// </summary>
        public double GetForcedProgress(double time)
        {
            if (this.Maximum == 0) return time > 0 ? 1 : 0;
            return Math.Clamp(time / this.Maximum, 0, 1);
        }

        /// <summary>
        /// Calculates window progress
        /// </summary>
        public double GetWindowProgress(double time)
        {
            if (this.Window == 0) return time > this.Minimum ? 1 : 0;
            return Math.Clamp((time - this.Minimum) / this.Window, 0, 1);
        }

        /// <summary>
        /// Gets a string representation of this time range
        /// </summary>
        public override string ToString()
        {
            return this.Minimum == this.Maximum ? TimeToString(this.Minimum) : $"{TimeToString(this.Minimum)}-{TimeToString(this.Maximum)}";
        }
    }
}
