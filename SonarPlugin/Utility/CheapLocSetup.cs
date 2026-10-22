using CheapLoc;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SonarPlugin.Utility
{
    /// <summary>
    /// Applies the embedded CheapLoc overrides matching the currently selected plugin language.
    /// Bundled language packs are named <c>&lt;code&gt;.plugin.lang.json</c> / <c>&lt;code&gt;.cheaploc.json</c>,
    /// so the language code decides which CheapLoc file is loaded.
    /// </summary>
    public static class CheapLocSetup
    {
        private const string DefaultLanguageCode = "cn";

        /// <summary>Language code currently applied to the plugin UI (for example <c>cn</c> or <c>tw</c>).</summary>
        public static string CurrentCode { get; private set; } = DefaultLanguageCode;

        /// <summary>Load <c>Resources\&lt;code&gt;.cheaploc.json</c> based on the given language resource name.</summary>
        /// <param name="languageResourceName">Resource name such as <c>SonarPlugin.Resources.tw.plugin.lang.json</c>, may also be <c>file:...</c> or null.</param>
        /// <returns>The language code that was applied.</returns>
        public static string Apply(string? languageResourceName)
        {
            var assembly = typeof(SonarPluginIoC).Assembly;
            var code = GetLanguageCode(languageResourceName);
            CurrentCode = code;
            var resource = FindResource(assembly, code) ?? FindResource(assembly, DefaultLanguageCode);
            if (resource is null) return code;

            using var stream = assembly.GetManifestResourceStream(resource);
            if (stream is null) return code;

            using var reader = new StreamReader(stream);
            Loc.Setup(reader.ReadToEnd());
            return code;
        }

        /// <summary>Extract the language code from a language resource name.</summary>
        public static string GetLanguageCode(string? languageResourceName)
        {
            if (string.IsNullOrWhiteSpace(languageResourceName)) return DefaultLanguageCode;
            if (languageResourceName.StartsWith("file:", StringComparison.Ordinal)) return DefaultLanguageCode;

            var name = Path.GetFileName(languageResourceName);
            var index = name.IndexOf('.');
            if (index <= 0) return DefaultLanguageCode;
            return name[..index];
        }

        private static string? FindResource(Assembly assembly, string code)
            => assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith($".{code}.cheaploc.json", StringComparison.OrdinalIgnoreCase));
    }
}
