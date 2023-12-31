using DryIocAttributes;
using Lumina.Excel.GeneratedSheets2;
using Sonar.Data.Details;
using SonarResources.Lgb;
using SonarResources.Lumina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sonar.Enums;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class FateReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }
        private LgbInstancesReader Lgb { get; }

        public FateReader(LuminaManager luminas, SonarDb db, LgbInstancesReader lgb, ZoneReader _)
        {
            this.Luminas = luminas;
            this.Db = db;
            this.Lgb = lgb;

            Console.WriteLine("Reading all fates");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.Read(entry) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this.Db.Fates.Count})");

            Console.WriteLine("Grouping fates");
            this.GroupFates();
        }

        public bool Read(LuminaEntry lumina)
        {
            var fateSheet = lumina.Data.GetExcelSheet<global::Lumina.Excel.GeneratedSheets2.Fate>(lumina.LuminaLanguage)?
                .Where(fate => fate.Location != 0);
            if (fateSheet is null) return false;

            var result = false;
            foreach (var fateRow in fateSheet)
            {
                var id = fateRow.RowId;

                var lgbId = fateRow.Location;
                var lgbInstance = this.Lgb.GetInstance(lgbId) ?? throw new KeyNotFoundException($"LGB Instance not found: {lgbId}");

                var zone = this.Db.Zones[lgbInstance.ZoneId];
                if (!zone.IsField) continue;

                if (!this.Db.Fates.TryGetValue(id, out var fate))
                {
                    this.Db.Fates[id] = fate = new()
                    {
                        Id = id,
                        IconId = fateRow.MapIcon,
                        ObjectiveIconId = fateRow.ObjectiveIcon.First(), // Placeholder for now (TODO)
                        Level = fateRow.ClassJobLevel,
                        ZoneId = zone.Id,
                        Coordinates = lgbInstance.Coords,
                        Scale = lgbInstance.Scale,
                        IsHidden = fateRow.Unknown8 == 1, // || f.ScreenImageAccept.Row == 37, NOTE: Unknown8 = parser.ReadOffset< byte >( 133 ); <-- Look for offset 133 if changed. Used to be Unknown24.
                        Expansion = zone.Expansion,
                    };
                    zone.FateIds.Add(fate.Id);
                }

                if (!fate.Name.ContainsKey(lumina.SonarLanguage))
                {
                    var name = fateRow.Name.ToTextString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        fate.Name[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }

                if (!fate.Description.ContainsKey(lumina.SonarLanguage))
                {
                    var description = fateRow.Description.ToTextString();
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        fate.Description[lumina.SonarLanguage] = description;
                        result = true;
                    }
                }

                if (!fate.Objective.ContainsKey(lumina.SonarLanguage))
                {
                    var objective = fateRow.Objective.ToTextString();
                    if (!string.IsNullOrWhiteSpace(objective))
                    {
                        fate.Objective[lumina.SonarLanguage] = objective;
                        result = true;
                    }
                }
            }
            return result;
        }

        private void GroupFates()
        {
            var groups = new Dictionary<string, List<uint>>();
            foreach (var fate in this.Db.Fates.Values)
            {
                var fateName = fate.Name[SonarLanguage.English];
                if (!groups.TryGetValue(fateName, out var group))
                {
                    groups[fateName] = group = new();
                }
                group.Add(fate.Id);
            }

            foreach (var fate in this.Db.Fates.Values)
            {
                var fateName = fate.Name[SonarLanguage.English];
                var group = groups[fateName];
                group.Sort();

                var groupId = group.First();

                fate.GroupId = groupId;
                fate.GroupFateIds = group.ToHashSet();
                fate.GroupMain = groupId == fate.Id;
            }
        }
    }
}
