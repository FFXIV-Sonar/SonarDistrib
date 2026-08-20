using AG.EnumLocalization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Localization
{
    [EnumLocStrings("Localization")]
    public enum PluginLocLoc
    {
        [EnumLoc(Fallback = "Import language file")]
        ImportPrompt,

        [EnumLoc(Fallback = "Export language file")]
        ExportPrompt,

        [EnumLoc(Fallback = "디버그 기본값")]
        DebugFallbacks,

        [EnumLoc(Fallback = "디버그 기본값 사용, 키 표시.")]
        DebugFallbacksTooltip,

        [EnumLoc(Fallback = "Minified export")]
        MinifiedExport,

        [EnumLoc(Fallback = "Run language setup in a separate thread.")]
        ThreadedTooltip,
    }
}
