using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Relays
{
    public enum ManualRelayKind
    {
        /// <summary>
        /// Unknown / Unspecified
        /// </summary>
        Unknown,

        /// <summary>
        /// Hunt Train
        /// </summary>
        HuntTrain,

        /// <summary>
        /// Fate Train / Grind
        /// </summary>
        FateTrain,

        /// <summary>
        /// Role Play Event
        /// </summary>
        RolePlay,

        /// <summary>
        /// Social Event
        /// </summary>
        Social,

        /// <summary>
        /// Other
        /// </summary>
        Other = 255,
    }
}
