using System.Diagnostics.CodeAnalysis;

namespace Sonar.Data.Web
{
    [SuppressMessage("Minor Code Smell", "S1075")]
    public static class WebConstants
    {
        /// <summary>Base Assets URL.</summary>
        public const string AssetsUrlBase = "https://assets.ffxivsonar.com/";

        /// <summary>Base API URL.</summary>
        public const string ApiUrlBase = "https://api.ffxivsonar.com/";

        /// <summary>Images path.</summary>
        public const string ImagesUrlBase = $"{AssetsUrlBase}images/";

        /// <summary>Data path.</summary>
        public const string DataUrlBase = $"{AssetsUrlBase}data/";
    }
}
