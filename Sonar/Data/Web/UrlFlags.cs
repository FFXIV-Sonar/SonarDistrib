using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data.Web
{
    [Flags]
    public enum UrlFlags
    {
        /// <summary>No flags</summary>
        None = 0,

        /// <summary>https://assets.ffxivsonar.com</summary>
        IncludeBase = 1,

        /// <summary>Directory path</summary>
        IncludePath = 2,

        /// <summary>IncludeBase + IncludePath</summary>
        Default = 3,
    }
}
