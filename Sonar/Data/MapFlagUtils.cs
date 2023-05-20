using System.Globalization;
using Sonar.Numerics;

namespace Sonar.Data
{
    /// <summary>
    /// Utility functions for converting raw game coordinates into flags and map pixel positions
    /// </summary>
    public static class MapFlagUtils
    {
        #region Raw to Flag Coordinates
        public static float RawToFlagCoord(float scale, float offset, float raw)
        {
            float scaled = (raw + offset) * scale;
            return ((41 / scale) * ((scaled + 1024) / 2048)) + 1;
        }
        public static float ZRawToFlagCoord(float scale, float offset, float raw)
        {
            return (raw - offset) / 100;
        }

        /// <summary>
        /// Converts raw coordinates to a flag
        /// </summary>
        public static SonarVector3 RawToFlag(float scale, SonarVector3 offset, SonarVector3 rawCoords) => new SonarVector3()
        {
            X = RawToFlagCoord(scale, offset.X, rawCoords.X),
            Y = RawToFlagCoord(scale, offset.Y, rawCoords.Y),
            Z = ZRawToFlagCoord(100, offset.Z, rawCoords.Z)
        };
        #endregion

        #region Flag To Raw Coordinates
        public static float FlagToRawCoord(float scale, float offset, float flag)
        {
            return (((((flag - 1) * scale / 41) * 2048) - 1024) / scale) - offset;
        }
        public static float ZFlagToRawCoord(float offset, float flag)
        {
            return (flag * 100) + offset;
        }

        /// <summary>
        /// Converts a flag to raw coordinates
        /// </summary>
        public static SonarVector3 FlagToRaw(float scale, SonarVector3 offset, SonarVector3 flagCoords) => new SonarVector3()
        {
            X = FlagToRawCoord(scale, offset.X, flagCoords.X),
            Y = FlagToRawCoord(scale, offset.Y, flagCoords.Y),
            Z = ZFlagToRawCoord(offset.Z, flagCoords.Z)
        };
        #endregion

        #region Flag to Pixel Coordinates
        public static float FlagToPixelCoord(float scale, float flag, int resolution = 2048)
        {
            return (flag - 1) * 50 * scale * resolution / 2048;
        }

        public static float PixelToFlagCoord(float scale, float pixel, int resolution = 2048)
        {
             return 1 + pixel / 50 / scale / resolution * 2048;
        }

        public static SonarVector3 FlagToPixel(float scale, SonarVector3 flag, int resolution = 2048)
        {
            return new SonarVector3(FlagToPixelCoord(scale, flag.X, resolution), FlagToPixelCoord(scale, flag.Y, resolution), flag.Z);
        }

        public static SonarVector3 PixelToFlag(float scale, SonarVector3 pixel, int resolution = 2048)
        {
            return new SonarVector3(PixelToFlagCoord(scale, pixel.X, resolution), PixelToFlagCoord(scale, pixel.Y, resolution), pixel.Z);
        }
        #endregion

        #region Pixel <==> Raw
        public static SonarVector3 RawToPixel(float scale, SonarVector3 offset, SonarVector3 raw, int resolution = 2048)
        {
            return FlagToPixel(scale, RawToFlag(scale, offset, raw), resolution);
        }

        public static SonarVector3 PixelToRaw(float scale, SonarVector3 offset, SonarVector3 pixel, int resolution = 2048)
        {
            return FlagToRaw(scale, offset, PixelToFlag(scale, pixel, resolution));
        }
        #endregion

        #region Flag coordinate strings
        /// <summary>
        /// Converts a flag coordinates into a string
        /// </summary>
        /// <param name="flag">Flag vector to convert</param>
        /// <param name="format">Format to use</param>
        /// <returns>Flag coordinates as a string</returns>
        public static string FlagToString(SonarVector3 flag, MapFlagFormatFlags format = MapFlagFormatFlags.SonarPreset)
        {
            var space = format.HasFlag(MapFlagFormatFlags.ExtraSpaces) ? " " : "";
            var fmt = format.HasFlag(MapFlagFormatFlags.ExtendedAccuracy) ? "F2" : "F1";

            var coordX = string.Format(CultureInfo.InvariantCulture, $"{{0:{fmt}}}", flag.X);
            var coordY = string.Format(CultureInfo.InvariantCulture, $"{{0:{fmt}}}", flag.Y);
            var coords = $"{coordX}{space}{space}, {coordY}"; // Yes that space is intentional

            if (format.HasFlag(MapFlagFormatFlags.Parenthesis))
            {
                coords = $"({space}{coords}{space})";
            }

            if (format.HasFlag(MapFlagFormatFlags.IncludeZ))
            {
                var coordZ = string.Format(CultureInfo.InvariantCulture, $"{{0:{fmt}}}", flag.Z);
                coords = $"{coords} Z: {coordZ}";
            }

            return coords;
        }
        #endregion
    }
}
