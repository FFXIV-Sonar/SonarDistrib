using DryIocAttributes;
using Lumina;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Sonar.Data.Details;
using SonarResources.Lumina;
using SonarResources.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class DatacenterReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }

        public DatacenterReader(LuminaManager luminas, SonarDb db, RegionProvider _)
        {
            this.Luminas = luminas;
            this.Db = db;

            Console.WriteLine("Reading all data centers");
            foreach (var entry in this.Luminas.GetAllDatas())
            {
                Program.WriteProgress(this.Read(entry) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this.Db.Datacenters.Count})");

            Console.WriteLine("Setting up Lodestone support");
            this.SetLodestones();

            Console.WriteLine("Setting up additional Data Centers");
            this.SetCustomDatacenters();
        }

        private bool Read(GameData data)
        {
            var dcSheet = data.GetExcelSheet<WorldDCGroupType>();
            if (dcSheet is null) return false;

            var result = false;
            foreach (var dcRow in dcSheet)
            {
                var id = dcRow.RowId;
                if (!this.Db.Datacenters.TryGetValue(id, out var dc))
                {
                    result = true;
                    var region = this.Db.Regions[dcRow.Region];
                    this.Db.Datacenters[id] = dc = new()
                    {
                        Id = id,
                        Name = dcRow.Name.ExtractText(),
                        RegionId = region.Id,
                        AudienceId = region.AudienceId,
                    };
                }
            }
            return result;
        }

        private void SetLodestones()
        {
            this.HasLodestone("Aether", "Crystal", "Primal", "Dynamis");
            this.HasLodestone("Chaos", "Light", "Shadow");
            this.HasLodestone("Elemental", "Gaia", "Mana", "Meteor");
            this.HasLodestone("Materia");
        }

        private void SetCustomDatacenters()
        {
            // Attempt to support Chinesse worlds
            //CustomDC(1000, "China", 1000, 1000, "China");
            //this.SetCustomDatacenter(1001, "豆豆柴", "CN");
            //this.SetCustomDatacenter(1002, "猫小胖", "CN");
            //this.SetCustomDatacenter(1003, "莫古力", "CN");
            //this.SetCustomDatacenter(1004, "陆行鸟", "CN");

            // Attempt to support Korean worlds
            //this.SetCustomDatacenter(2000, "Korea", "KR");
        }

        private void SetCustomDatacenter(uint id, string dcName, string regionName)
        {
            var region =
                this.Db.Regions.Values.FirstOrDefault(region => region.Name.Equals(regionName, StringComparison.InvariantCulture)) ??
                this.Db.Regions.Values.FirstOrDefault(region => region.Name.Equals(regionName, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"Region {regionName} not found", nameof(regionName));

            this.Db.Datacenters[id] = new()
            {
                Id = id,
                Name = dcName,
                RegionId = region.Id,
                AudienceId = region.AudienceId,
            };
        }

        private void HasLodestone(params string[] dcNames)
        {
            foreach (var dcName in dcNames)
            {
                var dc =
                    this.Db.Datacenters.Values.FirstOrDefault(datacenter => datacenter.Name.Equals(dcName, StringComparison.InvariantCulture)) ??
                    this.Db.Datacenters.Values.FirstOrDefault(datacenter => datacenter.Name.Equals(dcName, StringComparison.InvariantCultureIgnoreCase)) ??
                    throw new ArgumentException($"Datacenter {dcName} not found", nameof(dcNames));
                dc.HasLodestone = true;
            }
        }
    }
}
