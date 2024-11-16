using MessagePack.Formatters;
using Sonar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Relays
{
    internal interface IFateRelay : IRelay
    {
        /// <summary>Fate Progress</summary>
        public byte Progress { get; }

        /// <summary>Fate Status</summary>
        public FateStatus Status { get; }

        /// <summary>Start Time</summary>
        public double StartTime { get; }

        /// <summary>Duration</summary>
        public double Duration { get; }

        /// <summary>End Time</summary>
        public double EndTime { get; }

        /// <summary>Bonus EXP</summary>
        public bool Bonus { get; }
    }
}
