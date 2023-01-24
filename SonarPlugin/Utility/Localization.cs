using CheapLoc;
using SonarPlugin.Config;

namespace SonarPlugin.Utility
{
    [SingletonService]
    public class Localization
    {
        public void SetupLocalization(PluginLanguage language)
        {
            if (language == PluginLanguage.English)
            {
                Loc.SetupWithFallbacks();
            }
            //else
            //{
                // TODO: Add other .json files to /Resources and embed them as people translate the localization.
                //switch (language.GetLanguageCode())
                //{
                //    case "en":
                //    default:
                //        Loc.SetupWithFallbacks();
                //        break;
                //}
            //}
        }
    }
}
