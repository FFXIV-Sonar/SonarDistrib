using Lumina;
using Lumina.Data.Files;
using Lumina.Excel.Sheets;

namespace SonarResources.Maps
{
    public static class MapTexExtensions
    {
        extension(ref readonly Map map)
        {
            public TexFile? GetImageTex(GameData data) => GetTex(data, map.GetImagePath());
            public TexFile? GetMaskTex(GameData data) => GetTex(data, map.GetMaskPath());
        }

        private static TexFile? GetTex(GameData data, string? path) => path is not null ? data.GetFile<TexFile>(path) : null;
    }
}
