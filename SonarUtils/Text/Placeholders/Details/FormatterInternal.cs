using System.Text.RegularExpressions;

namespace SonarUtils.Text.Details
{
    internal static partial class FormatterInternal
    {
        public static readonly Regex DefaultRegex = GetDefaultRegex();

        [GeneratedRegex(@"\<(?<name>\w+?)\>", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetDefaultRegex();
    }
}
