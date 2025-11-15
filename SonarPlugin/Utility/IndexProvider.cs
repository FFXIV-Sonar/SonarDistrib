using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Data.Rows;
using Sonar.Enums;
using SonarUtils.Text;
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
        private Lazy<KeywordTextIndex<ZoneRow>> _zoneSearchIndex = default!;
        private Lazy<KeywordTextIndex<HuntRow>> _huntSearchIndex = default!;
        private Lazy<KeywordTextIndex<FateRow>> _fateSearchIndex = default!;

        public KeywordTextIndex<ZoneRow> Zones => this._zoneSearchIndex.Value;
        public KeywordTextIndex<HuntRow> Hunts => this._huntSearchIndex.Value;
        public KeywordTextIndex<FateRow> Fates => this._fateSearchIndex.Value;

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
            this._zoneSearchIndex = new(() => KeywordTextIndex.Create(zones, getter: zone => $"{zone.Name} {zone.Region} {zone.Expansion}", maxLength: 4));

            var hunts = Database.Hunts.Values;
            this._huntSearchIndex = new(() => KeywordTextIndex.Create(hunts, getter: hunt => $"{hunt.Name} {string.Join(' ', hunt.GetSpawnZones().Select(z => $"{z.Name} {z.Region}"))} {hunt.Expansion} {hunt.Rank}", maxLength: 4));

            var fates = Database.Fates.Values
                .Where(fate => fate.GetZone()!.IsField);
            this._fateSearchIndex = new(() => KeywordTextIndex.Create(fates, getter: fate => $"{fate.Level} {fate.Name} {fate.AchievementName} {fate.GetZone()!.Name} {fate.GetZone()!.Region} {fate.Expansion}", maxLength: 4));

            // Build indexes in the background
            Task.Run(() => this.Zones);
            Task.Run(() => this.Hunts);
            Task.Run(() => this.Fates);
        }
    }
}
