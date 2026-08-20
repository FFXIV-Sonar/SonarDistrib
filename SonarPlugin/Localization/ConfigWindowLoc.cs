using AG.EnumLocalization.Attributes;
using System.Text;

namespace SonarPlugin.Localization
{
    [EnumLocStrings("ConfigWindow")]
    public enum ConfigWindowLoc
    {
        [EnumLoc(Fallback = "Sonar 구성")]
        WindowTitle,

        [EnumLoc("Tab.General", "일반")]
        GeneralTab,

        [EnumLoc("Tab.Hunts", "마물")]
        HuntsTab,

        [EnumLoc("Tab.Fates", "돌발")]
        FatesTab,

        [EnumLoc("Tab.About", "정보")]
        AboutTab,

        [EnumLoc("Tab.Debug", "디버그")]
        DebugTab,

        #region General Tab

        [EnumLoc(Fallback = "오버레이 항상 표시")]
        OverlaysVisibleByDefault,

        [EnumLoc(Fallback = "오버레이 잠그기")]
        LockOverlays,

        [EnumLoc(Fallback = "타이틀바 숨기기")]
        HideTitleBar,

        [EnumLoc("Clickthrough.Window", Fallback = "클릭 무시 활성화")]
        EnableClickthrough,

        [EnumLoc("Clickthrough.Tabs", Fallback = "탭 바 클릭 무시 활성화")]
        TabsClickthrough,

        [EnumLoc("Clickthrough.List", Fallback = "목록 클릭 무시 활성화")]
        ListClickthrough,

        [EnumLoc(Fallback = "정렬 모드")]
        SortingMode,

        [EnumLoc(Fallback = "투명도")]
        Opacity,

        [EnumLoc(Fallback = "알림음 음량")]
        AlertVolume,

        [EnumLoc("Header.Duty", Fallback = "임무")]
        DutySettingsHeader,

        [EnumLoc("Header.Click", Fallback = "클릭")]
        ClickSettingsHeader,

        [EnumLoc("Header.Localization", Fallback = "언어")]
        LocalizationHeader,

        [EnumLoc("Header.Lodestone", Fallback = "로드스톤 인증")]
        LodestoneHeader,

        [EnumLoc("Header.Color", Fallback = "Sonar 색상 스타일")]
        ColorSettingsHeader,

        // Duty Settings

        [EnumLoc("Duty.DisableChatAlerts", Fallback = "임무 중 메시지 알림 비활성화")]
        DutyDisableChatAlerts,

        [EnumLoc("Duty.DisableSoundAlerts", Fallback = "임무 중 소리 알림 비활성화")]
        DutyDisableSoundAlerts,

        // Click Settings

        [EnumLoc("Click.Middle", Fallback = "휠클릭")]
        MiddleClick,

        [EnumLoc("Click.Right", Fallback = "우클릭")]
        RightClick,

        [EnumLoc("Click.ShiftMiddle", Fallback = "SHIFT + 휠클릭")]
        ShiftMiddleClick,

        [EnumLoc("Click.ShiftRight", Fallback = "SHIFT + 우클릭")]
        ShiftRightClick,

        [EnumLoc("Click.AltMiddle", Fallback = "ALT + 휠클릭")]
        AltMiddleClick,

        [EnumLoc("Click.AltRight", Fallback = "ALT + 우클릭")]
        AltRightClick,

        [EnumLoc("Click.CtrlMiddle", Fallback = "CTRL + 휠클릭")]
        CtrlMiddleClick,

        [EnumLoc("Click.CtrlRight", Fallback = "CTRL + 우클릭")]
        CtrlRightClick,

        [EnumLoc(Fallback = "선호하는 도시")]
        PreferredCityState,

        // Lodestone Settings
        [EnumLoc("Lodestone.SuppressVerification", Fallback = "인증 요청 알림")]
        SuppressLodestoneVerifications,

        // Color Settings

        [EnumLoc("Color.Hunt.Healthy", Fallback = "마물 - 생존")]
        HealthyHuntColor,

        [EnumLoc("Color.Hunt.Pulled", Fallback = "마물 - 토벌 중")]
        PulledHuntColor,

        [EnumLoc("Color.Hunt.Dead", Fallback = "마물 - 토벌 완료")]
        DeadHuntColor,

        [EnumLoc("Color.Fate.Running", Fallback = "돌발 - 진행 중 (0%)")]
        RunningFateColor,

        [EnumLoc("Color.Fate.Progress", Fallback = "돌발 - 진행 중")]
        ProgressFateColor,

        [EnumLoc("Color.Fate.Complete", Fallback = "돌발 - 완료")]
        CompleteFateColor,

        [EnumLoc("Color.Fate.Failed", Fallback = "돌발 - 실패")]
        FailedFateColor,

        [EnumLoc("Color.Fate.Prepararing", Fallback = "돌발 - 준비 중")]
        PreparingFateColor,

        [EnumLoc("Color.Fate.Unknown", Fallback = "돌발 - 알 수 없음")]
        UnknownFateColor,

        [EnumLoc("Color.s_presets", Fallback = "색상 스타일 설정값")]
        ColorPresets,

        [EnumLoc("Color.Preset.Default", Fallback = "기본 스타일")]
        DefaultColorPreset,

        [EnumLoc("Color.Preset.Original", Fallback = "초기 스타일")]
        OriginalColorPreset,

        #endregion

        [EnumLoc(Fallback = "마물 전파 메시지 알림")]
        ChatReportsConfig,

        [EnumLoc(Fallback = "대화 창에 표시")]
        ChatReportsEnabled,

        [EnumLoc(Fallback = "기울임체 사용")]
        ChatEnableItalics,

        [EnumLoc(Fallback = "타 서버 아이콘 표시")]
        ChatEnableCwIcon,

        [EnumLoc(Fallback = "토벌 완료 메시지 표시")]
        ChatEnableDeaths,

        [EnumLoc(Fallback = "마물 전파 소리 알림")]
        SoundAlertsConfig,

        #region Hunts Tab

        [EnumLoc("Hunts.SRankSounds", Fallback = "S급 알림음")]
        HuntSRankSounds,

        [EnumLoc("Hunts.ARankSounds", Fallback = "A급 알림음")]
        HuntARankSounds,

        [EnumLoc("Hunts.BRankSounds", Fallback = "B급 알림음")]
        HuntBRankSounds,

        [EnumLoc(Fallback = "")]
        i,

        [EnumLoc(Fallback = "")]
        j,

        [EnumLoc(Fallback = "")]
        k,

        [EnumLoc(Fallback = "")]
        l,

        [EnumLoc(Fallback = "")]
        m,

        [EnumLoc(Fallback = "")]
        n,

        [EnumLoc(Fallback = "")]
        o,

        [EnumLoc(Fallback = "")]
        p,

        [EnumLoc(Fallback = "")]
        q,

        [EnumLoc(Fallback = "")]
        r,

        [EnumLoc(Fallback = "")]
        s,

        [EnumLoc(Fallback = "")]
        t,

        [EnumLoc(Fallback = "")]
        u,

        [EnumLoc(Fallback = "")]
        v,

        [EnumLoc(Fallback = "")]
        w,

        [EnumLoc(Fallback = "")]
        x,

        [EnumLoc(Fallback = "")]
        y,

        [EnumLoc(Fallback = "")]
        z,

        #endregion

        #region Fates Tab
        [EnumLoc("Fates.FateSounds", Fallback = "돌발 알림 소리")]
        FateSounds,


        #endregion

        #region About Tab
        #endregion

        #region Debug Tab
        #endregion
    }
}
