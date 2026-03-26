using SonarPlugin.Attributes;

namespace SonarPlugin.Config
{
    public enum NotifyMode
    {
        [EnumCheapLoc("NotifyModeDefault", "기본값")]
        Default,

        [EnumCheapLoc("NotifyModeSingle", "한번")]
        Single,

        [EnumCheapLoc("NotifyModeMultiple", "여러번")]
        Multiple
    }
}
