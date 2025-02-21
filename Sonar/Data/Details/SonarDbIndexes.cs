using MessagePack;
using Sonar.Data.Rows;
using Sonar.Enums;
using SonarUtils.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data.Details
{
    public sealed class SonarDbIndexes
    {
        private readonly Lazy<KeywordTextIndex<HuntRow>> _huntsIndex;
        private readonly Lazy<KeywordTextIndex<FateRow>> _fatesIndex;
        private readonly Lazy<KeywordTextIndex<ZoneRow>> _zonesIndex;
        private readonly Lazy<KeywordTextIndex<MapRow>> _mapsIndex;
        private readonly Lazy<KeywordTextIndex<AetheryteRow>> _aetherytesIndex;
        private readonly Lazy<KeywordTextIndex<WeatherRow>> _weathersIndex;
        private readonly Lazy<KeywordTextIndex<AudienceRow>> _audiencesIndex;
        private readonly Lazy<KeywordTextIndex<RegionRow>> _regionsIndex;
        private readonly Lazy<KeywordTextIndex<DatacenterRow>> _datacentersIndex;
        private readonly Lazy<KeywordTextIndex<WorldRow>> _worldsIndex;
        private readonly SonarLanguage _language;

        private SonarDb Db { get; }

        public SonarDbIndexes(SonarLanguage language, SonarDb db)
        {
            this._language = language;
            this.Db = db;

            this._huntsIndex = new(this.HuntsIndexFactory);
            this._fatesIndex = new(this.FatesIndexFactory);
            this._zonesIndex = new(this.ZonesIndexFactory);
            this._mapsIndex = new(this.MapsIndexFactory);
            this._aetherytesIndex = new(this.AetherytesIndexFactory);
            this._weathersIndex = new(this.WeathersIndexFactory);
            this._audiencesIndex = new(this.AudiencesIndexFactory);
            this._regionsIndex = new(this.RegionsIndexFactory);
            this._datacentersIndex = new(this.DatacentersIndexFactory);
            this._worldsIndex = new(this.WorldsIndexFactory);
        }

        /// <summary>Hunts index</summary>
        public KeywordTextIndex<HuntRow> Hunts => this._huntsIndex.Value;

        /// <summary>Fates index</summary>
        public KeywordTextIndex<FateRow> Fates => this._fatesIndex.Value;

        /// <summary>Zones index</summary>
        public KeywordTextIndex<ZoneRow> Zones => this._zonesIndex.Value;

        /// <summary>Maps index</summary>
        public KeywordTextIndex<MapRow> Maps => this._mapsIndex.Value;

        /// <summary>Aetherytes index</summary>
        public KeywordTextIndex<AetheryteRow> Aetherytes => this._aetherytesIndex.Value;

        /// <summary>Weathers index</summary>
        public KeywordTextIndex<WeatherRow> Weathers => this._weathersIndex.Value;

        /// <summary>Audiences index</summary>
        public KeywordTextIndex<AudienceRow> Audiences => this._audiencesIndex.Value;

        /// <summary>Regions index</summary>
        public KeywordTextIndex<RegionRow> Regions => this._regionsIndex.Value;

        /// <summary>Datacenters index</summary>
        public KeywordTextIndex<DatacenterRow> Datacenters => this._datacentersIndex.Value;

        /// <summary>Worlds index</summary>
        public KeywordTextIndex<WorldRow> Worlds => this._worldsIndex.Value;

        private KeywordTextIndex<HuntRow> HuntsIndexFactory() => KeywordTextIndex.Create(this.Db.Hunts.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name[this._language].ToString())), getter: item => item.Name[this._language].ToString());
        private KeywordTextIndex<FateRow> FatesIndexFactory() => KeywordTextIndex.Create(this.Db.Fates.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name[this._language].ToString())), getter: item => item.Name[this._language].ToString());
        private KeywordTextIndex<ZoneRow> ZonesIndexFactory() => KeywordTextIndex.Create(this.Db.Zones.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name[this._language].ToString())), getter: item => item.Name[this._language].ToString());
        private KeywordTextIndex<MapRow> MapsIndexFactory() => KeywordTextIndex.Create(this.Db.Maps.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name[this._language].ToString())), getter: item => item.Name[this._language].ToString());
        private KeywordTextIndex<AetheryteRow> AetherytesIndexFactory() => KeywordTextIndex.Create(this.Db.Aetherytes.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name[this._language].ToString())), getter: item => item.Name[this._language].ToString());
        private KeywordTextIndex<WeatherRow> WeathersIndexFactory() => KeywordTextIndex.Create(this.Db.Weathers.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name[this._language].ToString())), getter: item => item.Name[this._language].ToString());
        private KeywordTextIndex<AudienceRow> AudiencesIndexFactory() => KeywordTextIndex.Create(this.Db.Audiences.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name.ToString())), getter: item => item.Name.ToString());
        private KeywordTextIndex<RegionRow> RegionsIndexFactory() => KeywordTextIndex.Create(this.Db.Regions.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name.ToString())), getter: item => item.Name.ToString());
        private KeywordTextIndex<DatacenterRow> DatacentersIndexFactory() => KeywordTextIndex.Create(this.Db.Datacenters.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name.ToString())), getter: item => item.Name.ToString());
        private KeywordTextIndex<WorldRow> WorldsIndexFactory() => KeywordTextIndex.Create(this.Db.Worlds.Values.Where(item => !string.IsNullOrWhiteSpace(item.Name.ToString())), getter: item => item.Name.ToString());
    }
}