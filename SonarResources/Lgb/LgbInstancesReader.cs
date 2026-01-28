using DryIoc.FastExpressionCompiler.LightExpression;
using DryIocAttributes;
using Lumina;
using Lumina.Data.Files;
using Lumina.Excel.Sheets;
using Sonar.Data.Details;
using Sonar.Numerics;
using SonarResources.Lumina;
using SonarResources.Readers;
using SonarUtils.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarResources.Lgb
{
    [ExportEx]
    [SingletonReuse]
    public sealed class LgbInstancesReader
    {
        private static readonly string[] s_filenames = [
            "bg.lgb",
            "planevent.lgb",
            "planlive.lgb",
            "planmap.lgb",
            // "planner.lgb", // This one doesn't seem to load properly but can be ignored
            "sound.lgb",
            "vfx.lgb",
        ];


        private readonly HashSet<LgbInstanceFile> _resourceFiles = new();
        private readonly Dictionary<uint, LgbInstance> _instances = new();
        private readonly Dictionary<uint, Dictionary<uint, LgbInstance>> _zoneInstances = new();
        private readonly List<(LgbInstanceFile File, Exception Exception)> _exceptions = new();

        private LuminaManager Luminas { get; }

        public LgbInstancesReader(LuminaManager luminas)
        {
            this.Luminas = luminas;
            Console.WriteLine("Determining LGB filenames");
            foreach (var data in this.Luminas.GetAllDatas())
            {
                Program.WriteProgress(this.ReadFilenames(data) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this._resourceFiles.Count})");

            Console.WriteLine("Processing LGB files");
            foreach (var file in this._resourceFiles)
            {
                var result = this.ReadLgb(file) switch
                {
                    LgbResult.Added => "+",
                    LgbResult.Missed => "-",
                    LgbResult.NotFound => "_",
                    LgbResult.Skipped => ".",
                    LgbResult.Error => "e",
                    _ => "?"
                };
                Console.Write(result);
            }
            Console.WriteLine($" ({this._instances.Count})");

            if (this._exceptions.Count > 0)
            {
                Console.WriteLine($"LGB Processing exceptions thrown: {this._exceptions.Count}");
                Console.WriteLine(string.Join("\n", this._exceptions.Select(i => $"{i.File.Key}: {i.Exception.GetType().Name}")));
            }
        }

        private bool ReadFilenames(GameData data)
        {
            var territories = data.GetExcelSheet<TerritoryType>()?
                .Where(t => !string.IsNullOrWhiteSpace(t.Bg.ExtractText()));
            if (territories is null) return false;

            var result = false;
            foreach (var territory in territories)
            {
                var bg = territory.Bg.ToString()[..^5];
                foreach (var filename in s_filenames)
                {
                    var path = $"bg/{bg}/{filename}";
                    if (data.FileExists(path)) result |= this._resourceFiles.Add(new(data, path, territory.RowId));
                }
            }
            return result;
        }

        private LgbResult ReadLgb(LgbInstanceFile file)
        {
            LgbFile? lgb;
            try
            {
                lgb = file.Data.GetFile<LgbFile>(file.Path);
                if (lgb is null) return LgbResult.NotFound;
            }
            catch (Exception ex)
            {
                lock (this._exceptions) this._exceptions.Add((file, ex));
                return LgbResult.Error;
            }

            var result = false;
            var zoneId = file.ZoneId;
            foreach (var layer in lgb.Layers)
            {
                foreach (var instance in layer.InstanceObjects)
                {
                    var id = instance.InstanceId;
                    if (this._instances.ContainsKey(id)) continue;

                    var transform = instance.Transform;
                    var positionX = transform.Translation.X;
                    var positionY = transform.Translation.Z; // y and z being swapped is intentional
                    var positionZ = transform.Translation.Y;
                    var scaleX = transform.Scale.X;
                    var scaleY = transform.Scale.Z; // y and z being swapped is intentional
                    var scaleZ = transform.Scale.Y;
                    var rotationX = transform.Rotation.X;
                    var rotationY = transform.Rotation.Z; // y and z being swapped is intentional
                    var rotationZ = transform.Rotation.Y;
                    var translationX = transform.Translation.X;
                    var translationY = transform.Translation.Z; // y and z being swapped is intentional
                    var translationZ = transform.Translation.Y;

                    var item = new LgbInstance()
                    {
                        Name = $"{layer.Name}:{instance.Name}",
                        Type = instance.AssetType,
                        Object = instance.Object,
                        ZoneId = zoneId,
                        Coords = new(positionX, positionY, positionZ),
                        Scale = new(scaleX, scaleY, scaleZ),
                        Rotation = new(rotationX, rotationY, rotationZ), // NOTE: Currently unused
                        Translation = new(translationX, translationY, translationZ),
                    };

                    result |= this._instances.TryAdd(id, item);
                    if (!this._zoneInstances.TryGetValue(zoneId, out var zoneInstances))
                    {
                        this._zoneInstances[zoneId] = zoneInstances = new();
                    }
                    result |= zoneInstances.TryAdd(id, item);
                }
            }
            return result ? LgbResult.Added : LgbResult.Missed;
        }

        public LgbInstance? GetInstance(uint instanceId) => this._instances.GetValueOrDefault(instanceId);
        public IEnumerable<LgbInstance> GetInstances() => this._instances.Values;
        public IEnumerable<LgbInstance> GetZoneInstances(uint zoneId) => this._zoneInstances.GetValueOrDefault(zoneId)?.Values ?? Enumerable.Empty<LgbInstance>();
    }
}
