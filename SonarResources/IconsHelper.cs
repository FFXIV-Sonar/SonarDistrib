using Lumina.Data.Files;
using System;

namespace SonarResources
{
    // TODO: Make use of this
    public static class IconsHelper
    {
        public static string GetIconGameDataPath(uint iconId, bool hd) => $"ui/icon/{iconId / 1000 * 1000:D6}/{iconId:D6}{(hd ? "_hr1" : string.Empty)}.tex";
        public static TexFile? GetIconTexture(uint iconId, bool hd)
        {
            //return SonarResourceGenerator.Lumina.GetFile<TexFile>(GetIconGameDataPath(iconId, hd));
            throw new NotImplementedException();
        }
        //public static Image<Bgra32>? GetIconImage(uint iconId, bool hd) => GetIconTexture(iconId, hd)?.ToImage();
    }
}
