using SonarResources.Lumina;
using System;
using System.Collections.Generic;
using DryIoc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc.MefAttributedModel;
using SonarResources.Readers;
using Sonar.Data.Details;
using SonarResources.Lgb;

namespace SonarResources
{
    public static class Program
    {
        public static bool ShowProgress { get; set; }
        public static Container Container { get; private set; } = default!;
        
        public static void Main(string[] args)
        {
            using var container = new Container();
            Container = container;

            Container.RegisterExports(typeof(Program).Assembly);
            Container.RegisterInstance(new SonarDb());

            var manager = Container.Resolve<LuminaManager>();
            // TODO: Move this to a configuration file.
            manager.LoadLumina(@"R:\SquareEnix\FINAL FANTASY XIV - A Realm Reborn\game\sqpack");
            manager.LoadLumina(@"R:\SquareEnix\FFXIV_CN\game\sqpack");
            manager.LoadLumina(@"R:\SquareEnix\FFXIV_KR\game\sqpack");

            ShowProgress = false;
            Container.Resolve<ResourcesMain>();
        }

        public static void WriteProgress(string mark)
        {
            if (ShowProgress) Console.Write(mark);
        }

        public static void WriteProgress(char mark)
        {
            if (ShowProgress) Console.Write(mark);
        }

        public static void WriteProgressLine(string mark)
        {
            if (ShowProgress) Console.WriteLine(mark);
        }

        public static void WriteProgressLine(char mark)
        {
            if (ShowProgress) Console.WriteLine(mark);
        }
    }
}
