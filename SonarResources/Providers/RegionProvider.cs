using DryIocAttributes;
using Sonar.Data.Details;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources.Providers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class RegionProvider
    {
        private SonarDb Db { get; }
        public RegionProvider(SonarDb db, AudienceProvider _)
        {
            this.Db = db;

            Console.WriteLine("Adding Regions");
            this.AddRegion(0, "INVALID", 0);
            this.AddRegion(1, "JP", "Global");
            this.AddRegion(2, "NA", "Global");
            this.AddRegion(3, "EU", "Global");
            this.AddRegion(4, "OC", "Global");
            this.AddRegion(5, "Unknown r5", 0);
            this.AddRegion(6, "Unknown r6", 0);
            this.AddRegion(7, "Cloud", "Global");
            this.AddRegion(1000, "CN", "China");
            this.AddRegion(2000, "KR", "Korea");
            Program.WriteProgressLine($" ({this.Db.Regions.Count})");
        }

        private void AddRegion(uint id, string name, string audienceName)
        {
            var audience =
                this.Db.Audiences.Values.FirstOrDefault(audience => audience.Name.Equals(audienceName, StringComparison.InvariantCulture)) ??
                this.Db.Audiences.Values.FirstOrDefault(audience => audience.Name.Equals(audienceName, StringComparison.InvariantCultureIgnoreCase)) ??
                throw new ArgumentException($"Audience {audienceName} not found", nameof(audienceName));

            this.AddRegion(id, name, audience.Id);
        }

        private void AddRegion(uint id, string name, uint audienceId)
        {
            this.Db.Regions.Add(id, new() { Id = id, Name = name, AudienceId = audienceId });
            Program.WriteProgress("+");
        }
    }
}
