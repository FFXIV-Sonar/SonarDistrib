using System.Runtime.CompilerServices;

namespace SonarPlugin.Utility
{
    /// <summary>ImGui Utility Methods for Sonar.</summary>
    public static class SonarImGuiUtils
    {
        /// <summary>Escape all <c>%</c> symbols.</summary>
        /// <param name="str">String with <c>%</c> symbols to escape.</param>
        /// <returns>Escaped string.</returns>
        /// <remarks>Escaped <c>%</c> will be replaced by <c>%%</c>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Escape(string str) => str.Replace("%", "%%");

        /// <summary>Unescape all <c>%%</c> symbols.</summary>
        /// <param name="str">String with <c>%%</c> symbols to unescape.</param>
        /// <returns>Unescaped string.</returns>
        /// <remarks>Unescaped <c>%%</c> will be replaced by <c>%</c>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Unescape(string str) => str.Replace("%%", "%");

        /// <summary>Check if <paramref name="str"/> is escaped.</summary>
        /// <param name="str">String to check.</param>
        /// <returns>A value indicating whether <paramref name="str"/> is escaping or no escaping needed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEscaped(string str) => str.Contains("%%") || !str.Contains('%');
    }
}
