using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources.Lgb
{
    public enum LgbResult
    {
        /// <summary>+ New entries added</summary>
        Added, // +

        /// <summary>- No new entries added</summary>
        Missed, // -

        /// <summary>e Exception occurred</summary>
        Error, // e

        /// <summary>_ Not found</summary>
        NotFound, // _

        /// <summary>. Skipped</summary>
        Skipped, // .
    }
}
