using Sonar.Data.Rows;

namespace Sonar.Data.Web
{
    /// <summary>Web related extension methods.</summary>
    public static class WebExtensions
    {
        /// <summary>Gets the map's image URL for a specified <paramref name="map"/>, <paramref name="size"/> and <paramref name="format"/>.</summary>
        /// <param name="map">Map.</param>
        /// <param name="size"><see cref="MapImageSize"/>.</param>
        /// <param name="format"><see cref="ImageFormat"/></param>
        /// <returns>Map's image URL.</returns>
        public static string GetMapImageUrl(this MapRow map, MapImageSize size = MapImageSize.Large, ImageFormat format = ImageFormat.Png)
            => WebUtils.GetMapImageUrl(map.Id, size, format);

        /// <summary>Gets the zone map's image URL for a specified <paramref name="zone"/>, <paramref name="size"/> and <paramref name="format"/>.</summary>
        /// <param name="zone">Map.</param>
        /// <param name="size"><see cref="MapImageSize"/>.</param>
        /// <param name="format"><see cref="ImageFormat"/></param>
        /// <returns>Zone's map image URL.</returns>
        public static string GetZoneMapImageUrl(this ZoneRow zone, MapImageSize size = MapImageSize.Large, ImageFormat format = ImageFormat.Png)
            => WebUtils.GetZoneMapImageUrl(zone.Id, size, format);


        /// <summary>Gets the pixels length of a side.</summary>
        /// <param name="size">Image size.</param>
        /// <returns>Side pixels length.</returns>
        /// <exception cref="ArgumentException">Invalid map image size</exception>
        public static uint ToLength(this MapImageSize size) => WebUtils.GetMapImageSidePixelsLength(size);
    }
}
