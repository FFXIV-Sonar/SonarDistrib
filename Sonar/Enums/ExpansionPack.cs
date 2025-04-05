using AG.EnumLocalization.Attributes;

namespace Sonar.Enums
{
    [EnumLocStrings]
    public enum ExpansionPack : byte
    {
        Unknown,

        [EnumLoc(Fallback = "A Realm Reborn")]
        ARealmReborn,
        
        Heavensward,
        
        Stormblood,
        
        Shadowbringers,
        
        Endwalker,
        
        Dawntrail,
    }
}
