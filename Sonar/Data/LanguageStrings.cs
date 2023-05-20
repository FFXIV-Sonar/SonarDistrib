using System;
using System.Collections.Generic;
using System.Linq;
using Sonar.Enums;
using MessagePack;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MessagePack.Formatters;

namespace Sonar.Data
{
    [Serializable]
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Intentional")]
    [MessagePackObject]
    [MessagePackFormatter(typeof(LanguageStringsFormatter))]
    public sealed class LanguageStrings : IDictionary<SonarLanguage, string>
    {
        private readonly Dictionary<SonarLanguage, string> strings = new();
        private static readonly HashSet<string> s_interner = new();

        /// <summary>
        /// Resolve which language to return (in case not all languages are supported)
        /// </summary>
        /// <param name="lang"></param>
        /// <returns>Resolved language, if any</returns>
        public SonarLanguage? ResolveLanguage(SonarLanguage lang = SonarLanguage.Default)
        {
            // Fail fast if no strings are stored
            if (this.strings.Count == 0) return null;

            // If default take it from Database.Default
            if (lang == SonarLanguage.Default) lang = Database.DefaultLanguage;

            // If a string for the requested language doesn't exist try the default language
            if (!this.strings.ContainsKey(lang)) lang = Database.DefaultLanguage;

            // If a string for the default language doesn't exist fall back to English
            if (!this.strings.ContainsKey(lang)) lang = SonarLanguage.English;

            // If a string doesn't exist use whichever key is found as a last resort
            if (!this.strings.ContainsKey(lang)) lang = this.strings.Keys.First();

            // Return the resolved language
            return lang;
        }

        private static void ThrowIfInvalidLanguage(SonarLanguage lang)
        {
            if (!Enum.IsDefined(lang)) throw new ArgumentException("Invalid Language");
        }

        [IgnoreMember]
        public ICollection<SonarLanguage> Keys => this.strings.Keys;

        [IgnoreMember]
        public ICollection<string> Values => this.strings.Values;

        [IgnoreMember]
        public int Count => this.strings.Count;

        [IgnoreMember]
        public bool IsReadOnly => false;

        /// <summary>
        /// Return a string of the specified language
        /// </summary>
        /// <param name="lang">Language of the string</param>
        /// <returns>String</returns>
        public string ToString(SonarLanguage lang)
        {
            // Resolve the language to return
            var resLang = this.ResolveLanguage(lang);

            // If resolving failed, return null
            if (!resLang.HasValue) return string.Empty;

            // Return the language string for the specified language
            return this.strings[resLang.Value];
        }

        /// <summary>
        /// Return a string of the specified language
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => this.ToString(Database.DefaultLanguage);

        /// <summary>
        /// ToString() array style accessor alternative.
        /// </summary>
        /// <param name="lang"></param>
        /// <returns>String</returns>
        [IgnoreMember]
        public string this[SonarLanguage lang]
        {
            get => this.ToString(lang);
            set
            {
                // Avoid invalid languages
                ThrowIfInvalidLanguage(lang);

                // Use Database.DefaultLanguage
                if (lang == SonarLanguage.Default) lang = Database.DefaultLanguage;

                // Just in case someone decides to use the Default language, we're english developers
                if (lang == SonarLanguage.Default) lang = SonarLanguage.English;

                // If the value is null, remove the string and return
                if (string.IsNullOrEmpty(value))
                {
                    this.strings.Remove(lang);
                    return;
                }

                // Sets the language string
                lock (s_interner) this.strings[lang] = s_interner.Intern(value);
            }
        }

        public bool ContainsKey(SonarLanguage key) => this.strings.ContainsKey(key);

        public void Add(SonarLanguage key, string value) => this[key] = value;

        public bool Remove(SonarLanguage key) => this.strings.Remove(key);

        public bool TryGetValue(SonarLanguage key, [MaybeNullWhen(false)] out string value) => this.strings.TryGetValue(key, out value);

        public void Add(KeyValuePair<SonarLanguage, string> item) => this[item.Key] = item.Value;

        public void Clear() => this.strings.Clear();

        public bool Contains(KeyValuePair<SonarLanguage, string> item) => this.strings.Contains(item);

        public void CopyTo(KeyValuePair<SonarLanguage, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<SonarLanguage, string>>)this.strings).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<SonarLanguage, string> item) => ((ICollection<KeyValuePair<SonarLanguage, string>>)this.strings).Remove(item);

        public IEnumerator<KeyValuePair<SonarLanguage, string>> GetEnumerator() => this.strings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.strings.GetEnumerator();

        public class LanguageStringsFormatter : IMessagePackFormatter<LanguageStrings>
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "MessagePack expectation")]
            public LanguageStrings Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil()) return null!;
                options.Security.DepthStep(ref reader);

                var ret = new LanguageStrings();
                var keyFormatter = options.Resolver.GetFormatterWithVerify<SonarLanguage>();
                var valueFormatter = options.Resolver.GetFormatterWithVerify<string>();

                int count = reader.ReadMapHeader();
                for (int i = 0; i < count; i++)
                {
                    var key = keyFormatter.Deserialize(ref reader, options);
                    var value = valueFormatter.Deserialize(ref reader, options);
                    ret[key] = value;
                }

                reader.Depth--;
                return ret;
            }

            public void Serialize(ref MessagePackWriter writer, LanguageStrings? value, MessagePackSerializerOptions options)
            {
                if (value is null)
                {
                    writer.WriteNil();
                    return;
                }

                var keyFormatter = options.Resolver.GetFormatterWithVerify<SonarLanguage>();
                var valueFormatter = options.Resolver.GetFormatterWithVerify<string>();

                writer.WriteMapHeader(value.Count);
                foreach (var item in value)
                {
                    keyFormatter.Serialize(ref writer, item.Key, options);
                    valueFormatter.Serialize(ref writer, item.Value, options);
                }
            }
        }
    }
}
