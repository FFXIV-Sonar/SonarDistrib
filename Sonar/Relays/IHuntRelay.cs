using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Relays
{
    internal interface IHuntRelay : IRelay
    {
        /// <summary>Actor ID</summary>
        public uint ActorId { get; }

        /// <summary>Current HP</summary>
        public uint CurrentHp { get; }

        /// <summary>Max HP</summary>
        public uint MaxHp { get; }

        /// <summary>HP Percent</summary>
        public float HpPercent { get; }

        /// <summary>Kill Progress</summary>
        public float Progress { get; }
    }
}
