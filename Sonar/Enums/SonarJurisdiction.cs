using AG.EnumLocalization.Attributes;

namespace Sonar.Enums
{
    [EnumLocStrings]
    public enum SonarJurisdiction : byte
    {
        [EnumLoc(Fallback = "기본값")]
        /// <summary>Default jurisdiction</summary>
        Default,

        [EnumLoc(Fallback = "사용 안 함")]
        /// <summary>None</summary>
        None,

        [EnumLoc(Fallback = "인스턴스")]
        /// <summary>Current Instance</summary>
        Instance,

        [EnumLoc(Fallback = "지역")]
        /// <summary>Current Zone</summary>
        Zone,

        [EnumLoc(Fallback = "서버")]
        /// <summary>Current World</summary>
        World,

        [EnumLoc(Fallback = "데이터 센터")]
        /// <summary>Current Datacenter</summary>
        Datacenter,

        [EnumLoc(Fallback = "국가")]
        /// <summary>Current Region</summary>
        Region, // US, UK, JP, CN, KR

        [EnumLoc(Fallback = "글로벌, 중국, 한국 서버")]
        /// <summary>Current Audience</summary>
        Audience, // Global, CN, KR

        [EnumLoc(Fallback = "전체")]
        /// <summary>Everything</summary>
        All,
    }
}
