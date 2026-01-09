using ImageMagick;
using Lumina.Data.Files;

namespace SonarResources.Maps
{
    public static class TexMagickExtensions
    {
        extension (TexFile texFile)
        {
            public IMagickImage<byte> ToMagickImage()
            {
                var magickImage = new MagickImage();
                magickImage.ReadPixels(texFile.ImageData, new PixelReadSettings(texFile.Header.Width, texFile.Header.Height, StorageType.Quantum, PixelMapping.BGRA));
                return magickImage;
            }
        }
    }
}
