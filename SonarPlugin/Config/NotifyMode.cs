using SonarPlugin.Attributes;

namespace SonarPlugin.Config
{
    public enum NotifyMode
    {
        [EnumCheapLoc("NotifyModeDefault", "Default")]
        Default,

        [EnumCheapLoc("NotifyModeSingle", "Single")]
        Single,

        [EnumCheapLoc("NotifyModeMultiple", "Multiple")]
        Multiple
    }
}
