using Lumina;
using Lumina.Data;
using Sonar.Enums;

namespace SonarResources.Lumina
{
    public sealed record LuminaEntry(GameData Data, Language LuminaLanguage, SonarLanguage SonarLanguage);
}
