using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using ImGuiNET;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Trackers;
using SonarPlugin.Utility;
using System.Linq;
using System.Threading;
using Sonar.Indexes;
using Lumina.Excel.Sheets;
using Dalamud.Interface.Components;
using Dalamud.Interface;
using SonarPlugin.Game;
using System.Diagnostics.CodeAnalysis;

namespace SonarPlugin.GUI
{
    [TransientService]
    public sealed class IndexSelectionWidget
    {
        private static int s_id;
        private readonly int _id;

        private int _audienceIndex;
        private int _regionIndex;
        private int _datacenterIndex;
        private int _worldIndex;
        private int _zoneIndex;
        private uint? _instanceId;
        private string _zoneSearchKeywords = string.Empty; 

        private string[] _audienceArray = default!;
        private string[] _regionArray = default!;
        private string[] _datacenterArray = default!;
        private string[] _worldArray = default!;
        private string[] _zoneArray = default!;

        private AudienceRow?[] _audiences = default!;
        private RegionRow?[] _regions = default!;
        private DatacenterRow?[] _datacenters = default!;
        private WorldRow?[] _worlds = default!;
        private ZoneRow?[] _zones = default!;

        private IndexProvider Index { get; }

        public AudienceRow? Audience => this._audiences[this._audienceIndex];
        public RegionRow? Region => this._regions[this._regionIndex];
        public DatacenterRow? Datacenter => this._datacenters[this._datacenterIndex];
        public WorldRow? World => this._worlds[this._worldIndex];
        public ZoneRow? Zone => this._zones[this._zoneIndex];

        public string AudienceText => this._audienceArray[this._audienceIndex];
        public string RegionText => this._regionArray[this._regionIndex];
        public string DatacenterText => this._datacenterArray[this._datacenterIndex];
        public string WorldText => this._worldArray[this._worldIndex];
        public string ZoneText => this._zoneArray[this._zoneIndex];

        public string IndexKey { get; private set; }

        public IndexSelectionWidget(IndexProvider index)
        {
            this._id = Interlocked.Increment(ref s_id);
            this.Index = index;
            this.LoadAudiences();
            this.LoadRegions();
            this.LoadDatacenters();
            this.LoadWorlds();
            this.LoadZones();
            this.IndexKey = "all";
        }

        public void DrawBreadcrumb()
        {
            if (ImGui.BeginPopup($"Audiences##{this._id}"))
            {
                if (this.DrawAudienceSelector()) ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
            if (ImGui.BeginPopup($"Regions##{this._id}"))
            {
                if (this.DrawRegionSelector()) ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
            if (ImGui.BeginPopup($"Datacenters##{this._id}"))
            {
                if (this.DrawDatacenterSelector()) ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
            if (ImGui.BeginPopup($"Worlds##{this._id}"))
            {
                if (this.DrawWorldSelector()) ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
            if (ImGui.BeginPopup($"Zones##{this._id}"))
            {
                if (this.DrawZoneSelector()) ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
            if (ImGui.BeginPopup($"Instances##{this._id}"))
            {
                if (this.DrawInstanceSelector()) ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }

            do // Hack to use break
            {
                if (ImGui.Button($"{this.AudienceText}##audiencebtn{this._id}")) ImGui.OpenPopup($"Audiences##{this._id}");
                if (this.Audience is null) break;
                ImGui.SameLine(); ImGui.Text($"{(char)SeIconChar.ArrowRight}"); ImGui.SameLine();
                if (ImGui.Button($"{this.RegionText}##regionbtn{this._id}")) ImGui.OpenPopup($"Regions##{this._id}");
                if (this.Region is null) break;
                ImGui.SameLine(); ImGui.Text($"{(char)SeIconChar.ArrowRight}"); ImGui.SameLine();
                if (ImGui.Button($"{this.DatacenterText}##datacenterbtn{this._id}")) ImGui.OpenPopup($"Datacenters##{this._id}");
                if (this.Datacenter is null) break;
                ImGui.SameLine(); ImGui.Text($"{(char)SeIconChar.ArrowRight}"); ImGui.SameLine();
                if (ImGui.Button($"{this.WorldText}##worldbtn{this._id}")) ImGui.OpenPopup($"Worlds##{this._id}");
            } while (false);

            ImGui.SameLine(); ImGui.Text($"{(char)SeIconChar.ArrowRight}"); ImGui.SameLine();
            if (ImGui.Button($"{this.ZoneText}##zonebtn{this._id}")) ImGui.OpenPopup($"Zones##{this._id}");
            ImGui.SameLine();
            if (ImGui.Button($"{(!this._instanceId.HasValue ? $"{(char)SeIconChar.InstanceMerged}" : PayloadExtensions.GenerateInstanceString(this._instanceId.Value, $"{(char)SeIconChar.BoxedNumber0}"))}##instancebtn{this._id}")) ImGui.OpenPopup($"Instances##{this._id}");
        }

        public bool DrawAudienceSelector()
        {
            if (ImGui.ListBox($"##audience{this._id}", ref this._audienceIndex, this._audienceArray, this._audienceArray.Length))
            {
                this._regionIndex = this._datacenterIndex = this._worldIndex = 0;
                this.LoadRegions();
                this.UpdateIndexKey();
                return true;
            }
            return false;
        }

        public bool DrawRegionSelector()
        {
            if (ImGui.ListBox($"##region{this._id}", ref this._regionIndex, this._regionArray, this._regionArray.Length))
            {
                this._datacenterIndex = this._worldIndex = 0;
                this.LoadDatacenters();
                this.UpdateIndexKey();
                return true;
            }
            return false;
        }

        public bool DrawDatacenterSelector()
        {
            if (ImGui.ListBox($"##datacenter{this._id}", ref this._datacenterIndex, this._datacenterArray, this._datacenterArray.Length))
            {
                this._worldIndex = 0;
                this.LoadWorlds();
                this.UpdateIndexKey();
                return true;
            }
            return false;
        }

        public bool DrawWorldSelector()
        {
            if (ImGui.ListBox($"##world{this._id}", ref this._worldIndex, this._worldArray, this._worldArray.Length))
            {
                this.UpdateIndexKey();
                return true;
            }
            return false;
        }

        public bool DrawZoneSelector()
        {
            if (ImGui.InputText($"##zoneSearch{this._id}", ref this._zoneSearchKeywords, 256))
            {
                this._zoneIndex = 0;
                this.LoadZones();
                this.UpdateIndexKey(); // In case user never selects any, this will default to all zones
            }
            if (ImGui.ListBox($"##zone{this._id}", ref this._zoneIndex, this._zoneArray, this._zoneArray.Length))
            {
                this.UpdateIndexKey();
                return true;
            }
            return false;
        }

        public bool DrawInstanceSelector()
        {
            var selected = false;
            if (ImGui.Button($"{(char)SeIconChar.InstanceMerged}##{this._id}")) { this._instanceId = null; selected = true; }
            ImGui.SameLine();
            if (ImGui.Button($"{(char)SeIconChar.BoxedNumber0}##{this._id}")) { this._instanceId = 0; selected = true; }

            if (ImGui.Button($"{(char)SeIconChar.Instance1}##{this._id}")) { this._instanceId = 1; selected = true; }
            ImGui.SameLine();
            if (ImGui.Button($"{(char)SeIconChar.Instance2}##{this._id}")) { this._instanceId = 2; selected = true; }
            ImGui.SameLine();
            if (ImGui.Button($"{(char)SeIconChar.Instance3}##{this._id}")) { this._instanceId = 3; selected = true; }

            // Future proofing should Square Enix decides to add more than 3 instances

            if (ImGui.Button($"{(char)SeIconChar.Instance4}##{this._id}")) { this._instanceId = 4; selected = true; }
            ImGui.SameLine();
            if (ImGui.Button($"{(char)SeIconChar.Instance5}##{this._id}")) { this._instanceId = 5; selected = true; }
            ImGui.SameLine();
            if (ImGui.Button($"{(char)SeIconChar.Instance6}##{this._id}")) { this._instanceId = 6; selected = true; }

            if (ImGui.Button($"{(char)SeIconChar.Instance7}##{this._id}")) { this._instanceId = 7; selected = true; }
            ImGui.SameLine();
            if (ImGui.Button($"{(char)SeIconChar.Instance8}##{this._id}")) { this._instanceId = 8; selected = true; }
            ImGui.SameLine();
            if (ImGui.Button($"{(char)SeIconChar.Instance9}##{this._id}")) { this._instanceId = 9; selected = true; }

            // There are no more than Instance9 in the icon characters...

            if (selected) this.UpdateIndexKey();
            return selected;
        }

        private void LoadAudiences()
        {
            this._audiences = Database.Audiences.Values
                .OrderBy(a => a.Name)
                .Prepend(null)
                .ToArray();
            this._audienceArray = this._audiences.Select(a => a?.Name ?? $"{(char)SeIconChar.CrossWorld}").ToArray();
        }

        private void LoadRegions()
        {
            this._regions = Database.Regions.Values
                .Where(r => r is null || r.AudienceId == this.Audience?.Id)
                .OrderBy(r => r.Name)
                .Prepend(null)
                .ToArray();
            this._regionArray = this._regions.Select(r => r?.Name ?? $"{(char)SeIconChar.CrossWorld}").ToArray();
        }

        private void LoadDatacenters()
        {
            this._datacenters = Database.Datacenters.Values
                .Where(d => d is null || d.RegionId == this.Region?.Id)
                .OrderBy(d => d.Name)
                .Prepend(null)
                .ToArray();
            this._datacenterArray = this._datacenters.Select(d => d?.Name ?? $"{(char)SeIconChar.CrossWorld}").ToArray();
        }

        private void LoadWorlds()
        {
            this._worlds = Database.Worlds.Values
                .Where(w => w is null || w.DatacenterId == this.Datacenter?.Id)
                .OrderBy(w => w.Name)
                .Prepend(null)
                .ToArray();
            this._worldArray = this._worlds.Select(w => w?.Name ?? $"{(char)SeIconChar.CrossWorld}").ToArray();
        }

        private void LoadZones()
        {
            this._zones = this.Index.Zones.Search(this._zoneSearchKeywords)
                .Where(z => z.IsField)
                .OrderBy(z => z.Name.ToString())
                .Prepend(null)
                .ToArray();
            this._zoneArray = this._zones.Select(z => z?.Name.ToString() ?? $"{(char)SeIconChar.Hyadelyn}").ToArray();
        }

        public void UpdateIndexKey()
        {
            var info = new IndexInfo()
            {
                AudienceId = this.Region is null ? this.Audience?.Id : null,
                RegionId = this.Datacenter is null ? this.Region?.Id : null,
                DatacenterId = this.World is null ? this.Datacenter?.Id : null,
                WorldId = this.World?.Id,
                ZoneId = this.Zone?.Id,
                InstanceId = this._instanceId,
            };
            var type = info.DeriveIndexType(true);
            this.IndexKey = info.GetIndexKey(type);
        }
    }
}
