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
    public sealed class AudienceProvider
    {
        private SonarDb Db { get; }
        public AudienceProvider(SonarDb db)
        {
            this.Db = db;

            Console.WriteLine("Adding Audiences");
            this.AddAudience(0, "INVALID");
            this.AddAudience(1, "Global");
            this.AddAudience(1000, "China");
            this.AddAudience(2000, "Korea");
            this.AddAudience(4000, "Taiwan");
            Program.WriteProgressLine($" ({this.Db.Audiences.Count})");
        }

        private void AddAudience(uint id, string name)
        {
            this.Db.Audiences.Add(id, new() { Id = id, Name = name });
            Program.WriteProgress("+");
        }
    }
}
