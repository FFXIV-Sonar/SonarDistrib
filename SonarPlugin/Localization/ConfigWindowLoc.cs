using AG.EnumLocalization.Attributes;
using System.Text;

namespace SonarPlugin.Localization
{
    [EnumLocStrings("ConfigWindow")]
    public enum ConfigWindowLoc
    {
        [EnumLoc(Fallback = "Sonar Configuration")]
        WindowTitle,

        [EnumLoc("Tab.General", "General")]
        GeneralTab,

        [EnumLoc("Tab.Hunts", "Hunts")]
        HuntsTab,

        [EnumLoc("Tab.Fates", "FATEs")]
        FatesTab,

        [EnumLoc("Tab.About", "About")]
        AboutTab,

        [EnumLoc("Tab.Debug", "Debug")]
        DebugTab,

        #region General Tab

        [EnumLoc(Fallback = "Overlay Visible by default")]
        OverlaysVisibleByDefault,

        [EnumLoc(Fallback = "Lock overlays")]
        LockOverlays,

        [EnumLoc(Fallback = "Hide Title Bar")]
        HideTitleBar,

        [EnumLoc("Clickthrough.Window", Fallback = "Enable window clickthrough")]
        EnableClickthrough,

        [EnumLoc("Clickthrough.Tabs", Fallback = "Enable tab bar clickthrough")]
        TabsClickthrough,

        [EnumLoc("Clickthrough.List", Fallback = "Enable list clickthrough")]
        ListClickthrough,

        [EnumLoc(Fallback = "Sorting Mode")]
        SortingMode,

        Opacity,

        [EnumLoc(Fallback = "Alert Volume")]
        AlertVolume,

        [EnumLoc("Header.Duty", Fallback = "Duty Settings")]
        DutySettingsHeader,

        [EnumLoc("Header.Click", Fallback = "Click Settings")]
        ClickSettingsHeader,

        [EnumLoc("Header.Localization", Fallback = "Localization Settings")]
        LocalizationHeader,

        [EnumLoc("Header.Lodestone", Fallback = "Lodestone Verification Settings")]
        LodestoneHeader,

        [EnumLoc("Header.Color", Fallback = "Sonar Color Scheme")]
        ColorSettingsHeader,

        // Duty Settings

        [EnumLoc("Duty.DisableChatAlerts", Fallback = "Disable Chat Alerts during Duties")]
        DutyDisableChatAlerts,

        [EnumLoc("Duty.DisableSoundAlerts", Fallback = "Disable Sound Alerts during Duties")]
        DutyDisableSoundAlerts,

        // Click Settings

        [EnumLoc("Click.Middle", Fallback = "Middle Click")]
        MiddleClick,

        [EnumLoc("Click.Right", Fallback = "Right Click")]
        RightClick,

        [EnumLoc("Click.ShiftMiddle", Fallback = "Shift Middle Click")]
        ShiftMiddleClick,

        [EnumLoc("Click.ShiftRight", Fallback = "Shift Right Click")]
        ShiftRightClick,

        [EnumLoc("Click.AltMiddle", Fallback = "Alt Middle Click")]
        AltMiddleClick,

        [EnumLoc("Click.AltRight", Fallback = "Alt Right Click")]
        AltRightClick,

        [EnumLoc("Click.CtrlMiddle", Fallback = "Ctrl Middle Click")]
        CtrlMiddleClick,

        [EnumLoc("Click.CtrlRight", Fallback = "Ctrl Right Click")]
        CtrlRightClick,

        [EnumLoc(Fallback = "Preferred City State")]
        PreferredCityState,

        // Lodestone Settings
        [EnumLoc("Lodestone.SuppressVerification", Fallback = "Suppress Verification Requests")]
        SuppressLodestoneVerifications,

        // Color Settings

        [EnumLoc("Color.Hunt.Healthy", Fallback = "Hunt - Healthy")]
        HealthyHuntColor,

        [EnumLoc("Color.Hunt.Pulled", Fallback = "Hunt - Pulled")]
        PulledHuntColor,

        [EnumLoc("Color.Hunt.Dead", Fallback = "Hunt - Dead")]
        DeadHuntColor,

        [EnumLoc("Color.Fate.Running", Fallback = "Fate - Running")]
        RunningFateColor,

        [EnumLoc("Color.Fate.Progress", Fallback = "Fate - Progress")]
        ProgressFateColor,

        [EnumLoc("Color.Fate.Complete", Fallback = "Fate - Complete")]
        CompleteFateColor,

        [EnumLoc("Color.Fate.Failed", Fallback = "Fate - Failed")]
        FailedFateColor,

        [EnumLoc("Color.Fate.Prepararing", Fallback = "Fate - Preparing")]
        PreparingFateColor,

        [EnumLoc("Color.Fate.Unknown", Fallback = "Fate - Unknown")]
        UnknownFateColor,

        [EnumLoc("Color.s_presets", Fallback = "s_presets")]
        ColorPresets,

        [EnumLoc("Color.Preset.Default", Fallback = "Default")]
        DefaultColorPreset,

        [EnumLoc("Color.Preset.Original", Fallback = "Original")]
        OriginalColorPreset,

        #endregion

        #region Hunts Tab

        [EnumLoc(Fallback = "Chat Reports Configuration")]
        ChatReportsConfig,

        [EnumLoc(Fallback = "Show reports in game chat")]
        ChatReportsEnabled,

        [EnumLoc(Fallback = "Use italic font")]
        ChatEnableItalics,

        [EnumLoc(Fallback = "Use crossworld icon when report is in another world")]
        ChatEnableCwIcon,

        [EnumLoc(Fallback = "Show death reports")]
        ChatEnableDeaths,

        [EnumLoc(Fallback = "")]
        f,

        [EnumLoc(Fallback = "")]
        g,

        [EnumLoc(Fallback = "")]
        h,

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
        #endregion

        #region About Tab
        #endregion

        #region Debug Tab
        #endregion
    }
}
