using AG.EnumLocalization.Attributes;
namespace SonarPlugin.Localization
{
    [EnumLocStrings("MainWindow")]
    public enum MainWindowLoc
    {
        [EnumLoc(Fallback = "Sonar")]
        WindowTitle,

        [EnumLoc("Tabs.All", "All")]
        AllTab,

        [EnumLoc("Detail.Location", "Location")]
        LocationDetail,

        [EnumLoc("Detail.FlagButtonTooltip", "Show coordinates in chat window")]
        DetailFlagToolTip,

        [EnumLoc("Detail.MapButtonTooltip", "Set flag and open map")]
        DetailMapToolTip,

        [EnumLoc("Detail.TeleportButtonTooltip", "Teleport to Closest Aetheryte")]
        DetailTeleportToolTip,

        [EnumLoc("Detail.RemoveButtonTooltip", "Remove hunt from the list until next update")]
        DetailRemoveToolTip,
    }
}
