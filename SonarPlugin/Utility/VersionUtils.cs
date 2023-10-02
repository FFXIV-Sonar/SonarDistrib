using System;
using System.Collections.Generic;
using System.Linq;
using Sonar.Models;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Plugin.Services;
using Dalamud.Interface;
using Dalamud.Utility;
using System.Runtime.CompilerServices;

namespace SonarPlugin.Utility
{
    public static class VersionUtils
    {
        /// <summary>
        /// Get Sonar Plugin Version
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetSonarPluginVersion() => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        /// <summary>
        /// Get Dalamud Version
        /// </summary>
        public static string GetDalamudVersion() => Util.AssemblyVersion;

        /// <summary>
        /// Get Dalamud Build Git Hash
        /// </summary>
        public static string GetDalamudBuild() => Util.GetGitHash();

        /// <summary>
        /// Get Dalamud Hash
        /// </summary>
        public static string GetDalamudHash() => SonarVersion.GetAssemblyHash(typeof(Util).Assembly);

        /// <summary>
        /// Steal Game Version information from Dalamud's Start Info
        /// </summary>
        public static string GetGameVersion(IDataManager data) => data.GameData.Repositories["ffxiv"].Version;

        /// <summary>
        /// Get SonarVersion for Sonar.NET
        /// </summary>
        public static SonarVersion GetSonarVersionModel(IDataManager data)
        {
            return new SonarVersion
            {
                Game = GetGameVersion(data),
                Plugin = $"{Assembly.GetExecutingAssembly().GetName().Name} {GetSonarPluginVersion()}",
                PluginHash = SonarVersion.GetAssemblyHash(Assembly.GetExecutingAssembly()),
                Dalamud = $"{GetDalamudVersion()} ({GetDalamudBuild()})",
                DalamudHash = GetDalamudHash()
            };
        }
    }
}
