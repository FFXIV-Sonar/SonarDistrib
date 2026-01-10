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
                var magickImage = new MagickImage(texFile.ImageData, new PixelReadSettings(texFile.Header.Width, texFile.Header.Height, StorageType.Quantum, PixelMapping.BGRA));
                magickImage.Alpha(AlphaOption.Set);
                return magickImage;
            }

            public IMagickImage<byte> ToMagickImageWithMask(TexFile? texMask)
            {
                var magickImage = texFile.ToMagickImage();
                if (texMask is null) return magickImage;
                using (magickImage)
                {
                    using var magickMask = texMask.ToMagickImage();
                    return new MagickImageCollection([magickImage, magickMask]).Evaluate(EvaluateOperator.Multiply);
                }
            }
        }
    }
}
