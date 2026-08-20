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
        [EnumLoc(Fallback = "기본값")]
        Default,

        [EnumLoc(Fallback = "사용 안 함")]
        None,

        [EnumLoc(Fallback = "대화 창에 전송")]
        Chat,

        [EnumLoc(Fallback = "지도에 표시")]
        Map,

        [EnumLoc(Fallback = "텔레포")]
        Teleport,

        [EnumLoc(Fallback = "삭제")]
        Remove,
    }
}
