using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources
{
    public sealed class SonarResourcesConfig
    {
        public string[] GameSqpacks { get; set; } = [@"G:\FFXIV\global\game\sqpack", @"G:\FFXIV\cn\game\sqpack", @"G:\FFXIV\kr\game\sqpack", @"G:\FFXIV\tw\game\sqpack"];
        public string AssetsPath { get; set; } = @"G:\FFXIV\assets";
        public string ResourcesPath { get; set; } = "../../../Sonar/Resources";

        public bool BuildMapImages { get; set; } = false;
        public bool BuildZoneImages { get; set; } = false;

        public int BuildMapParallel { get; set; } = -1;

        public bool Interactive { get; set; } = true;
    }
}
