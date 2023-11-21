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
        private static Lazy<SonarDb> s_db = new(LoadEmbeddedDb);
        internal static SonarDb Instance
        {
            get => s_db.Value;
            set => s_db = new(value);
        }

        internal static void Reset()
        {
            s_db = new(LoadEmbeddedDb);
        }

        private static SonarDb LoadEmbeddedDb()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Sonar.Resources.Db.data");
            if (stream is null) throw new FileNotFoundException($"Couldn't read database resources");

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            var db = SonarSerializer.DeserializeData<SonarDb>(bytes);
            db.Freeze();
            return db;
        }

        /// <summary>Warning: Slow</summary>
        public static byte[] ComputeHash() => Instance.ComputeHash();

        /// <summary>Warning: Slow</summary>
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
        public static IReadOnlyDictionary<uint, WorldRow> Worlds => Instance.Worlds.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, DatacenterRow> Datacenters => Instance.Datacenters.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, RegionRow> Regions => Instance.Regions.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, AudienceRow> Audiences => Instance.Audiences.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, HuntRow> Hunts => Instance.Hunts.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, FateRow> Fates => Instance.Fates.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, MapRow> Maps => Instance.Maps.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, ZoneRow> Zones => Instance.Zones.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, WeatherRow> Weathers => Instance.Weathers.ToIReadOnlyDictionaryUnsafe();
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyDictionary<TKey, TValue> ToIReadOnlyDictionaryUnsafe<TKey, TValue>(this IDictionary<TKey, TValue> dict) => Unsafe.As<IReadOnlyDictionary<TKey, TValue>>(dict);

    }
}
