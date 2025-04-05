using AG.EnumLocalization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    [EnumLocStrings]
    public enum ReleaseMode : byte
    {
        /// <summary>Release normally</summary>
        Normal,

        /// <summary>Hold for some time longer</summary>
        Hold,

        /// <summary>Release instantly</summary>
        Forced,
    }
}
