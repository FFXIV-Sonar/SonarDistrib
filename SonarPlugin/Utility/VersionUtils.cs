using System;
using System.Collections.Generic;
using System.Linq;
using Sonar.Models;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using System.Reflection;
using Dalamud.Data;

namespace SonarPlugin.Utility
{
    public static class VersionUtils
    {
        /// <summary>
        /// Get Sonar Plugin Version
        /// </summary>
        public static string GetSonarPluginVersion() => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        /// <summary>
        /// Get Dalamud Version
        /// </summary>
        public static string GetDalamudVersion() => typeof(DalamudStartInfo).Assembly.GetName().Version!.ToString();

        /// <summary>
        /// Get Dalamud Build Git Hash
        /// </summary>
        public static string GetDalamudBuild() => Dalamud.Utility.Util.GetGitHash();

        /// <summary>
        /// Get Dalamud Hash
        /// </summary>
        public static string GetDalamudHash() => SonarVersion.GetAssemblyHash(typeof(DalamudStartInfo).Assembly);

        /// <summary>
        /// Steal Game Version information from Dalamud's Start Info
        /// </summary>
        public static string GetGameVersion(DataManager data) => data.GameData.Repositories["ffxiv"].Version;

        /// <summary>
        /// Get SonarVersion for Sonar.NET
        /// </summary>
        public static SonarVersion GetSonarVersionModel(DataManager data)
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
