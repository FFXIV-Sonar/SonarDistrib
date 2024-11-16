using DryIocAttributes;
using Lumina.Excel.Sheets;
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
using System.Diagnostics;
using Lumina.Data;
using System.Runtime.InteropServices;
using Sonar.Data.Rows;
using SonarUtils;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            Console.WriteLine("Reading Fates (Stage 1)");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.Stage1(entry) ? "+" : ".");
            }

            Console.WriteLine("Reading Fates (Stage 2)");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.Stage2(entry) ? "+" : ".");
            }

            Console.WriteLine("Cleaning up fates");
            this.CheckAndCleanupFates();

            Program.WriteProgressLine($" ({this.Db.Fates.Count})");

            Console.WriteLine("Grouping fates");
            this.GroupFates();
        }

        public bool Stage1(LuminaEntry lumina)
        {
            var fateSheet = lumina.Data.GetExcelSheet<global::Lumina.Excel.Sheets.Fate>(lumina.LuminaLanguage);
            if (fateSheet is null) return false;

            var fates = (Dictionary<uint, FateRow>)this.Db.Fates;
            foreach (var fateRow in fateSheet)
            {
                var id = fateRow.RowId;
                ref var fate = ref CollectionsMarshal.GetValueRefOrAddDefault(fates, id, out var exists);
                if (!exists)
                {
                    fate = new()
                    {
                        Id = id,
                        IconId = fateRow.MapIcon,
                        ObjectiveIconId = fateRow.ObjectiveIcon[0].Icon, // Placeholder for now (TODO)
                        Level = fateRow.ClassJobLevel,
                        IsHidden = fateRow.Unknown8, // || f.ScreenImageAccept.Row == 37, NOTE: Unknown8 = parser.ReadOffset< byte >( 133 ); <-- Look for offset 133 if changed. Used to be Unknown24.
                    };

                    var lgbId = fateRow.Location;
                    var lgbInstance = this.Lgb.GetInstance(lgbId);
                    if (lgbInstance is not null)
                    {
                        fate.LgbId = lgbId;
                        fate.ZoneId = lgbInstance.ZoneId;
                        fate.Coordinates = lgbInstance.Coords;
                        fate.Scale = lgbInstance.Scale;
                        fate.Expansion = this.Db.Zones.GetValueOrDefault(lgbInstance.ZoneId)?.Expansion ?? ExpansionPack.Unknown;
                    }
                }
            }
            return true;
        }

        public bool Stage2(LuminaEntry lumina)
        {
            var fateSheet = lumina.Data.GetExcelSheet<global::Lumina.Excel.Sheets.Fate>(lumina.LuminaLanguage)?
                .Where(fate => fate.Location != 0);
            if (fateSheet is null) return false;

            var fates = (Dictionary<uint, FateRow>)this.Db.Fates;
            foreach (var fateRow in fateSheet)
            {
                if (!fates.TryGetValue(fateRow.RowId, out var fate)) throw new Exception($"Fate ID {fate.Id} doesn't exist!"); // This should never happen
                if (!fate.Name.ContainsKey(lumina.SonarLanguage))
                {
                    var name = fateRow.Name.ExtractTextWithSheets(lumina.Data, lumina.LuminaLanguage);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        fate.Name[lumina.SonarLanguage] = name;
                    }
                }

                if (!fate.Description.ContainsKey(lumina.SonarLanguage))
                {
                    var description = fateRow.Description.ExtractTextWithSheets(lumina.Data, lumina.LuminaLanguage);
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        fate.Description[lumina.SonarLanguage] = description;
                    }
                }

                if (!fate.Objective.ContainsKey(lumina.SonarLanguage))
                {
                    var objective = fateRow.Objective.ExtractTextWithSheets(lumina.Data, lumina.LuminaLanguage);
                    if (!string.IsNullOrWhiteSpace(objective))
                    {
                        fate.Objective[lumina.SonarLanguage] = objective;
                    }
                }
            }
            return true;
        }

        public bool CheckAndCleanupFates()
        {
            var toRemove = new List<uint>();
            foreach (var fate in this.Db.Fates.Values)
            {
                var id = fate.Id;
                if (fate.LgbId == 0 || fate.Name.Count == 0 || !this.Db.Zones.TryGetValue(fate.ZoneId, out var zone) || !zone.IsField)
                {
                    toRemove.Add(id);
                    continue;
                }

                if (fate.ZoneId == 0) Console.WriteLine($"Fate ID {id} has no zone: {fate.Name}");
                if (!this.Db.Zones.ContainsKey(fate.ZoneId)) Console.WriteLine($"Fate ID {id} has invalid zone ID {fate.ZoneId}: {fate.Name}");

                if (fate.Name.Count == 0) Console.WriteLine($"Fate ID {id} has no name");
                if (fate.Description.Count == 0) Console.WriteLine($"Fate ID {id} has no description");
            }
            this.Db.Fates.RemoveRange(toRemove);
            return true;
        }

        private void GroupFates()
        {
            var groups = new Dictionary<string, List<uint>>();
            foreach (var fate in this.Db.Fates.Values)
            {
                var fateName = fate.Name[SonarLanguage.English];
                if (!groups.TryGetValue(fateName, out var group))
                {
                    groups[fateName] = group = [];
                }
                group.Add(fate.Id);
            }

            foreach (var fate in this.Db.Fates.Values)
            {
                var fateName = fate.Name[SonarLanguage.English];
                var group = groups[fateName];
                group.Sort();

                var groupId = group[0];

                fate.GroupId = groupId;
                fate.GroupFateIds = [.. group];
                fate.GroupMain = groupId == fate.Id;
            }
        }
    }
}
