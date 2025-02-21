using Lumina.Data.Files;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Processors.Drawing;
using SixLabors.ImageSharp.Processing;

namespace SonarResources
{
    public static class TexFileExtensions
    {
        public static Image<Bgra32> ToImage(this TexFile tex)
        {
            return Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);
        }
    }
}
