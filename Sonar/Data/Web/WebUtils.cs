using System;
using Sonar.Numerics;
using SonarUtils;

namespace Sonar.Data.Web
{
    /// <summary>Web related utility methods.</summary>
    public static class WebUtils
    {
        /// <summary>Gets the pixels length of a side.</summary>
        /// <param name="size">Image size.</param>
        /// <returns>Side pixels length.</returns>
        /// <exception cref="ArgumentException">Invalid map image size</exception>
        public static int GetMapImageSidePixelsLength(MapImageSize size)
        {
            return size switch
            {
                MapImageSize.Large => 2048,
                MapImageSize.Medium => 1024,
                MapImageSize.Small => 512,
                MapImageSize.Tiny => 256,
                _ => throw new ArgumentException("Invalid map image size", nameof(size))
            };
        }

        /// <summary>Gets the best <see cref="MapImageSize"/> for <paramref name="sidePixelsLength"/>.</summary>
        /// <param name="sidePixelsLength">Pixel length of one of the sides.</param>
        /// <returns>Best <see cref="MapImageSize"/> for the specified <paramref name="sidePixelsLength"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sidePixelsLength"/> must be greater than zero.</exception>
        public static MapImageSize GetBestMapImageSize(int sidePixelsLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sidePixelsLength);
            return sidePixelsLength switch
            {
                > 1024 => MapImageSize.Large,
                > 512 and <= 1024 => MapImageSize.Medium,
                > 256 and <= 512 => MapImageSize.Small,
                > 0 and <= 256 => MapImageSize.Tiny,
                _ => (MapImageSize)(-1) // Assert: Should never happen
            };
        }

        /// <summary>Gets the map's image URL for a specified <paramref name="mapId"/>, <paramref name="size"/> and <paramref name="format"/>.</summary>
        /// <param name="mapId">Map ID.</param>
        /// <param name="size"><see cref="MapImageSize"/>.</param>
        /// <param name="format"><see cref="ImageFormat"/></param>
        /// <returns>Map's image URL.</returns>
        public static string GetMapImageUrl(uint mapId, MapImageSize size = MapImageSize.Large, ImageFormat format = ImageFormat.Png) => $"{WebConstants.ImagesUrlBase}map-{StringUtils.GetNumber(mapId)}-{WebUtilsInternal.GetMapImageSizeSuffix(size)}.{WebUtilsInternal.GetImageFormatExtension(format)}";

        /// <summary>Gets the zone's map image URL for a specified <paramref name="zoneId"/>, <paramref name="size"/> and <paramref name="format"/>.</summary>
        /// <param name="zoneId">Zone ID.</param>
        /// <param name="size"><see cref="MapImageSize"/>.</param>
        /// <param name="format"><see cref="ImageFormat"/></param>
        /// <returns>Zone's map image URL.</returns>
        public static string GetZoneMapImageUrl(uint zoneId, MapImageSize size = MapImageSize.Large, ImageFormat format = ImageFormat.Png) => $"{WebConstants.ImagesUrlBase}zone-{StringUtils.GetNumber(zoneId)}-{WebUtilsInternal.GetMapImageSizeSuffix(size)}.{WebUtilsInternal.GetImageFormatExtension(format)}";

        /// <summary>Gets the URL of a data json file.</summary>
        /// <param name="type"><see cref="JsonDataFile"/>.</param>
        /// <returns>Data json URL.</returns>
        public static string GetDataJsonUrl(JsonDataFile type) => $"{WebConstants.DataUrlBase}{type.ToString().ToLowerInvariant()}.json";

        /// <summary>Gets an URL for a map of <paramref name="length"/> size marked with a spot at <paramref name="coords"/> and optionally drawing a <paramref name="fate"/> circle.</summary>
        /// <param name="mapId">Map ID.</param>
        /// <param name="coords">Raw coordinates.</param>
        /// <param name="length">Side length.</param>
        /// <param name="fate">Draw fate circle.</param>
        /// <returns>URL request with the above properties.</returns>
        public static string GetMapWithSpotUrl(uint mapId, SonarVector3 coords, int length = 512, bool fate = false, ImageFormat format = ImageFormat.Png) => $"{WebConstants.ApiUrlBase}render/map?mapId={mapId}&x={coords.X}&y={coords.Y}&size={length}&fate={fate}";

        /// <summary>Gets an URL for a map of <paramref name="length"/> size marked with a spot at <paramref name="flagCoords"/> and optionally drawing a <paramref name="fate"/> circle.</summary>
        /// <param name="mapId">Map ID.</param>
        /// <param name="flagCoords">Flag coordinates.</param>
        /// <param name="length">Side length.</param>
        /// <param name="fate">Draw fate circle.</param>
        /// <returns>URL request with the above properties.</returns>
        public static string GetMapWithSpotUrlUsingFlag(uint mapId, SonarVector3 flagCoords, int length = 512, bool fate = false, ImageFormat format = ImageFormat.Png) => $"{WebConstants.ApiUrlBase}render/map?mapId={mapId}&flagX={flagCoords.X}&flagY={flagCoords.Y}&size={length}&fate={fate}";

        /// <summary>Gets an URL for a map of <paramref name="length"/> size marked with a spot at <paramref name="coords"/> and optionally drawing a <paramref name="fate"/> circle.</summary>
        /// <param name="mapId">Map ID.</param>
        /// <param name="coords">Raw coordinates.</param>
        /// <param name="length">Side length.</param>
        /// <param name="fate">Draw fate circle.</param>
        /// <returns>URL request with the above properties.</returns>
        public static string GetZoneWithSpotUrl(uint mapId, SonarVector3 coords, int length = 512, bool fate = false, ImageFormat format = ImageFormat.Jpg) => $"{WebConstants.ApiUrlBase}render/map?mapId={mapId}&x={coords.X}&y={coords.Y}&size={length}&fate={fate}&format={format}";

        /// <summary>Gets an URL for a map of <paramref name="length"/> size marked with a spot at <paramref name="flagCoords"/> and optionally drawing a <paramref name="fate"/> circle.</summary>
        /// <param name="mapId">Map ID.</param>
        /// <param name="flagCoords">Flag coordinates.</param>
        /// <param name="length">Side length.</param>
        /// <param name="fate">Draw fate circle.</param>
        /// <returns>URL request with the above properties.</returns>
        public static string GetZoneWithSpotUrlUsingFlag(uint mapId, SonarVector3 flagCoords, int length = 512, bool fate = false, ImageFormat format = ImageFormat.Jpg) => $"{WebConstants.ApiUrlBase}render/map?mapId={mapId}&flagX={flagCoords.X}&flagY={flagCoords.Y}&size={length}&fate={fate}&format={format}";
    }
}
