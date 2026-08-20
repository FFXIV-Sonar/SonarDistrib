using AG.EnumLocalization.Attributes;

namespace Sonar.Enums
{
    [EnumLocStrings]
    public enum ExpansionPack : byte
    {
        Unknown,

        [EnumLoc(Fallback = "신생 에오르제아")]
        ARealmReborn,

        [EnumLoc(Fallback = "창천의 이슈가르드")]
        Heavensward,

        [EnumLoc(Fallback = "홍련의 해방자")]
        Stormblood,

        [EnumLoc(Fallback = "칠흑의 반역자")]
        Shadowbringers,

        [EnumLoc(Fallback = "효월의 종언")]
        Endwalker,

        [EnumLoc(Fallback = "황금의 유산")]
        Dawntrail,
    }
}
