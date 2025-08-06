using DryIocAttributes;
using Sonar.Data.Details;
using Sonar.Data.Rows;
using SonarResources.Aetherytes;
using SonarResources.Providers;
using SonarResources.Readers;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SonarResources.Maps;
using SonarResources.Lumina;
using Sonar;
using System.Threading;
using SonarUtils;

namespace SonarResources
{
    [ExportEx]
    [SingletonReuse]
    public sealed class ResourcesMain
    {
        private SonarDb Db { get; }
        private LuminaManager Luminas { get; }
        
        // Argument order doesn't matter, only their presence
        public ResourcesMain(
            // Readers
            AchievementsReader _1,
            AetheryteReader _11,
            DatacenterReader _2,
            FateReader _3,
            HuntReader _4,
            MapReader _5,
            WeatherReader _6,
            WorldReader _7,
            ZoneReader _8,

            // Providers
            AudienceProvider _9,
            RegionProvider _10,
            WorldTravelProvider _12,

            // Database
            LuminaManager luminas,
            SonarDb db
            )
        {
            this.Luminas = luminas;
            this.Db = db;

            this.GenerateMapAssetsAsync().GetAwaiter().GetResult();

            Console.WriteLine("Serializing and deserializing");
            var dbBytes = SonarSerializer.SerializeServerToClient(db);
            this.Db = SonarSerializer.DeserializeServerToClient<SonarDb>(dbBytes);

            Console.WriteLine("Generating metadata");
            var timestamp = UnixTimeHelper.UnixNow;
            this.Db.Timestamp = timestamp;
            this.Db.Hash = this.Db.ComputeHash();

            db.Timestamp = timestamp;
            db.Hash = db.ComputeHash();
            if (!this.Db.HashString.Equals(db.HashString, StringComparison.Ordinal))
            {
                // TODO: Figure out why this happens
                //Console.WriteLine("MUTATION DETECTED DURING SERIALIZATION AND DESERIALIZATION STEPS");
                //Console.WriteLine($"{db.HashString}");
                //Console.WriteLine($"{this.Db.HashString}");
                Console.WriteLine();

                Console.WriteLine("Database Result (Original)");
                Console.WriteLine("===============");
                Console.WriteLine(db);
                Console.WriteLine("---------------\n");

                Console.WriteLine("Database Result (Serialized)");
                Console.WriteLine("===============");
                Console.WriteLine(this.Db);
                Console.WriteLine("---------------\n");

                var dbBytes2 = SonarSerializer.SerializeServerToClient(this.Db);
                var db2 = SonarSerializer.DeserializeServerToClient<SonarDb>(dbBytes2);
                db2.Timestamp = timestamp;
                db2.Hash = db2.ComputeHash();

                // This is isually the same, so the mutation only happen on the first time.
                Console.WriteLine("Database Result (Double Serialization - Debug)");
                Console.WriteLine("===============");
                Console.WriteLine(db2);
                Console.WriteLine("---------------\n");

                // If it does happen twice let me know.
                if (Debugger.IsAttached && !this.Db.HashString.Equals(db2.HashString, StringComparison.Ordinal)) Debugger.Break();
            }

            // Data Views
            DataViewer.ShowWorlds(this.Db);

            // Save resources
            Console.WriteLine("Saving resources");
            this.SaveSonarDb(Path.Join(Program.Config.ResourcesPath, "Db.data"));
            this.SaveSonarDb(Path.Join(Program.Config.AssetsPath, "resources", "Db.data"));

            Console.WriteLine("Saving JSON files"); // NOTE: Paths inside these methods are assumed
            SaveToJson(this.Db.Audiences);
            SaveToJson(this.Db.Regions);
            SaveToJson(this.Db.Datacenters);
            SaveToJson(this.Db.Worlds);
            SaveToJson(this.Db.Maps);
            SaveToJson(this.Db.Zones);
            SaveToJson(this.Db.Hunts);
            SaveToJson(this.Db.Fates);
            SaveToJson(this.Db.Weathers);
            SaveToJson(this.Db.Aetherytes);
            SaveToJson(this.Db.WorldTravelData);

            Console.WriteLine("\nDatabase Result");
            Console.WriteLine("===============");
            Console.WriteLine(this.Db);
            Console.WriteLine("---------------");
        }

        public void SaveSonarDb(string filename)
        {
            Console.Write($"Saving {filename}...");
            var bytes = SonarSerializer.SerializeData(this.Db);
            File.WriteAllBytes(filename, bytes);
            Console.WriteLine($"{bytes.Length} bytes saved");
        }

        private static void SaveToJson<T>(IDictionary<uint, T> dict) where T : IDataRow
        {
            var name = typeof(T).Name;

            var filename = Path.Join(Program.Config.AssetsPath, "data", $"{name.Substring(0, name.Length - 3).ToLowerInvariant()}.json");
            Console.Write($"Saving {filename} (Count: {dict.Count})...");
            var jsonOptions2 = new JsonSerializerOptions(JsonSerializerOptions.Default) { WriteIndented = true };
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dict, jsonOptions2));
            File.WriteAllBytes(filename, bytes);
            Console.WriteLine($"{bytes.Length} bytes saved");

            filename = Path.Join(Program.Config.AssetsPath, "data", $"{name.Substring(0, name.Length - 3).ToLowerInvariant()}.min.json");
            Console.Write($"Saving {filename} (Count: {dict.Count})...");
            bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dict));
            File.WriteAllBytes(filename, bytes);
            Console.WriteLine($"{bytes.Length} bytes saved");
        }

        private async Task GenerateMapAssetsAsync(CancellationToken cancellationToken = default)
        {
            var maps = Program.Config.BuildMapImages;
            if (Program.Config.Interactive) maps = ConsoleHelper.AskUserYN("This take a really long time, generate all map assets?");
            if (!maps) return;

            var zones = Program.Config.BuildZoneImages;
            var parallel = Program.Config.BuildMapParallel;
            if (Program.Config.Interactive)
            {
                zones = ConsoleHelper.AskUserYN("Generate all zone assets?");
                parallel = ConsoleHelper.AskUserYN("Perform image processing in parallel?") ? -1 : 1;
            }

            var generator = new MapsGenerator();
            foreach (var data in this.Luminas.GetAllDatas())
            {
                await generator.GenerateAllMapImagesAsync(data, this.Db, parallel, cancellationToken);
            }

            Console.WriteLine("Propagating blurhash from maps to zones");
            foreach (var zone in this.Db.Zones.Values)
            {
                zone.BlurHash = this.Db.Maps.GetValueOrDefault(zone.MapId)?.BlurHash;
            }
            
            if (!zones) return;
            foreach (var data in this.Luminas.GetAllDatas())
            {
                await generator.GenerateAllZoneImagesAsync(data, cancellationToken);
            }
        }
    }
}
