using DryIoc;
using DryIoc.MefAttributedModel;
using Lumina;
using Lumina.Data;
using Sonar.Data;
using Sonar.Data.Details;
using SonarResources.Lgb;
using SonarResources.Lumina;
using SonarResources.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace SonarResources
{
    public static class Program
    {
        public static bool ShowProgress { get; set; }

        public static Container Container { get; private set; } = default!;
        public static SonarResourcesConfig Config { get; private set; } = default!;
        
        public static async Task Main(string[] args)
        {
            var config = await LoadConfigurationAsync(File.Exists("config.json") ? "config.json" : null).ConfigureAwait(false);
            Config = config;

            using var container = new Container();
            Container = container;

            var currentDb = Database.Instance;
            GC.KeepAlive(currentDb);

            Container.RegisterExports(typeof(Program).Assembly);
            Container.RegisterInstance(new SonarDb());

            var manager = Container.Resolve<GameDataManager>();

            var tasks = new List<Task<GameData>>();
            foreach (var sqpack in config.GameSqpacks)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Game Data Initializing: {sqpack}");
                    var gameData = LoadGameData(sqpack);
                    Console.WriteLine($"Game Data Initialized: {sqpack}");
                    return gameData;
                }, CancellationToken.None, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
                tasks.Add(task);
            }

            foreach (var task in tasks)
            {
                manager.Add(await task.ConfigureAwait(false));
            }

            ShowProgress = true;
            Container.Resolve<ResourcesMain>();
        }

        public static GameData LoadGameData(string sqPath)
        {
            return new(sqPath, new()
            {
                CacheFileResources = true,
                PanicOnSheetChecksumMismatch = false,
                LoadMultithreaded = true,
            });
        }

        public static async Task<SonarResourcesConfig> LoadConfigurationAsync(string? file = null, CancellationToken cancellationToken = default)
        {
            if (file is not null)
            {
                using var stream = File.OpenRead(file);
                var config = await JsonSerializer.DeserializeAsync<SonarResourcesConfig>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
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
