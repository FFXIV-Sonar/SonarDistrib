using AG.EnumLocalization.Attributes;

namespace Sonar.Enums
{
    [EnumLocStrings]
    public enum SonarJurisdiction : byte
    {
        /// <summary>Default jurisdiction</summary>
        Default,

        /// <summary>None</summary>
        None,

        /// <summary>Current Instance</summary>
        Instance,

        /// <summary>Current Zone</summary>
        Zone,

        /// <summary>Current World</summary>
        World,

        [EnumLoc(Fallback = "Data Center")]
        /// <summary>Current Datacenter</summary>
        Datacenter,

        /// <summary>Current Region</summary>
        Region, // US, UK, JP, CN, KR

        /// <summary>Current Audience</summary>
        Audience, // Global, CN, KR

        /// <summary>Everything</summary>
        All,
    }
}
