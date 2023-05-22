using MessagePack;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Sonar.Messages;

namespace Sonar.Models
{
    /// <summary>
    /// Please fill in the following fields
    /// - Plugin
    /// - PluginHash (Integrity checking)
    /// - Dalamud (if in Dalamud) (future: rename this field since I may include ACT in)
    /// </summary>
    [JsonObject]
    [MessagePackObject] // TODO: Change to key indexes
    [Serializable]
    public sealed class SonarVersion : ISonarMessage, ICloneable
    {
        [Key("version")]
        public int Version { get; set; } = SonarConstants.SonarVersion;

        [Key("sonarNETVersion")] // TODO: Rename to sonar
        public string? Sonar { get; set; }

        [Key("sonarHash")]
        public string? SonarHash { get; set; }

        [Key("plugin")]
        public string? Plugin { get; set; }

        [Key("pluginHash")]
        public string? PluginHash { get; set; }

        [Key("game")]
        public string? Game { get; set; }

        [Key("dalamudVersion")] // TODO: Rename to dalamud
        public string? Dalamud { get; set; }

        [Key("dalamudHash")]
        public string? DalamudHash { get; set; }

        [Key("os")]
        public string? OperatingSystem { get; set; }

        public void ResetSonarVersion()
        {
            var assembly = typeof(SonarVersion).Assembly;
            this.Version = SonarConstants.SonarVersion;
            this.Sonar = assembly.GetName().Version!.ToString();
            this.SonarHash = GetAssemblyHash(assembly);
            this.OperatingSystem = Environment.OSVersion.VersionString;
        }

        public object Clone() => this.MemberwiseClone();

        public override string ToString()
        {
            List<string> output = new(5);
            output.Add($"Sonar: {this.Sonar} ({this.Version})");

            if (!string.IsNullOrWhiteSpace(this.Plugin))
                output.Add($"Plugin: {this.Plugin}");

            if (!string.IsNullOrWhiteSpace(this.Game))
                output.Add($"Game: {this.Game}");

            if (!string.IsNullOrWhiteSpace(this.Dalamud))
                output.Add($"Dalamud: {this.Dalamud}");

            if (!string.IsNullOrWhiteSpace(this.OperatingSystem))
                output.Add($"OS: {this.OperatingSystem}");

            return string.Join(" | ", output);
        }

        public string ToHashesString()
        {
            var output = new List<string>(7)
            {
                $"Sonar: {this.SonarHash ?? "Unknown"}",
                $"Plugin: {this.PluginHash ?? "Unknown"}",
                $"Dalamud: {this.DalamudHash ?? "Unknown"}",
            };
            return string.Join(" | ", output);
        }

        /// <summary>
        /// Generates a hash from assembly
        /// </summary>
        /// <remarks>If failed return starts with Unknown</remarks>
        public static string GetAssemblyHash(Assembly assembly)
        {
            try
            {
                return Convert.ToBase64String(SHA256.HashData(File.ReadAllBytes(assembly.Location)));
            }
            catch (Exception ex)
            {
                return $"Unknown ({ex.GetType().Name}: {ex.Message})"; // No stack trace
            }
        }
    }
}
