using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonar.Data.Rows;

namespace Sonar.Data.Web
{
    public enum MapImageSize
    {
        /// <summary>
        /// 256x256
        /// </summary>
        Tiny,

        /// <summary>
        /// 512x512
        /// </summary>
        Small,

        /// <summary>
        /// 1024x1024
        /// </summary>
        Medium,

        /// <summary>
        /// 2048x2048
        /// </summary>
        Large,
    }

    public enum ImageFormat
    {
        Jpg,
        Png,
    }

    public enum JsonDataFile
    {
        Datacenter,
        Fate,
        Hunt,
        World,
        Zone,
        Map,
    }

    [Flags]
    public enum UrlFlags
    {
        /// <summary>
        /// https://assets.ffxivsonar.com
        /// </summary>
        IncludeBase = 1,

        /// <summary>
        /// Directory path
        /// </summary>
        IncludePath = 2,

        /// <summary>
        /// IncludeBase + IncludePath
        /// </summary>
        Default = 3,
    }

    /// <summary>
    /// Web related extensions (in development)
    /// </summary>
    public static class WebExtensions
    {
        #region Constants
        /// <summary>
        /// Base Assets URL
        /// </summary>
        public const string UrlBase = "https://assets.ffxivsonar.com";

        /// <summary>
        /// Maps Path
        /// </summary>
        public const string MapPath = "images";

        /// <summary>
        /// Data Path
        /// </summary>
        public const string DataPath = "data";

        /// <summary>
        /// Map filename prefix
        /// </summary>
        public const string MapFilePrefix = "map";

        /// <summary>
        /// Map filename prefix
        /// </summary>
        public const string ZonneFilePrefix = "zone";
        #endregion

        #region Private Members
        private static readonly Dictionary<MapImageSize, string> MapImageSizeSuffix = new Dictionary<MapImageSize, string>()
        {
            { MapImageSize.Tiny , "t" }, // 256
            { MapImageSize.Small , "s" }, // 512
            { MapImageSize.Medium , "m" }, // 1024
            { MapImageSize.Large , "l" }, // 2048
        };

        private static readonly Dictionary<ImageFormat, string> ImageFormatExt = new Dictionary<ImageFormat, string>()
        {
            { ImageFormat.Jpg , "jpg" },
            { ImageFormat.Png , "png" },
        };

        private static readonly Dictionary<JsonDataFile, string> JsonFile = new Dictionary<JsonDataFile, string>()
        {
            { JsonDataFile.Datacenter, "datacenter.json" },
            { JsonDataFile.Fate, "fate.json" },
            { JsonDataFile.Hunt, "hunt.json" },
            { JsonDataFile.World, "world.json" },
            { JsonDataFile.Zone, "zone.json" },
            { JsonDataFile.Map, "map.json" },
        };

        private static string GenerateUrl(string path, string file, UrlFlags flags)
        {
            string url = string.Empty;
            if ((flags & UrlFlags.IncludeBase) != 0) url += $"{UrlBase}/";
            if ((flags & UrlFlags.IncludePath) != 0 && (flags & UrlFlags.IncludeBase) != 0) url += $"{path}/";
            return $"{url}{file}";
        }
        #endregion

        private static string ImageFile(string name, ImageFormat format) => $"{name}.{ImageFormatExt[format]}";

        /// <summary>
        /// Get Zone Image URL
        /// </summary>
        /// <param name="zone">Zone</param>
        /// <param name="size">Size</param>
        /// <param name="format">Format</param>
        /// <param name="flags">Flags</param>
        /// <returns>Zone Image URL</returns>
        public static string GetImageURL(this ZoneRow zone, MapImageSize size = MapImageSize.Medium, ImageFormat format = ImageFormat.Jpg, UrlFlags flags = UrlFlags.Default)
        {
            return GenerateUrl(MapPath, ImageFile($"{MapFilePrefix}-{zone.MapId}-{MapImageSizeSuffix[size]}", format), flags);
        }

        /// <summary>
        /// Get Map Image URL
        /// </summary>
        /// <param name="map">Map</param>
        /// <param name="size">Size</param>
        /// <param name="format">Format</param>
        /// <param name="flags">Flags</param>
        /// <returns>Map Image URL</returns>
        public static string GetImageURL(this MapRow map, MapImageSize size = MapImageSize.Medium, ImageFormat format = ImageFormat.Jpg, UrlFlags flags = UrlFlags.Default)
        {
            return GenerateUrl(MapPath, ImageFile($"{MapFilePrefix}-{map.Id}-{MapImageSizeSuffix[size]}", format), flags);
        }

        /// <summary>
        /// Get JSON Data URL
        /// </summary>
        /// <param name="json">JSON data</param>
        /// <param name="flags">Flags</param>
        /// <returns>JSON Data URL</returns>
        public static string GetJsonDataUrl(JsonDataFile json, UrlFlags flags = UrlFlags.Default)
        {
            return GenerateUrl(DataPath, JsonFile[json], flags);
        }
    }
}
