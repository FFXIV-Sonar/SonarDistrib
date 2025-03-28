using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources
{
    public sealed class SonarResourcesConfig
    {
        public string[] GameSqpacks { get; set; } = [@"R:\SquareEnix\FINAL FANTASY XIV - A Realm Reborn\game\sqpack", @"R:\SquareEnix\FFXIV_CN\game\sqpack", @"R:\SquareEnix\FFXIV_KR\game\sqpack"];
        public string AssetsPath { get; set; } = "../../../Assets";
        public string ResourcesPath { get; set; } = "../../../Sonar/Resources";

        public bool BuildMapImages { get; set; } = false;
        public bool BuildZoneImages { get; set; } = false;

        public int BuildMapParallel { get; set; } = -1;

        public bool Interactive { get; set; } = true;
    }
}
