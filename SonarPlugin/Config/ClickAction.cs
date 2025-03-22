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
        [EnumCheapLoc("ClickActionDefault", "기본값")]
        Default,

        [EnumCheapLoc("ClickActionNone", "사용 안 함")]
        None,

        [EnumCheapLoc("ClickActionChat", "대화 창에 전송")]
        Chat,

        [EnumCheapLoc("ClickActionMap", "지도에 표시")]
        Map,

        [EnumCheapLoc("ClickActionTeleport", "텔레포")]
        Teleport,
    }
}
