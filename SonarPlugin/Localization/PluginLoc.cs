using AG.EnumLocalization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Localization
{
    [EnumLocStrings("Plugin")]
    public enum PluginLoc
    {
        Plugin,

        Language,

        Advanced,

        Fallbacks,

        Presets,

        Resources,

        Import,

        Export,

        [EnumLoc(Fallback = "Export Fallbacks")]
        ExportFallbacks,

        Load,

        Save,

        [EnumLoc(Fallback = "Run Setup")]
        RunSetup,
        
        Debug,
    }
}
