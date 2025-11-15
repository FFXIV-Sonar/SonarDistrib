using AG;
using AG.EnumLocalization;
using Sonar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SonarPlugin.Utility
{
    public static class EnumLocUtils
    {
        private static readonly Regex s_resourceNameRegex = new(@"^.*\.lang\.json$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly ConcurrentDictionary<Assembly, ImmutableArray<string>> s_languages = new();

        /// <summary>RunSetup localization.</summary>
        /// <param name="threaded">Launch a background task to perform the setup.</param>
        /// <param name="debugFallbacks">Use debugFallbacks fallbacks.</param>
        public static void Setup(bool debugFallbacks)
        {
            SetupCore(debugFallbacks);
        }

        public static ImmutableArray<string> GetLanguageResources(Assembly assembly) => s_languages.GetOrAdd(assembly, GetLanguageResourcesCore);

        public static void SetLanguage(Assembly assembly, string? langCode)
        {
            EnumLoc.SetDefaultLanguage(langCode, assembly);
            if (langCode?.StartsWith("file:") is true)
            {
                try
                {
                    SetLanguageCore(assembly, langCode);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    GC.KeepAlive(ex);
                }
            }
        }

        private static void SetupCore(bool debug)
        {
            Span<Assembly> assemblies = [typeof(SonarPlugin).Assembly, typeof(SonarClient).Assembly];
            foreach (var assembly in assemblies)
            {
                try
                {
                    SetupAssemblyCore(assembly, debug);
                    var langCode = EnumLoc.GetDefaultLanguage(assembly);
                    if (langCode?.StartsWith("file:") is true) SetLanguageCore(assembly, langCode);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    GC.KeepAlive(ex);
                }
            }
        }

        private static void SetupAssemblyCore(Assembly assembly, bool debug)
        {
            EnumLoc.SetupAssembly(assembly, debug);
            foreach (var name in GetLanguageResources(assembly))
            {
                using var stream = assembly.GetManifestResourceStream(name);
                ThrowHelper.ThrowIf(stream is null, static () => new NullReferenceException("Opened stream was null."));
                LoadLanguageCore(assembly, name, stream);
            }
        }

        private static ImmutableArray<string> GetLanguageResourcesCore(Assembly assembly)
        {
            var result = new List<string>();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                var match = s_resourceNameRegex.Match(name);
                if (!match.Success) continue;
                result.Add(name);
            }
            return [.. result];
        }

        /// <summary>Precondition: <paramref name="name"/> starts with <c>file:</c></summary>
        private static void SetLanguageCore(Assembly assembly, string? name)
        {
            Debug.Assert(name?.StartsWith("file:") is true);
            try
            {
                var stream = File.OpenRead(name[5..]);
                ThrowHelper.ThrowIf(stream is null, () => new NullReferenceException("Opened stream was null."));
                LoadLanguageCore(assembly, name, stream);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                GC.KeepAlive(ex);
            }
        }

        private static void LoadLanguageCore(Assembly assembly, string name, Stream stream)
        {
            var data = JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(stream);
            ThrowHelper.ThrowIf(data is null, () => new NullReferenceException("Stream deserialization returned null."));
            EnumLoc.SetupLanguage(name, data, assembly);
        }
    }
}
