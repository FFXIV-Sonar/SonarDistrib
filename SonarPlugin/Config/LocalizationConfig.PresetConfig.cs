using Sonar.Enums;

namespace SonarPlugin.Config
{
    public sealed partial class LocalizationConfig
    {
        public record PresetConfig(SonarLanguage Db, string? Plugin, string? Dll);
    }
}
