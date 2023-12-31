using System;
using System.Collections.Generic;
using System.Linq;
using Sonar.Enums;
using MessagePack;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MessagePack.Formatters;
using SonarUtils;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Sonar.Data
{
    [Serializable]
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Intentional")]
    [MessagePackObject]
    [MessagePackFormatter(typeof(LanguageStringsFormatter))]
    public sealed class LanguageStrings : IDictionary<SonarLanguage, string>
    {
        private readonly IDictionary<SonarLanguage, string> _strings = new Dictionary<SonarLanguage, string>();

        /// <summary>
        /// Resolve which language to return (in case not all languages are supported)
        /// </summary>
        /// <param name="lang"></param>
        /// <returns>Resolved language, if any</returns>
        public SonarLanguage? ResolveLanguage(SonarLanguage lang = SonarLanguage.Default)
        {
            // Fail fast if no strings are stored
            if (this._strings.Count == 0) return null;

            // If default take it from Database.Default
            if (lang == SonarLanguage.Default) lang = Database.DefaultLanguage;

            // If a string for the requested language doesn't exist try the default language
            if (!this._strings.ContainsKey(lang)) lang = Database.DefaultLanguage;

            // If a string for the default language doesn't exist fall back to English
            if (!this._strings.ContainsKey(lang)) lang = SonarLanguage.English;

            // If a string doesn't exist use whichever key is found as a last resort
            if (!this._strings.ContainsKey(lang)) lang = this._strings.Keys.First();

            // Return the resolved language
            return lang;
        }

        private static void ThrowIfInvalidLanguage(SonarLanguage lang)
        {
            if (!Enum.IsDefined(lang)) throw new ArgumentException("Invalid Language");
        }

        [IgnoreMember]
        public ICollection<SonarLanguage> Keys => this._strings.Keys;

        [IgnoreMember]
        public ICollection<string> Values => this._strings.Values;

        [IgnoreMember]
        public int Count => this._strings.Count;

        [IgnoreMember]
        public bool IsReadOnly => this._strings.IsReadOnly;

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
            return this._strings[resLang.Value];
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
                    this._strings.Remove(lang);
                    return;
                }

                // Sets the language string
                this._strings[lang] = StringUtils.Intern(value);
            }
        }

        public bool ContainsKey(SonarLanguage key) => this._strings.ContainsKey(key);

        public void Add(SonarLanguage key, string value) => this[key] = value;

        public bool Remove(SonarLanguage key) => this._strings.Remove(key);

        public bool TryGetValue(SonarLanguage key, [MaybeNullWhen(false)] out string value) => this._strings.TryGetValue(key, out value);

        public void Add(KeyValuePair<SonarLanguage, string> item) => this[item.Key] = item.Value;

        public void Clear() => this._strings.Clear();

        public bool Contains(KeyValuePair<SonarLanguage, string> item) => this._strings.Contains(item);

        public void CopyTo(KeyValuePair<SonarLanguage, string>[] array, int arrayIndex) => this._strings.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<SonarLanguage, string> item) => this._strings.Remove(item);

        public IEnumerator<KeyValuePair<SonarLanguage, string>> GetEnumerator() => this._strings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this._strings.GetEnumerator();

        public class LanguageStringsFormatter : IMessagePackFormatter<LanguageStrings?>
        {
            //[SuppressMessage("Major Code Smell", "S1168", Justification = "MessagePack expectation")]
            public LanguageStrings? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil()) return null;
                options.Security.DepthStep(ref reader);

                var ret = new LanguageStrings();
                var keyFormatter = options.Resolver.GetFormatterWithVerify<SonarLanguage>();
                var valueFormatter = options.Resolver.GetFormatterWithVerify<string>();

                var count = reader.ReadMapHeader();
                for (var i = 0; i < count; i++)
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
