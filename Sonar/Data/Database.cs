using Sonar.Data.Rows;
using Sonar.Enums;
using System.Collections.Generic;
using Sonar.Data.Details;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Sonar.Threading;
using System.Linq;

namespace Sonar.Data
{
    /// <summary>
    /// Sonar database without the SQL
    /// </summary>
    public static class Database
    {
        private static readonly ResettableLazy<SonarDb> s_db = new(LoadEmbeddedDb) { ShouldDispose = false };
        internal static SonarDb Instance
        {
            get
            {
                return s_db.Value;
            }
            set
            {
                s_db.Value = value;
                ResetLaziness();
            }
        }

        private static void ResetLaziness()
        {
            s_worlds.Reset();
            s_datacenters.Reset();
            s_regions.Reset();
            s_audiences.Reset();
            s_hunts.Reset();
            s_fates.Reset();
            s_maps.Reset();
            s_zones.Reset();
            s_weathers.Reset();
        }

        internal static void Reset()
        {
            s_db.Reset();
            ResetLaziness();
        }

        private static SonarDb LoadEmbeddedDb()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Sonar.Resources.Db.data");
            if (stream is null) throw new FileNotFoundException($"Couldn't read database resources");

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return SonarSerializer.DeserializeData<SonarDb>(bytes);
        }

        /// <summary>
        /// Warning: Slow
        /// </summary>
        public static byte[] ComputeHash() => Instance.ComputeHash();

        /// <summary>
        /// Warning: Slow
        /// </summary>
        public static bool VerifyHash() => Instance.VerifyHash();

        public static SonarDbInfo GetDbInfo() => Instance.GetDbInfo();

        #region Properties
        private static SonarLanguage defaultLanguage = SonarLanguage.English;
        public static SonarLanguage DefaultLanguage
        {
            get => defaultLanguage;
            set
            {
                defaultLanguage = value switch
                {
                    SonarLanguage.Default => SonarLanguage.English,
                    _ => value,
                };
            }
        }
        #endregion

        public static double Timestamp => Instance.Timestamp;
        public static ReadOnlySpan<byte> Hash => Instance.Hash;
        public static string HashString => Instance.HashString;

        #region Dictionaries and Lists
        private static readonly ResettableLazy<IReadOnlyDictionary<uint, WorldRow>> s_worlds = new(() => new ReadOnlyDictionary<uint, WorldRow>(Instance.Worlds)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, WorldRow> Worlds => s_worlds.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, DatacenterRow>> s_datacenters = new(() => new ReadOnlyDictionary<uint, DatacenterRow>(Instance.Datacenters)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, DatacenterRow> Datacenters => s_datacenters.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, RegionRow>> s_regions = new(() => new ReadOnlyDictionary<uint, RegionRow>(Instance.Regions)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, RegionRow> Regions => s_regions.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, AudienceRow>> s_audiences = new(() => new ReadOnlyDictionary<uint, AudienceRow>(Instance.Audiences)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, AudienceRow> Audiences => s_audiences.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, HuntRow>> s_hunts = new(() => new ReadOnlyDictionary<uint, HuntRow>(Instance.Hunts)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, HuntRow> Hunts => s_hunts.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, FateRow>> s_fates = new(() => new ReadOnlyDictionary<uint, FateRow>(Instance.Fates)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, FateRow> Fates => s_fates.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, MapRow>> s_maps = new(() => new ReadOnlyDictionary<uint, MapRow>(Instance.Maps)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, MapRow> Maps => s_maps.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, ZoneRow>> s_zones = new(() => new ReadOnlyDictionary<uint, ZoneRow>(Instance.Zones)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, ZoneRow> Zones => s_zones.Value;

        private static readonly ResettableLazy<IReadOnlyDictionary<uint, WeatherRow>> s_weathers = new(() => new ReadOnlyDictionary<uint, WeatherRow>(Instance.Weathers)) { ShouldDispose = false };
        public static IReadOnlyDictionary<uint, WeatherRow> Weathers => s_weathers.Value;
        #endregion

    }
}
