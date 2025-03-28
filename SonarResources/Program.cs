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
using System.Text.Json.Nodes;
using System.IO;
using System.Text.Json;
using System.Threading;
using Lumina.Data;

namespace SonarResources
{
    public static class Program
    {
        public static bool ShowProgress { get; set; }

        public static Container Container { get; private set; } = default!;
        public static SonarResourcesConfig Config { get; private set; } = default!;
        
        public static async Task Main(string[] args)
        {
            var config = await LoadConfigurationAsync(File.Exists("config.json") ? "config.json" : null);
            Config = config;

            using var container = new Container();
            Container = container;

            Container.RegisterExports(typeof(Program).Assembly);
            Container.RegisterInstance(new SonarDb());

            var manager = Container.Resolve<LuminaManager>();
            foreach (var sqpack in config.GameSqpacks)
            {
                Console.WriteLine($"Initializing Game Data: {sqpack}");
                manager.LoadLumina(sqpack);
            }

            ShowProgress = false;
            Container.Resolve<ResourcesMain>();
        }

        public static async Task<SonarResourcesConfig> LoadConfigurationAsync(string? file = null, CancellationToken cancellationToken = default)
        {
            if (file is not null)
            {
                using var stream = File.OpenRead(file);
                var config = await JsonSerializer.DeserializeAsync<SonarResourcesConfig>(stream, cancellationToken: cancellationToken);
                if (config is not null) return config;
            }
            return new SonarResourcesConfig();
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
