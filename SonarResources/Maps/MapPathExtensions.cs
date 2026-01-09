using Lumina.Excel.Sheets;

namespace SonarResources.Maps
{
    public static class MapPathExtensions
    {
        private const string PathFormat = "ui/map/{0}/{1}{2}_m.tex";

        extension(ref readonly Map map)
        {
            public string? GetImagePath() => GetPath(map.Id.ExtractText(), false);
            public string? GetMaskPath() => GetPath(map.Id.ExtractText(), true);
        }

        private static string? GetPath(string? mapId, bool mask)
        {
            if (mapId is null) return null;
            var fileName = mapId.Replace("/", "");
            return string.Format(PathFormat, mapId, fileName, mask ? "m" : null);
        }
    }
}
