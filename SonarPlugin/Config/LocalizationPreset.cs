using AG.EnumLocalization.Attributes;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Config
{
    [EnumLocStrings("Localization.Preset")]
    public enum LocalizationPreset
    {
        Undefined,

        Default,

        Japanese,

        English,

        German,

        French,

        //[EnumLoc(Fallback = "Chinese Simplified")]
        Chinese, // Simplified

        //[EnumLoc(Fallback = "Chinese Traditional")]
        // ChineseTraditional? // Traditional

        Korean = 8,
    }
}
