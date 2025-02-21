using System;

namespace Sonar.Data.Web
{
    internal static class WebUtilsInternal
    {
        public static string GetMapImageSizeSuffix(MapImageSize size)
        {
            return size switch
            {
                MapImageSize.Large => "l",
                MapImageSize.Medium => "m",
                MapImageSize.Small => "s",
                MapImageSize.Tiny => "t",
                _ => throw new ArgumentException($"Invalid map image size: {size}", nameof(size))
            };
        }

        public static string GetImageFormatExtension(ImageFormat format)
        {
            return format switch
            {
                ImageFormat.Jpg => "jpg",
                ImageFormat.Png => "png",
                ImageFormat.Gif => "gif",
                ImageFormat.WebP => "webp",
                _ => throw new ArgumentException($"Invalid image format: {format}", nameof(format))
            };
        }
    }
}
