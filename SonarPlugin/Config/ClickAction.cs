using AG.EnumLocalization.Attributes;
using SonarPlugin.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Config
{
    [EnumLocStrings]
    public enum ClickAction
    {
        [EnumLoc(Fallback = "Default")]
        Default,

        [EnumLoc(Fallback = "None")]
        None,

        [EnumLoc(Fallback = "Send to Chat")]
        Chat,

        [EnumLoc(Fallback = "Create Flag")]
        Map,

        [EnumLoc(Fallback = "Teleport")]
        Teleport,

        [EnumLoc(Fallback = "Remove")]
        Remove,
    }
}
