using AG.EnumLocalization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Config
{
    [EnumLocStrings("Modifier")]
    public enum KeyModifier
    {
        None,

        Ctrl,

        Shift,

        Alt,
    }
}
