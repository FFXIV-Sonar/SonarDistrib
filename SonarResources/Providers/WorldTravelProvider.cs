using Sonar.Data.Details;
using Sonar.Data.Rows;
using SonarResources.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using DryIocAttributes;
using System.Diagnostics;

namespace SonarResources.Providers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class WorldTravelProvider
    {
        private readonly HashSet<WorldTravelRow> _travels = [];
        private uint _sequenceId;

        private SonarDb Db { get;} 

        public WorldTravelProvider(SonarDb db, WorldReader _)
        {
            this.Db = db;

            Console.WriteLine("Adding world travel");

            // Self travels (Needed due to the way I'll be checking)
            this.AddSelfTravels();

            // All regions can travel among its datacenters, and in turn among its worlds
            this.AddRegionTravel("JP");
            this.AddRegionTravel("NA");
            this.AddRegionTravel("EU");
            this.AddRegionTravel("OC");
            this.AddRegionTravel("CN");
            this.AddRegionTravel("KR");
            this.AddRegionTravel("TW");

            // JP, NA and EU data centers can travel to OC data centers but not back
            this.AddRegionTravel("JP", "OC", false);
            this.AddRegionTravel("NA", "OC", false);
            this.AddRegionTravel("EU", "OC", false);

            Program.WriteProgressLine($" ({this.Db.WorldTravelData.Count})");
        }

        private void AddWorldTravelCore(uint startId, uint endId)
        {
            var id = ++this._sequenceId;
            var travel = new WorldTravelRow()
            {
                Id = id,
                StartWorldId = startId,
                EndWorldId = endId
            };
            if (this._travels.Add(travel))
            {
                Program.WriteProgress("+");
                this.Db.WorldTravelData.Add(id, travel);
            }
            else
            {
                Program.WriteProgress(".");
            }
        }

        public void AddSelfTravels()
        {
            var worldIds = this.Db.Worlds.Values.Where(world => world.IsPublic).Select(world => world.Id);
            foreach (var worldId in worldIds) this.AddWorldTravel(worldId);
        }

        [SuppressMessage("Major Code Smell", "S2234", Justification = "Bidirectional condition")]
        public void AddWorldTravel(uint startId, uint endId, bool bidirectional = true)
        {
            if (!this.Db.Worlds.TryGetValue(startId, out var start) || !start.IsPublic || !this.Db.Worlds.TryGetValue(endId, out var end) || !end.IsPublic) return;
            this.AddWorldTravelCore(startId, endId);
            if (bidirectional) this.AddWorldTravelCore(endId, startId);
        }

        public void AddWorldTravel(string startWorld, string endWorld, bool bidirectional = true)
        {
            var startId = this.Db.Worlds.Values.First(world => world.IsPublic && world.Name.Equals(startWorld, StringComparison.InvariantCultureIgnoreCase)).Id;
            var endId = this.Db.Worlds.Values.First(world => world.IsPublic && world.Name.Equals(endWorld, StringComparison.InvariantCultureIgnoreCase)).Id;
            this.AddWorldTravel(startId, endId, bidirectional);
        }

        public void AddWorldTravel(uint worldId)
        {
            this.AddWorldTravel(worldId, worldId, false);
        }

        public void AddWorldTravel(string world)
        {
            this.AddWorldTravel(world, world, false);
        }

        public void AddDatacenterTravel(uint startId, uint endId, bool bidirectional = true)
        {
            var startWorldIds = this.Db.Worlds.Values.Where(world => world.IsPublic && world.DatacenterId == startId).Select(world => world.Id);
            var endWorldIds = this.Db.Worlds.Values.Where(world => world.IsPublic && world.DatacenterId == endId).Select(world => world.Id);
            foreach (var startWorldId in startWorldIds)
            {
                foreach (var endWorldId in endWorldIds)
                {
                    this.AddWorldTravel(startWorldId, endWorldId, bidirectional);
                }
            }
        }

        public void AddDatacenterTravel(string startDc, string endDc, bool bidirectional = true)
        {
            var startId = this.Db.Datacenters.Values.First(dc => dc.IsPublic && dc.Name.Equals(startDc, StringComparison.InvariantCultureIgnoreCase)).Id;
            var endId = this.Db.Datacenters.Values.First(dc => dc.IsPublic && dc.Name.Equals(endDc, StringComparison.InvariantCultureIgnoreCase)).Id;
            this.AddDatacenterTravel(startId, endId, bidirectional);
        }

        public void AddDatacenterTravel(uint dcId)
        {
            this.AddDatacenterTravel(dcId, dcId, true);
        }

        public void AddDatacenterTravel(string dc)
        {
            this.AddDatacenterTravel(dc, dc, true);
        }

        public void AddRegionTravel(uint startId, uint endId, bool bidirectional = true)
        {
            var startDcIds = this.Db.Datacenters.Values.Where(dc => dc.IsPublic && dc.RegionId == startId).Select(dc => dc.Id);
            var endDcIds = this.Db.Datacenters.Values.Where(dc => dc.IsPublic && dc.RegionId == endId).Select(dc => dc.Id);
            foreach (var startDcId in startDcIds)
            {
                foreach (var endDcId in endDcIds)
                {
                    this.AddDatacenterTravel(startDcId, endDcId, bidirectional);
                }
            }
        }

        public void AddRegionTravel(string startRegion, string endRegion, bool bidirectional = true)
        {
            var startId = this.Db.Regions.Values.First(region => region.IsPublic && region.Name.Equals(startRegion, StringComparison.InvariantCultureIgnoreCase)).Id;
            var endId = this.Db.Regions.Values.First(region => region.IsPublic && region.Name.Equals(endRegion, StringComparison.InvariantCultureIgnoreCase)).Id;
            this.AddRegionTravel(startId, endId, bidirectional);
        }

        public void AddRegionTravel(uint regionId)
        {
            this.AddRegionTravel(regionId, regionId, true);
        }

        public void AddRegionTravel(string region)
        {
            this.AddRegionTravel(region, region, true);
        }

        public void AddAudienceTravel(uint startId, uint endId, bool bidirectional = true)
        {
            var startRegionIds = this.Db.Regions.Values.Where(region => region.IsPublic && region.AudienceId == startId).Select(region => region.Id);
            var endRegionIds = this.Db.Regions.Values.Where(region => region.IsPublic && region.AudienceId == endId).Select(region => region.Id);
            foreach (var startRegionId in startRegionIds)
            {
                foreach (var endRegionId in endRegionIds)
                {
                    this.AddRegionTravel(startRegionId, endRegionId, bidirectional);
                }
            }
        }

        public void AddAudienceTravel(string startAudience, string endAudience, bool bidirectional = true)
        {
            var startId = this.Db.Audiences.Values.First(audience => audience.IsPublic && audience.Name.Equals(startAudience, StringComparison.InvariantCultureIgnoreCase)).Id;
            var endId = this.Db.Audiences.Values.First(audience => audience.IsPublic && audience.Name.Equals(endAudience, StringComparison.InvariantCultureIgnoreCase)).Id;
            this.AddAudienceTravel(startId, endId, bidirectional);
        }

        public void AddAudienceTravel(uint audienceId)
        {
            this.AddAudienceTravel(audienceId, audienceId, true);
        }

        public void AddAudienceTravel(string audience)
        {
            this.AddAudienceTravel(audience, audience, true);
        }

        public void Benchmark()
        {
            var worldIds = this.Db.Worlds.Values.Where(world => world.IsPublic).Select(world => world.Id);
            foreach (var startId in worldIds)
            {
                foreach (var endId in worldIds)
                {
                    this.AddWorldTravel(startId, endId, false);
                }
            }
        }
    }
}
