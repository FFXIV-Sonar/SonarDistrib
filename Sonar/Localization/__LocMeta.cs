using AG.EnumLocalization.Attributes;
using System.ComponentModel;

namespace Sonar.Localization
{
    [EnumLocStrings("__Meta")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum __LocMeta
    {
        [EnumLoc(Fallback = "Sonar Team")]
        Author,

        [EnumLoc(Fallback = "English")]
        Language,

        [EnumLoc(Fallback = "Default sonar fallbacks")]
        Notes,
    }
}
