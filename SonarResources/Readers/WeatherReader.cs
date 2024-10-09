using DryIocAttributes;
using Humanizer;
using Lumina.Excel.Sheets;
using Sonar.Data.Details;
using SonarResources.Lumina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources.Readers
{
    [ExportEx]
    [SingletonReuse]
    public sealed class WeatherReader
    {
        private LuminaManager Luminas { get; }
        private SonarDb Db { get; }
        public WeatherReader(LuminaManager luminas, SonarDb db)
        {
            this.Luminas = luminas;
            this.Db = db;

            Console.WriteLine("Reading all weathers");
            foreach (var entry in this.Luminas.GetAllLuminasEntries())
            {
                Program.WriteProgress(this.Read(entry) ? "+" : ".");
            }
            Program.WriteProgressLine($" ({this.Db.Weathers.Count})");
        }

        private bool Read(LuminaEntry lumina)
        {
            var weatherSheet = lumina.Data.GetExcelSheet<Weather>(lumina.LuminaLanguage);
            if (weatherSheet is null) return false;

            var result = false;
            foreach (var weatherRow in weatherSheet)
            {
                var id = weatherRow.RowId;

                if (!this.Db.Weathers.TryGetValue(id, out var weather))
                {
                    this.Db.Weathers[id] = weather = new()
                    {
                        Id = id,
                        IconId = (uint)weatherRow.Icon
                    };
                }

                if (!weather.Name.ContainsKey(lumina.SonarLanguage))
                {
                    var name = weatherRow.Name.ExtractText();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        weather.Name[lumina.SonarLanguage] = name;
                        result = true;
                    }
                }

                if (!weather.Description.ContainsKey(lumina.SonarLanguage))
                {
                    var description = weatherRow.Description.ExtractText();
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        weather.Description[lumina.SonarLanguage] = description;
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
