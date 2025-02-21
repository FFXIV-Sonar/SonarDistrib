using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils
{
    /// <summary>Instance symbols kind.</summary>
    public enum InstanceSymbolKind
    {
        /// <summary>Do not use symbols.</summary>
        /// <remarks><strong>Range</strong>: None</remarks>
        None,

        /// <summary>Game symbols.</summary>
        /// <remarks>
        /// <para><strong>Range</strong>: <c>1</c>-<c>9</c></para>
        /// <para>These symbols only works in-game.</para>
        /// </remarks>
        Game,

        /// <summary>Circled symbols.</summary>
        /// <remarks><strong>Range</strong>: <c>1</c> - <c>50</c></remarks>
        Circled,
    };
}
