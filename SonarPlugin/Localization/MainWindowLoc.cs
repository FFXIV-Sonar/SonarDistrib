using AG.EnumLocalization.Attributes;
namespace SonarPlugin.Localization
{
    [EnumLocStrings("MainWindow")]
    public enum MainWindowLoc
    {
        [EnumLoc(Fallback = "Sonar")]
        WindowTitle,

        [EnumLoc("Tabs.All", "전체")]
        AllTab,

        [EnumLoc("Detail.Location", "지역")]
        LocationDetail,

        [EnumLoc("Detail.FlagButtonTooltip", "대화 창에 좌표 표시")]
        DetailFlagToolTip,

        [EnumLoc("Detail.MapButtonTooltip", "지도에 표시")]
        DetailMapToolTip,

        [EnumLoc("Detail.TeleportButtonTooltip", "가까운 에테라이트로 텔레포")]
        DetailTeleportToolTip,

        [EnumLoc("Detail.RemoveButtonTooltip", "다음 갱신까지 이 마물을 목록에서 제거")]
        DetailRemoveToolTip,
    }
}
