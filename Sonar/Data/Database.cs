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
using System.Diagnostics.CodeAnalysis;
using Sonar.Extensions;

namespace Sonar.Data
{
    /// <summary>
    /// Sonar database without the SQL
    /// </summary>
    public static class Database
    {
        private static readonly SonarLanguage[] s_languages = Enum.GetValues<SonarLanguage>().Where(language => language is not SonarLanguage.Default).ToArray();
        private static Lazy<SonarDb> s_db = new(LoadEmbeddedDb);
        private static SonarLanguage s_defaultLanguage = SonarLanguage.English;

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
            using var stream = assembly.GetManifestResourceStream("Sonar.Resources.Db.data") ?? throw new FileNotFoundException($"Couldn't read database resources");

            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes, 0, bytes.Length);
            var db = SonarSerializer.DeserializeData<SonarDb>(bytes);
            db.Freeze();

            DbLoaded?.SafeInvoke(db);
            return db;
        }

        /// <summary>Invoked whenever a new <see cref="SonarDb"/> instance is loaded to <see cref="Instance"/>.</summary>
        /// <remarks>WARNING: Exceptions swallowed!</remarks>
        public static event Action<SonarDb>? DbLoaded;

        /// <summary>Warning: Slow</summary>
        public static byte[] ComputeHash() => Instance.ComputeHash();

        /// <summary>Warning: Slow</summary>
        public static bool VerifyHash() => Instance.VerifyHash();

        public static SonarDbInfo GetDbInfo() => Instance.GetDbInfo();

        #region Language
        public static SonarLanguage DefaultLanguage
        {
            get => s_defaultLanguage;
            set => s_defaultLanguage = ResolveLanguage(value);
        }

        public static IEnumerable<SonarLanguage> Languages => s_languages;

        /// <summary>Resolve which language to return (in case not all languages are supported)</summary>
        /// <returns>Resolved language</returns>
        public static SonarLanguage ResolveLanguage(SonarLanguage language, IEnumerable<SonarLanguage> languages, bool throwOnInvalid = true)
        {
            if (!languages.Any())
            {
                AG.ThrowHelper.ThrowIf(throwOnInvalid, static () => new ArgumentException("No valid languages"));
                languages = s_languages;
            }

            if (!languages.All(static language => s_languages.Contains(language)))
            {
                AG.ThrowHelper.ThrowIf(throwOnInvalid, static (languages) => new ArgumentException($"Invalid languages detected: {string.Join(", ", languages.Where(language => !s_languages.Contains(language)))}"), languages);
                languages = languages.Where(s_languages.Contains); // Remove all invalid languages
                return ResolveLanguage(language, languages, throwOnInvalid); // ASSERT: Only a single recursion will happen
            }

            if (!Enum.IsDefined(language))
            {
                AG.ThrowHelper.ThrowIf(throwOnInvalid, static () => new ArgumentException("Invalid Language: {language}"));
                language = DefaultLanguage;
            }

            if (language is SonarLanguage.Default) language = SonarLanguage.English;
            if (!languages.Contains(language)) return languages.First();
            return language;
        }

        public static SonarLanguage ResolveLanguage(SonarLanguage language, bool throwOnInvalid = true) => ResolveLanguage(language, s_languages, throwOnInvalid);
        #endregion

        public static double Timestamp => Instance.Timestamp;
        public static ReadOnlySpan<byte> Hash => Instance.Hash;
        public static string HashString => Instance.HashString;
        public static SonarDbIndexesFacade Indexes => Instance.Indexes;

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
        public static IReadOnlyDictionary<uint, AetheryteRow> Aetherytes => Instance.Aetherytes.ToIReadOnlyDictionaryUnsafe();
        public static IReadOnlyDictionary<uint, WorldTravelRow> WorldTravelData => Instance.WorldTravelData.ToIReadOnlyDictionaryUnsafe();
        #endregion

        #region Utility Helpers
        public static WorldTravelHelper WorldTravel => Instance.WorldTravel;
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyDictionary<TKey, TValue> ToIReadOnlyDictionaryUnsafe<TKey, TValue>(this IDictionary<TKey, TValue> dict) => Unsafe.As<IReadOnlyDictionary<TKey, TValue>>(dict);

    }
}
