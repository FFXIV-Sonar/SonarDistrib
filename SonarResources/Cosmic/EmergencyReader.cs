using DryIocAttributes;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;
using Sonar.Data.Details;
using Sonar.Data.Rows;
using Sonar.Enums;
using SonarResources.Lgb;
using SonarResources.Lumina;
using SonarUtils.Greek;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarResources.Cosmic
{
    /// <summary></summary>
    [ExportMany]
    [SingletonReuse]
    public sealed class EmergencyReader : IReader
    {
        private readonly Lazy<Task> _workerTask;

        private LuminaManager DataSources { get; }
        private LgbInstancesReader Lgb { get; }
        private SonarDb Db { get; }

        public Task WorkerTask => this._workerTask.Value;

        public EmergencyReader(LuminaManager datas, SonarDb db, LgbInstancesReader lgb)
        {
            this.DataSources = datas;
            this.Lgb = lgb;
            this.Db = db;

            this._workerTask = new(this.WorkerAsync());

            // TODO: Remove this
            this._workerTask.Value.Wait();
        }

        private async Task WorkerAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            var events = (Dictionary<uint, EventRow>)this.Db.Events;
            this.ReadGameData(events, cancellationToken);

            // Sinus Ardorum Red Alert names
            this.ManualNaming(1, 0, "Red Alert: Astromagnetic Storm");
            this.ManualNaming(1, 1, "Red Alert: Astromagnetic Storm");
            this.ManualNaming(1, 2, "Red Alert: Meteor Shower");
            this.ManualNaming(1, 3, "Red Alert: Meteor Shower");
            this.ManualNaming(1, 4, "Red Alert: Sporing Mist");
            this.ManualNaming(1, 5, "Red Alert: Sporing Mist");

            // Phaenna Red Alert names
            this.ManualNaming(2, 0, "Red Alert: Thunderstorm");
            this.ManualNaming(2, 1, "Red Alert: Thunderstorm");
            this.ManualNaming(2, 2, "Red Alert: Annealing Winds");
            this.ManualNaming(2, 3, "Red Alert: Annealing Winds");
            this.ManualNaming(2, 4, "Red Alert: Glass Rain");
            this.ManualNaming(2, 5, "Red Alert: Glass Rain");

            // Oizys Red Alert names
            this.ManualNaming(3, 0, "Red Alert: Gravitational Anomaly");
            this.ManualNaming(3, 1, "Red Alert: Bubble Bloom");
            this.ManualNaming(3, 2, "Red Alert: Gale-force Winds");
            this.ManualNaming(3, 3, "Red Alert: Gale-force Winds");

            // Alpha, Beta, Gamma
            AddGreekSymbols(events, cancellationToken);
        }

        public void ReadGameData(Dictionary<uint, EventRow> events, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Reading Cosmic Emergencies...");
            foreach (var dataSource in this.DataSources.GetAllLuminasEntries())
            {
                var data = dataSource.Data;
                var dataLang = dataSource.LuminaLanguage;
                var sonarLang = dataSource.SonarLanguage;

                var emergencies = data.GetSubrowExcelSheet<WKSEmergencyInfo>(dataLang)!;
                var problems = data.GetExcelSheet<WKSEmergencyProblem>(dataLang)!;
                var warnings = data.GetExcelSheet<WKSEmergencyInfoText>(dataLang)!;
                foreach (var row in emergencies)
                {
                    foreach (var subRow in row)
                    {
                        Debug.Assert(row.RowId == subRow.RowId);
                        cancellationToken.ThrowIfCancellationRequested();

                        var rowId = subRow.RowId;
                        var subRowId = subRow.SubrowId;
                        if (rowId is 0 && subRowId is 0) continue; // Skip 0.0
                        var id = EventUtils.ToId(EventType.CosmicEmergency, rowId, subRowId);

                        ref var ev = ref CollectionsMarshal.GetValueRefOrAddDefault(events, id, out var exists)!;
                        if (!exists)
                        {
                            var coords = subRow.EmergencyProblem
                                .Select(problem => problems.GetRowOrDefault(problem.RowId))
                                .Where(problem => problem is not null)
                                .Select(problem => this.Lgb.GetInstance(problem!.Value.EventLayout))
                                .Where(lgb => lgb is not null)
                                .Select(lgb => new CoordsData() { ZoneId = lgb!.ZoneId, Coords = lgb.Coords, RadiusType = RadiusType.Cylindrical, Scale = lgb.Scale })
                                .ToList();

                            ev = new()
                            {
                                Id = id,
                                GroupId = id,
                                GroupMain = true,

                                Coords = coords,

                                Expansion = ExpansionPack.Endwalker,
                            };
                        }

                        var name = $"Emergency {rowId}.{subRowId}"; // TODO: Retrieve name from sheetSubRow.Name once Lumina.Excel nuget is updated
                        var description = warnings.TryGetRow(subRow.WKSEmergencyWarningText.RowId, out var warning) ? warning.Text.ExtractText() : $"Emergency {rowId}.{subRowId}";
                        ev.Name[sonarLang] = name;
                        ev.Description[sonarLang] = description;
                    }
                }
            }
        }

        public void ManualNaming(uint rowId, uint subRowId, string? name = null, string? description = null)
        {
            var id = EventUtils.ToId(EventType.CosmicEmergency, rowId, subRowId);
            var genericText = $"Emergency {rowId}.{subRowId}";

            if (!this.Db.Events.TryGetValue(id, out var eventRow))
            {
                throw new KeyNotFoundException($"Cosmic {genericText} not found!");
            }

            if (name is not null && eventRow.Name[SonarLanguage.English] == genericText)
            {
                foreach (var (lang, _) in eventRow.Name)
                {
                    eventRow.Name[lang] = name;
                }
            }

            description ??= name;
            if (description is not null && eventRow.Description[SonarLanguage.English] == genericText)
            {
                foreach (var (lang, _) in eventRow.Description)
                {
                    eventRow.Description[lang] = description;
                }
            }
        }

        private static void AddGreekSymbols(Dictionary<uint, EventRow> events, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Adding emergency greek symbols...");
            var emergencies = events.Values.Where(ev => ev.Type is EventType.CosmicEmergency).ToList(); // NOTE: .ToList() needed to prevent enumerating all events multiple times
            foreach (var name in emergencies.Select(emergency => emergency.Name).Distinct())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = emergencies.Count(otherName => otherName.Name[SonarLanguage.English] == name[SonarLanguage.English]);
                if (count > 1)
                {
                    var greekIndex = 0;
                    foreach (var similarName in emergencies.Select(emerg => emerg.Name).Where(otherName => otherName[SonarLanguage.English] == name[SonarLanguage.English]).ToList()) // NOTE: .ToList() needed for logic to work
                    {
                        var symbol = (GreekSymbol)(++greekIndex);
                        foreach (var (lang, str) in similarName.ToList())
                        {
                            similarName[lang] = $"{str} {symbol.LowerChar}";
                        }
                    }
                }
            }
        }
    }
}
