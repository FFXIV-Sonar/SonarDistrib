using AG.EnumLocalization.Attributes;

namespace Sonar.Enums
{
    [EnumLocStrings("Language")]
    public enum SonarLanguage
    {
        Default,

        // Global supported languages
        Japanese, // ja
        English, // en
        German, // de
        French, // fr

        // China languages
        [EnumLoc("Chinese Simplified")]
        ChineseSimplified, // zh_CN

        [EnumLoc("Chinese Traditional")]
        ChineseTraditional, // zh_TW

        // Korea languages
        Korean, // ko
    }
}
