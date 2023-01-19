using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace SonarPlugin
{
    internal static class SonarModule
    {
        [SuppressMessage("Usage", "CA2255")] // Justification = "Costura"
        [ModuleInitializer]
        internal static void ModuleInit()
        {
            var context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
            if (context is not null)
            {
                context.Resolving += ResolveAssembly;
            }
            else
            {
                Debugger.Break();
                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            }
        }

        private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
        {
            var assemblyName = args.Name; //new AssemblyName(args.Name); <-- just in case, I don't know what it has
            Debugger.Break();
            if (assemblyName is null) return null;
            var resourceBytes = Assembly.GetExecutingAssembly().GetUncompressedManifestResourceBytes($"costura.{assemblyName.ToLowerInvariant()}.dll.compressed");
            if (resourceBytes is null) return null;
            try
            {
                return Assembly.Load(resourceBytes);
            }
            catch (Exception ex)
            {
#if DEBUG
                File.AppendAllText(@"C:\SonarModuleError.log", $"[{DateTime.UtcNow:u}] {ex}\n\n");
#endif
                // I have no choice but to be hungry and swallow this exception
                return null;
            }
        }

        private static Assembly? ResolveAssembly(AssemblyLoadContext context, AssemblyName name)
        {
            var assemblyName = name.Name;
            if (assemblyName is null) return null;
            var resourceStream = Assembly.GetExecutingAssembly().GetUncompressedManifestResourceStream($"costura.{assemblyName.ToLowerInvariant()}.dll.compressed");
            if (resourceStream is null) return null;
            try
            {
                return context.LoadFromStream(resourceStream);
            }
            catch (Exception ex)
            {
#if DEBUG
                File.AppendAllText(@"C:\SonarModuleError.log", $"[{DateTime.UtcNow:u}] {ex}\n\n");
#endif
                // I have no choice but to be hungry and swallow this exception
                return null;
            }
        }

        private static Stream? GetUncompressedManifestResourceStream(this Assembly assembly, string name)
        {
            var stream = assembly.GetManifestResourceStream(name);
            if (stream is null) return null;
            using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, false);
            var memoryStream = new MemoryStream();
            deflateStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static byte[]? GetUncompressedManifestResourceBytes(this Assembly assembly, string name)
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream is null) return null;
            var bytes = new byte[stream.Length];
            stream.Read(bytes);
            return bytes;
        }

        /// <summary>
        /// DO NOT CALL THIS
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void MakeCosturaStopComplainingAboutNotCallingCosturaUtilityInitialize() => CosturaUtility.Initialize(); // Else this loads stuff into the Dalamud context...
    }
}