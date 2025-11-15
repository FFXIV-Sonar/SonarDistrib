using AG.EnumLocalization.Attributes;
using System.ComponentModel;

namespace SonarPlugin.Sounds
{
    [EnumLocStrings("Sounds")]
    public enum SoundsLoc : uint
    {
        [EnumLoc("Config.PlaySounds", Fallback = "Play Sound")]
        PlaySound,

        [EnumLoc("Config.Sounds", Fallback = "Sounds")]
        Sounds,

        [EnumLoc("Config.GameSound", Fallback = "Game sound effect")]
        GameSound,

        [EnumLoc("Config.Resource", Fallback = "Legacy sound")]
        Resource,

        [EnumLoc("Config.Custom", Fallback = "Custom")]
        Custom,

        [EnumLoc("Config.Unknown", Fallback = "Unknown")]
        Unknown,

        [EnumLoc("Config.GameSoundTooltip", Fallback = "Uses FFXIV's audio engine to play the game's built in sound effects.\nUses FFXIV's system sounds volume.")]
        GameSoundTooltip,

        [EnumLoc("Config.ResourceTooltip", Fallback = "Uses Sonar's audio engine to play previously included sound effects.\nUses Sonar's alert volume.")]
        ResourceTooltip,

        [EnumLoc("Config.CustomTooltip", Fallback = "Uses Sonar's audio engine to play a custom sound effect.\nUses Sonar's alert volume.")]
        CustomTooltip,

        [EnumLoc("Config.UnknownTooltip", Fallback = "Contact Sonar Support should this appear")]
        UnknownTooltip,

        [EnumLoc("Config.SelectFile", Fallback = "Select file...")]
        SelectFile,

        [EnumLoc("Config.SelectFilePrompt", Fallback = "Custom sound file")]
        SelectFilePrompt,

        [EnumLoc("Config.SelectFilePromptFilter.SoundFiles", Fallback = "Sound files")]
        SelectFilePromptSoundFiles,

        #region Sound Effect Fallbacks
        // NOTE: SoundXY implicictly exists
        // This is used to define fallbacks
        // More information at SoundEngine.GetSoundName

        [EnumLoc(Fallback = "Sound <id>")] // Generic fallback for non-existent SoundXY entries
        SoundGeneric,

        [EnumLoc(Fallback = "Chat Sounds <se.1>")]
        Sound37,

        [EnumLoc(Fallback = "Chat Sounds <se.2>")]
        Sound38,

        [EnumLoc(Fallback = "Chat Sounds <se.3>")]
        Sound39,

        [EnumLoc(Fallback = "Chat Sounds <se.4>")]
        Sound40,

        [EnumLoc(Fallback = "Chat Sounds <se.5>")]
        Sound41,

        [EnumLoc(Fallback = "Chat Sounds <se.6>")]
        Sound42,

        [EnumLoc(Fallback = "Chat Sounds <se.7>")]
        Sound43,

        [EnumLoc(Fallback = "Chat Sounds <se.8>")]
        Sound44,

        [EnumLoc(Fallback = "Chat Sounds <se.9>")]
        Sound45,

        [EnumLoc(Fallback = "Chat Sounds <se.10>")]
        Sound46,

        [EnumLoc(Fallback = "Chat Sounds <se.11>")]
        Sound47,

        [EnumLoc(Fallback = "Chat Sounds <se.12>")]
        Sound48,

        [EnumLoc(Fallback = "Chat Sounds <se.13>")]
        Sound49,

        [EnumLoc(Fallback = "Chat Sounds <se.14>")]
        Sound50,

        [EnumLoc(Fallback = "Chat Sounds <se.15>")]
        Sound51,

        [EnumLoc(Fallback = "Chat Sounds <se.16>")]
        Sound52,
        #endregion

        #region Resource Sound Fallbacks
        [EnumLoc("Resources.EnterChat", Fallback = "Enter Chat")]
        ResEnterChat,

        [EnumLoc("Resources.Fanfare", Fallback = "Fanfare")]
        ResFanfare,

        [EnumLoc("Resources.FeatureUnlocked", Fallback = "Feature Unlocked")]
        ResFeatureUnlocked,

        [EnumLoc("Resources.IncomingTell1", Fallback = "Incoming Tell 1")]
        ResIncomingTell1,

        [EnumLoc("Resources.IncomingTell2", Fallback = "Incoming Tell 2")]
        ResIncomingTell2,

        [EnumLoc("Resources.LimitBreakCharged", Fallback = "Limit Break Charged")]
        ResLimitBreakCharged,

        [EnumLoc("Resources.LimitBreakUnlocked", Fallback = "Limit Break Unlocked")]
        ResLimitBreakUnlocked,

        [EnumLoc("Resources.LinkshellTransmission", Fallback = "Linkshell Transmission")]
        ResLinkshellTransmission,

        [EnumLoc("Resources.Notification", Fallback = "Notification")]
        ResNotification,
        #endregion
    }
}
