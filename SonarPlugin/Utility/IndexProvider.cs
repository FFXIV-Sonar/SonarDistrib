using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Data.Rows;
using Sonar.Enums;
using Sonar.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Utility
{
    [SingletonService]
    public sealed class IndexProvider
    {
        private object? _checkObj;
        private SonarLanguage _lastLanguage;
        private Lazy<FullTextIndex<ZoneRow>> _zoneSearchIndex = default!;
        private Lazy<FullTextIndex<HuntRow>> _huntSearchIndex = default!;
        private Lazy<FullTextIndex<FateRow>> _fateSearchIndex = default!;

        public FullTextIndex<ZoneRow> Zones => this._zoneSearchIndex.Value;
        public FullTextIndex<HuntRow> Hunts => this._huntSearchIndex.Value;
        public FullTextIndex<FateRow> Fates => this._fateSearchIndex.Value;

        public IndexProvider()
        {
            this.ResetIfDbChanged();
        }

        private void ResetIfDbChanged()
        {
            if (!ReferenceEquals(Database.Worlds, this._checkObj) || Database.DefaultLanguage != this._lastLanguage)
            {
                this._checkObj = Database.Worlds;
                this._lastLanguage = Database.DefaultLanguage;
                this.ResetIndexes();
            }
        }

        private void ResetIndexes()
        {
            var zones = Database.Zones.Values
                .Where(zone => zone.IsField);
            this._zoneSearchIndex = new(() => FullTextIndex.Create(zones, getter: zone => $"{zone.Name} {zone.Region}"));

            var hunts = Database.Hunts.Values;
            this._huntSearchIndex = new(() => FullTextIndex.Create(hunts, getter: hunt => $"{hunt.Name} {string.Join(' ', hunt.GetSpawnZones().Select(z => $"{z.Name} {z.Region}"))}"));

            var fates = Database.Fates.Values
                .Where(fate => fate.GetZone()!.IsField);
            this._fateSearchIndex = new(() => FullTextIndex.Create(fates, getter: fate => $"{fate.Level} {fate.Name} {fate.AchievementName} {fate.GetZone()!.Name} {fate.GetZone()!.Region}"));

            // Build indexes in the background
            Task.Run(() => this.Zones);
            //Task.Run(() => this.Hunts);
            //Task.Run(() => this.Fates);
        }
    }
}
