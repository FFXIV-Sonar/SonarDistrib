using SonarPlugin.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Config
{
    public enum ClickAction
    {
        [EnumCheapLoc("ClickActionDefault", "Default")]
        Default,

        [EnumCheapLoc("ClickActionNone", "None")]
        None,

        [EnumCheapLoc("ClickActionChat", "Send to Chat")]
        Chat,

        [EnumCheapLoc("ClickActionMap", "Create Flag")]
        Map,

        [EnumCheapLoc("ClickActionTeleport", "Teleport")]
        Teleport,

        [EnumCheapLoc("ClickActionRemove", "Remove")]
        Remove,
    }
}
