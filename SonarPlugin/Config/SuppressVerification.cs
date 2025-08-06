using AG.EnumLocalization.Attributes;
using SonarPlugin.Attributes;
using System.Text;

namespace SonarPlugin.Config
{
    [EnumLocStrings]
    public enum SuppressVerification
    {
        [EnumLoc(Fallback = "Never")]
        None,

        [EnumLoc(Fallback = "Unless required")]
        UnlessRequired,

        [EnumLoc(Fallback = "Always")]
        Always
    }
}
