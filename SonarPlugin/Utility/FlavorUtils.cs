using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SonarPlugin.Utility
{
    internal static class FlavorUtils
    {
        public static string? DetermineFlavor(IDalamudPluginInterface pluginInterface, IPluginLog logger)
        {
            logger.Debug("Determining Flavor");

            // Attempt #1: Flavor resource
            var flavor = GetFlavorResource(logger);
            if (flavor is not null) return flavor;

            // Attempt #2: Testing
            if (pluginInterface.IsDev) return "dev";
            else if (pluginInterface.IsTesting) return "testing";

            // Attempt #3 and #4: Internal name and Directory name
            flavor = DetermineFlavorCore(pluginInterface.InternalName) ?? DetermineFlavorCore(pluginInterface.AssemblyLocation.Directory?.Name);
            if (flavor is not null) return flavor;

            // Attempt #5: Give up
            logger.Warning("Unable to determine flavor");
            return null;
        }

        [SuppressMessage("Performance", "SYSLIB1045")]
        private static string? DetermineFlavorCore(string? input)
        {
            // Attempt #1: No flavour
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Attempt #2: SonarPlugin-something
            var match = Regex.Match(input, @"^SonarPlugin-?(?<flavor>.*)$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            if (match.Success)
            {
                var flavor = match.Groups["flavor"].Value;
                if (string.IsNullOrEmpty(flavor)) return input[^1] is '-' ? "negative" : null;
                return flavor;
            }

            // Attempt #3: bin (likely to be a dev build)
            match = Regex.Match(input, @"^bin$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            if (match.Success) return "dev";

            // Attempt #4: Give up
            return input;
        }

        private static string? GetFlavorResource(IPluginLog logger)
        {
            // Open the Flavor.data embedded resource stream
            var assembly = typeof(SonarPluginStub).Assembly;
            var stream = assembly.GetManifestResourceStream("SonarPlugin.Resources.Flavor.data");
            if (stream is null)
            {
                logger.Warning("Flavor resource not found!");
                return null;
            }

            // Read the stream into a bytes array
            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes, 0, bytes.Length);

            // Decode the flavor string
            var flavor = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(flavor))
            {
                logger.Debug("Resource flavor is empty");
                return null;
            }
            return flavor;
        }
    }
}
