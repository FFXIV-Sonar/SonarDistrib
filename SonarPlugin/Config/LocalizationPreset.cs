using AG.EnumLocalization.Attributes;

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

        [EnumLoc(Fallback = "Chinese Simplified")]
        Chinese, // Simplified

        [EnumLoc(Fallback = "Chinese Traditional")]
        ChineseTraditional, // Traditional

        Korean = 8,
    }
}