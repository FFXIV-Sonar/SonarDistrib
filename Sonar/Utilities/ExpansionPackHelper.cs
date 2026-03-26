using Sonar.Enums;

namespace Sonar.Utilities
{
    public static class ExpansionPackHelper
    {
        public static ExpansionPack GetExpansionPack(string expac) => expac.ToUpper() switch
        {
            // Long strings (as received from XIVAPI)
            "A REALM REBORN" => ExpansionPack.ARealmReborn,
            "HEAVENSWARD" => ExpansionPack.Heavensward,
            "STORMBLOOD" => ExpansionPack.Stormblood,
            "SHADOWBRINGERS" => ExpansionPack.Shadowbringers,
            "ENDWALKER" => ExpansionPack.Endwalker,
            "DAWNTRAIL" => ExpansionPack.Dawntrail,

            // Short strings (as we players abbreviate)
            "ARR" => ExpansionPack.ARealmReborn,
            "HW" => ExpansionPack.Heavensward,
            "SB" => ExpansionPack.Stormblood,
            "SHB" => ExpansionPack.Shadowbringers,
            "EW" => ExpansionPack.Endwalker,
            "DT" => ExpansionPack.Dawntrail,

            // Alternative abbreviations (because we players don't agree on one) and names
            "AREALMREBORN" => ExpansionPack.ARealmReborn, // Because of the enum
            "REALMREBORN" => ExpansionPack.ARealmReborn,
            "AR" => ExpansionPack.ARealmReborn,
            "RR" => ExpansionPack.ARealmReborn, // ...
            "STB" => ExpansionPack.Stormblood,
            "SBR" => ExpansionPack.Shadowbringers,
            "SHBR" => ExpansionPack.Shadowbringers,
            "SHADOWBRINGER" => ExpansionPack.Shadowbringers, // <== who knows...
            "ENDWALKERS" => ExpansionPack.Endwalker,
            "DAWNTRAILS" => ExpansionPack.Dawntrail,

            // In case we decide to use short names
            "REALM" => ExpansionPack.ARealmReborn,
            "REBORN" => ExpansionPack.ARealmReborn,
            "HEAVEN" => ExpansionPack.Heavensward,
            "HEAVENS" => ExpansionPack.Heavensward,
            "WARD" => ExpansionPack.Heavensward,
            "STORM" => ExpansionPack.Stormblood,
            "BLOOD" => ExpansionPack.Stormblood,
            "SHADOW" => ExpansionPack.Shadowbringers,
            "BRINGERS" => ExpansionPack.Shadowbringers,
            "BRINGER" => ExpansionPack.Shadowbringers,
            "END" => ExpansionPack.Endwalker,
            "WALKER" => ExpansionPack.Endwalker,
            "DAWN" => ExpansionPack.Dawntrail,
            "TRAIL" => ExpansionPack.Dawntrail,
            "TRAILS" => ExpansionPack.Dawntrail,

            // Unable to determnine
            _ => ExpansionPack.Unknown
        };

        public static string GetExpansionPackLongString(ExpansionPack expac) => expac switch
        {
            ExpansionPack.ARealmReborn => "신생 에오르제아",
            ExpansionPack.Heavensward => "창천의 이슈가르드",
            ExpansionPack.Stormblood => "홍련의 해방자",
            ExpansionPack.Shadowbringers => "칠흑의 반역자",
            ExpansionPack.Endwalker => "효월의 종언",
            ExpansionPack.Dawntrail => "황금의 유산",
            ExpansionPack.Unknown => "알 수 없음",
            _ => "INVALID"
        };
        public static string GetExpansionPackLongString(string expac) => GetExpansionPackLongString(GetExpansionPack(expac));

        public static string GetExpansionPackShortString(ExpansionPack expac) => expac switch
        {
            ExpansionPack.ARealmReborn => "ARR",
            ExpansionPack.Heavensward => "HW",
            ExpansionPack.Stormblood => "SB",
            ExpansionPack.Shadowbringers => "ShB",
            ExpansionPack.Endwalker => "EW",
            ExpansionPack.Dawntrail => "DT",
            ExpansionPack.Unknown => "Unknown",
            _ => "INVALID"
        };
        public static string GetExpansionPackShortString(string expac) => GetExpansionPackShortString(GetExpansionPack(expac));
    }
}
