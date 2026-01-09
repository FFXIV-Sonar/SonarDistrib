using ImageMagick;
using Lumina;
using Lumina.Excel.Sheets;

namespace SonarResources.Maps
{
    public static class MapMagickExtensions
    {
        extension (ref readonly Map map)
        {
            public IMagickImage<byte>? GetMagickImage(GameData data) => map.GetImageTex(data)?.ToMagickImage();

            public IMagickImage<byte>? GetMagickMask(GameData data) => map.GetMaskTex(data)?.ToMagickImage();

            public IMagickImage<byte>? GetMagickMap(GameData data)
            {
                var magickImage = map.GetMagickImage(data);
                if (magickImage is null) return null;

                var magickMask = map.GetMagickMask(data);
                if (magickMask is null) return magickImage;

                try
                {
                    return new MagickImageCollection() { magickImage, magickMask }.Evaluate(EvaluateOperator.Multiply);
                }
                finally
                {
                    magickMask.Dispose();
                    magickImage.Dispose();
                }
            }
        }

    }
}
