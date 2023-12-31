using DryIocAttributes;
using Lumina;
using Lumina.Excel.GeneratedSheets2;
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
                var dcId = worldRow.DataCenter.Row;
                var dc = this.Db.Datacenters[dcId];

                if (!this.Db.Worlds.TryGetValue(id, out var world))
                {
                    this.Db.Worlds[id] = world = new()
                    {
                        Id = id,
                        Name = worldRow.Name.ToTextString(),
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

            // Setup CN Worlds
            // Attempt to support Chinesse worlds (https://ff.web.sdo.com/web8/index.html#/servers)
            // Range taken from https://github.com/xivapi/ffxiv-datamining/blob/master/csv/World.csv 164 worlds
            foreach (var world in this.Db.Worlds.Values.Where(world => world.Id is >= 1040 and <= 1203))
            {
                //this.SetWorldDc(world.Id, "China", true); // No longer needed. Keeping foreach loop just in case
            }
            this.SetWorldDc2("水晶塔", "豆豆柴", true); //3
            this.SetWorldDc2("银泪湖", "豆豆柴", true); //5
            this.SetWorldDc2("太阳海岸", "豆豆柴", true); //2
            this.SetWorldDc2("伊修加德", "豆豆柴", true); //1
            this.SetWorldDc2("红茶川", "豆豆柴", true); //4
            this.SetWorldDc2("黄金谷", "豆豆柴", true);
            this.SetWorldDc2("月牙湾", "豆豆柴", true);
            this.SetWorldDc2("雪松原", "豆豆柴", true);
            this.Db.Worlds[1057].IsPublic = false;
            this.Db.Worlds[1048].IsPublic = false;
            this.Db.Worlds[1064].IsPublic = false; // So far, all 10xx worlds are fake
            this.Db.Worlds[1074].IsPublic = false; // Correct worlds are 11xx and 12xx
            this.Db.Worlds[1056].IsPublic = false;
            this.Db.Worlds[1050].IsPublic = false;
            this.Db.Worlds[1058].IsPublic = false;
            this.Db.Worlds[1068].IsPublic = false;

            this.SetWorldDc("紫水栈桥", "猫小胖", true);
            this.SetWorldDc("延夏", "猫小胖", true);
            this.SetWorldDc("静语庄园", "猫小胖", true);
            this.SetWorldDc("摩杜纳", "猫小胖", true);
            this.SetWorldDc("海猫茶屋", "猫小胖", true);
            this.SetWorldDc("柔风海湾", "猫小胖", true);
            this.SetWorldDc("琥珀原", "猫小胖", true);

            this.SetWorldDc("白银乡", "莫古力", true);
            this.SetWorldDc("白金幻象", "莫古力", true);
            this.SetWorldDc("神拳痕", "莫古力", true);
            this.SetWorldDc("潮风亭", "莫古力", true);
            this.SetWorldDc("旅人栈桥", "莫古力", true);
            this.SetWorldDc("拂晓之间", "莫古力", true);
            this.SetWorldDc("龙巢神殿", "莫古力", true);
            this.SetWorldDc("梦羽宝境", "莫古力", true);

            this.SetWorldDc("红玉海", "陆行鸟", true);
            this.SetWorldDc("神意之地", "陆行鸟", true);
            this.SetWorldDc("拉诺西亚", "陆行鸟", true);
            this.SetWorldDc("幻影群岛", "陆行鸟", true);
            this.SetWorldDc("萌芽池", "陆行鸟", true);
            this.SetWorldDc("宇宙和音", "陆行鸟", true);
            this.SetWorldDc("沃仙曦染", "陆行鸟", true);
            this.SetWorldDc("晨曦王座", "陆行鸟", true);

            // Attempt to support Korean worlds
            // Range taken from https://github.com/xivapi/ffxiv-datamining/blob/master/csv/World.csv 33 worlds
            foreach (var world in this.Db.Worlds.Values.Where(world => world.Id is >= 2048 and <= 2080))
            {
                this.SetWorldDc(world.Id, "Korea", true); // Only seen Korean data twice
            }
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
