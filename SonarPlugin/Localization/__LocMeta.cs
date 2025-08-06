using AG.EnumLocalization.Attributes;
using System.ComponentModel;

namespace SonarPlugin.Localization
{
    [EnumLocStrings("__Meta")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum __LocMeta
    {
        [EnumLoc(Fallback = "Sonar Team")]
        Author,

        [EnumLoc(Fallback = "English")]
        Language,

        [EnumLoc(Fallback = "Default plugin fallbacks")]
        Notes,
    }
}
