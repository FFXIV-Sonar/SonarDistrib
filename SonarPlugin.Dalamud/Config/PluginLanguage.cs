namespace SonarPlugin.Config
{
    public enum PluginLanguage
    {
        English
        //,French
        //,German
        //,Japanese
        //,Korean
    }

    public static class LanguageExtensions
    {
        public static string GetLanguageCode(this PluginLanguage language)
        {
            switch (language)
            {
                case PluginLanguage.English:
                    return "en";
                default:
                    return "en";
            }
        }
    }
}