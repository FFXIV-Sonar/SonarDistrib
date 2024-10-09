using Lumina;
using Lumina.Excel.Sheets;
using Sonar.Data.Details;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lumina.Data;
using Sonar.Enums;
using SonarResources.Lumina;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using DryIocAttributes;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class AchievementsReader
    {
        private static readonly Regex s_achievementRegex = new(@"Successfully complete (either )?the FATE (?<fates>.*) with the highest rating possible\.", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private readonly Dictionary<uint, uint> _fateAchievement = new();
        private SonarDb Db { get; }
        private LuminaManager Luminas { get; }

        public AchievementsReader(LuminaManager luminas, SonarDb db, FateReader _)
        {
            this.Db = db;
            this.Luminas = luminas;

            Console.WriteLine("Reading fate achievements");
            foreach (var data in this.Luminas.GetAllDatas())
            {
                Program.WriteProgress(this.ReadFateAchievements(data) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this._fateAchievement.Values.Distinct().Count()})");

            Console.WriteLine("Applying fate achievements");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.ApplyFateAchievements(entry) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this._fateAchievement.Count})");
        }

        private bool ReadFateAchievements(GameData data)
        {
            var achievementSheet = data.GetExcelSheet<Achievement>(Language.English);
            if (achievementSheet is null) return false;

            var result = false;
            foreach (var achievement in achievementSheet)
            {
                var achievementId = achievement.RowId;

                var description = achievement.Description.ExtractText();
                var match = s_achievementRegex.Match(description);
                if (match.Success)
                {
                    var fatesGroup = match.Groups["fates"].Value;
                    var curPos = 0;

                    while (true)
                    {
                        var startPos = fatesGroup.IndexOf("“", curPos, StringComparison.InvariantCulture);
                        if (startPos == -1) break;
                        var endPos = fatesGroup.IndexOf("”", startPos, StringComparison.InvariantCulture);
                        if (endPos == -1) break;
                        curPos = endPos;

                        var fateName = fatesGroup[(startPos + 1)..endPos];
                        if (fateName.EndsWith(",", StringComparison.InvariantCulture)) fateName = fateName[0..^1];

                        var fates = this.Db.Fates.Values
                            .Where(fate => fate.Name[SonarLanguage.English]?.Equals(fateName, StringComparison.InvariantCultureIgnoreCase) ?? false);
                        if (!fates.Any()) continue;// throw new KeyNotFoundException($"FATE {fateName} not found!");

                        foreach (var fate in fates)
                        {
                            result |= this._fateAchievement.TryAdd(fate.Id, achievementId);
                        }
                    }
                }
            }
            return result;
        }

        private bool ApplyFateAchievements(LuminaEntry lumina)
        {
            var achievementSheet = lumina.Data.GetExcelSheet<Achievement>(lumina.LuminaLanguage);
            if (achievementSheet is null) return false;

            var result = false;
            foreach (var fateAchievement in this._fateAchievement)
            {
                var fateId = fateAchievement.Key;
                var fate = this.Db.Fates[fateId];

                var achievementId = fateAchievement.Value;
                var achievement = achievementSheet.GetRowOrDefault(achievementId);
                if (achievement is null) continue;

                fate.HasAchievement = true;
                if (!fate.AchievementName.ContainsKey(lumina.SonarLanguage))
                {
                    result |= fate.AchievementName.TryAdd(lumina.SonarLanguage, achievement.Value.Name.ExtractText());
                }
            }
            return result;
        }
    }
}
