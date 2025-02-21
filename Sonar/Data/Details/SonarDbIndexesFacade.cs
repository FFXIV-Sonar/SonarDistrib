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
    public sealed class SonarDbIndexesFacade(SonarDb db)
    {
        private readonly ConcurrentDictionary<SonarLanguage, SonarDbIndexes> _indexes = new();

        private SonarDb Db { get; } = db;

        public SonarDbIndexes this[SonarLanguage language] => this._indexes.GetOrAdd(language, static (language, db) => new(language, db), this.Db);

        /// <summary>Hunts index (using default language)</summary>
        public KeywordTextIndex<HuntRow> Hunts => this.Default.Hunts;

        /// <summary>Fates index (using default language)</summary>
        public KeywordTextIndex<FateRow> Fates => this.Default.Fates;

        /// <summary>Zones index (using default language)</summary>
        public KeywordTextIndex<ZoneRow> Zones => this.Default.Zones;

        /// <summary>Maps index (using default language)</summary>
        public KeywordTextIndex<MapRow> Maps => this.Default.Maps;

        /// <summary>Aetherytes index (using default language)</summary>
        public KeywordTextIndex<AetheryteRow> Aetherytes => this.Default.Aetherytes;

        /// <summary>Weathers index (using default language)</summary>
        public KeywordTextIndex<WeatherRow> Weathers => this.Default.Weathers;

        /// <summary>Audiences index (using default language)</summary>
        public KeywordTextIndex<AudienceRow> Audiences => this.Default.Audiences;

        /// <summary>Regions index (using default language)</summary>
        public KeywordTextIndex<RegionRow> Regions => this.Default.Regions;

        /// <summary>Datacenters index (using default language)</summary>
        public KeywordTextIndex<DatacenterRow> Datacenters => this.Default.Datacenters;

        /// <summary>Worlds index (using default language)</summary>
        public KeywordTextIndex<WorldRow> Worlds => this.Default.Worlds;

        /// <summary>Default indexes</summary>
        public SonarDbIndexes Default => this[Database.ResolveLanguage(SonarLanguage.Default)];
    }
}
