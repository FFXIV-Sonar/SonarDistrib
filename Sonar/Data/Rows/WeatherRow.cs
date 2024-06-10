using MessagePack;
using Sonar.Enums;
using System;

namespace Sonar.Data.Rows
{
    /// <summary>
    /// Weather Data row
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public sealed class WeatherRow : IDataRow
    {
        [Key(0)]
        public uint Id { get; set; }
        [Key(1)]
        public uint IconId { get; set; }
        [Key(2)]
        public LanguageStrings Name { get; set; } = new();
        [Key(3)]
        public LanguageStrings Description { get; set; } = new();
        public override string ToString() => this.Name.ToString();
        public string ToString(SonarLanguage lang) => this.Name.ToString(lang);
    }
}
