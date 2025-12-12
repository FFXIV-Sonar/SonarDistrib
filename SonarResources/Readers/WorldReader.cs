using DryIocAttributes;
using Lumina;
using Lumina.Excel.Sheets;
using Sonar.Data.Details;
using SonarResources.Lumina;
using SonarResources.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using SonarUtils;
using Spectre.Console;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class WorldReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }

        public WorldReader(LuminaManager luminas, SonarDb db, DatacenterReader _)
        {
            this.Luminas = luminas;
            this.Db = db;

            Console.WriteLine("Reading all worlds");
            foreach (var data in this.Luminas.GetAllDatas())
            {
                Program.WriteProgress(this.Read(data) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this.Db.Worlds.Count})");

            Console.WriteLine("Setting worlds");
            this.SetupWorlds();

            Console.WriteLine("Propagating worlds IsPublic to Data Centers, Regions and Audiences");
            this.PropagatePublic();
        }

        [SuppressMessage("Style", "IDE0059")]
        private bool Read(GameData data)
        {
            var worldSheet = data.GetExcelSheet<World>();
            if (worldSheet is null) return false;

            var result = false;
            foreach (var worldRow in worldSheet)
            {
                var id = worldRow.RowId;
                var dcId = worldRow.DataCenter.RowId;
                var dc = this.Db.Datacenters[dcId];

                if (!this.Db.Worlds.TryGetValue(id, out var world))
                {
                    this.Db.Worlds[id] = world = new()
                    {
                        Id = id,
                        Name = worldRow.Name.ExtractText(),
                        DatacenterId = dcId,
                        RegionId = dc.RegionId,
                        AudienceId = dc.AudienceId,
                        IsPublic = worldRow.IsPublic && dcId != 0
                    };
                    result = true;
                }
            }
            return result;
        }

        private void SetupWorlds()
        {
            // Special Case for Sonar
            this.Db.Worlds[0] = new() { Id = 0, Name = "INVALID" };

            // Temporary: Set all worlds public for Dynamis
            this.SetAllWorldsPublic("Dynamis");
            this.SetAllWorldsPublic("Shadow");

            this.SetAllWorldsPublic(101);
            this.SetAllWorldsPublic(102);
            this.SetAllWorldsPublic(103);
            this.SetAllWorldsPublic(104);
            this.SetAllWorldsPublic(201);

            // Temporary: Add TC Worlds
            // https://github.com/harukaxxxx/ffxiv-datamining-tw/blob/main/World.csv
            // https://discord.com/channels/205430339907223552/693223864741920788/1449038383253950647
            // https://discord.com/channels/205430339907223552/1448275645103734795
            this.CustomWorld(4028, "伊弗利特", "陸行鳥");
            this.CustomWorld(4029, "迦樓羅", "陸行鳥");
            this.CustomWorld(4030, "利維坦", "陸行鳥");
            this.CustomWorld(4031, "鳳凰", "陸行鳥");
            this.CustomWorld(4032, "奧汀", "陸行鳥");
            this.CustomWorld(4033, "巴哈姆特", "陸行鳥");
            this.CustomWorld(4034, "拉姆", "陸行鳥");
            this.CustomWorld(4035, "泰坦", "陸行鳥");
            this.SetAllWorldsPublic("陸行鳥"); // 151
        }

        private void CustomWorld(uint worldId, string worldName, string dcName, bool isPublic = true)
        {
            var dc =
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCulture)) ??
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"Datacenter {dcName} does not exist", nameof(dcName));

            this.Db.Worlds[worldId] = new()
            {
                Id = worldId,
                Name = worldName,
                DatacenterId = dc.Id,
                RegionId = dc.RegionId,
                AudienceId = dc.AudienceId,
                IsPublic = isPublic
            };
        }

        private void SetWorldDc(string worldName, string dcName, bool? isPublic = null)
        {
            var world =
                this.Db.Worlds.Values.FirstOrDefault(world => world.Name.Equals(worldName, StringComparison.InvariantCulture)) ??
                this.Db.Worlds.Values.FirstOrDefault(world => world.Name.Equals(worldName, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"World {worldName} does not exist", nameof(worldName));

            var dc =
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCulture)) ??
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"Datacenter {dcName} does not exist", nameof(dcName));

            world.DatacenterId = dc.Id;
            world.RegionId = dc.RegionId;
            world.AudienceId = dc.AudienceId;
            world.IsPublic = isPublic ?? world.IsPublic;
        }

        private void SetWorldDc(uint worldId, string dcName, bool? isPublic = null)
        {
            var world = this.Db.Worlds[worldId];

            var dc =
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCulture)) ??
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"Datacenter {dcName} does not exist", nameof(dcName));

            world.DatacenterId = dc.Id;
            world.RegionId = dc.RegionId;
            world.AudienceId = dc.AudienceId;
            world.IsPublic = isPublic ?? world.IsPublic;
        }

        private void SetWorldDc2(string worldName, string dcName, bool? isPublic = null)
        {
            var worlds = this.Db.Worlds.Values.Where(world => world.Name.Equals(worldName, StringComparison.InvariantCultureIgnoreCase));
            if (!worlds.Any()) throw new ArgumentException($"World {worldName} does not exist", nameof(worldName));

            var dc =
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCulture)) ??
                this.Db.Datacenters.Values.FirstOrDefault(dc => dc.Name.Equals(dcName, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"Datacenter {dcName} does not exist", nameof(dcName));

            foreach (var world in worlds)
            {
                world.DatacenterId = dc.Id;
                world.RegionId = dc.RegionId;
                world.AudienceId = dc.AudienceId;
                world.IsPublic = isPublic ?? world.IsPublic;
            }
        }

        private void SetAllWorldsPublic(string datacenterName)
        {
            SetAllWorldsPublic(this.Db.Datacenters.Values.First(dc => dc.Name.Equals(datacenterName, StringComparison.InvariantCultureIgnoreCase)).Id);
        }

        private void SetAllWorldsPublic(uint datacenterId)
        {
            Console.WriteLine($"Setting all worlds public for {this.Db.Datacenters[datacenterId].Name} data center");
            this.Db.Worlds.Values
                .Where(world => world.DatacenterId == datacenterId)
                .ForEach(world =>
                {
                    var oldPublic = world.IsPublic;
                    world.IsPublic = true;
                    Console.WriteLine($"- {world.Name}: {oldPublic} => {world.IsPublic}");
                });
        }

        private void PropagatePublic()
        {
            foreach (var dc in this.Db.Datacenters.Values)
            {
                dc.IsPublic = this.Db.Worlds.Values.Any(world => world.DatacenterId == dc.Id && world.IsPublic);
            }

            foreach (var region in this.Db.Regions.Values)
            {
                region.IsPublic = this.Db.Datacenters.Values.Any(dc => dc.RegionId == region.Id && dc.IsPublic);
            }

            foreach (var audience in this.Db.Audiences.Values)
            {
                audience.IsPublic = this.Db.Regions.Values.Any(region => region.AudienceId == audience.Id && region.IsPublic);
            }
        }
    }
}
