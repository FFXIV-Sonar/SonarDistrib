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
        [EnumLoc(Fallback = "플러그인")]
        Plugin,

        [EnumLoc(Fallback = "언어")]
        Language,

        [EnumLoc(Fallback = "고급 설정")]
        Advanced,

        [EnumLoc(Fallback = "기본값")]
        Fallbacks,

        [EnumLoc(Fallback = "프리셋")]
        Presets,

        [EnumLoc(Fallback = "리소스")]
        Resources,

        [EnumLoc(Fallback = "불러오기")]
        Import,

        [EnumLoc(Fallback = "내보내기")]
        Export,

        [EnumLoc(Fallback = "기본값 내보내기")]
        ExportFallbacks,

        Load,

        Save,

        [EnumLoc(Fallback = "설정")]
        RunSetup,

        [EnumLoc(Fallback = "디버그")]
        Debug,
    }
}
