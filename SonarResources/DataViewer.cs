using Sonar.Data.Details;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources
{
    public static class DataViewer
    {

        public static void ShowWorlds(SonarDb db)
        {
            foreach (var audience in db.Audiences.Values.Where(audience => audience.IsPublic))
            {
                var audienceName = $"{audience.Name} Audience (ID: {audience.Id})";
                Console.WriteLine($"/{new string('=', audienceName.Length)}\\");
                Console.WriteLine($"|{audienceName}|");
                Console.WriteLine($"\\{new string('=', audienceName.Length)}/");
                foreach (var region in db.Regions.Values.Where(region => region.IsPublic && region.AudienceId == audience.Id))
                {
                    Console.WriteLine($"{region.Name} Region (ID: {region.Id})");
                    foreach (var datacenter in db.Datacenters.Values.Where(datacenter => datacenter.IsPublic && datacenter.RegionId == region.Id))
                    {
                        Console.Write($" - {datacenter.Name} ({datacenter.Id}): ");
                        bool first = true;
                        foreach (var world in db.Worlds.Values.Where(world => world.IsPublic && world.DatacenterId == datacenter.Id))
                        {
                            if (!first)
                            {
                                Console.Write(", ");
                            }
                            first = false;
                            Console.Write($"{world} ({world.Id})");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}
