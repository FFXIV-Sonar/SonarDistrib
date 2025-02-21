using SonarUtils.Text.Details;
using SonarUtils.Text.Placeholders.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SonarUtils.Text.Placeholders
{
    /// <summary>Formats text replacing placeholders with relevant text.</summary>
    public sealed class PlaceholderFormatter
    {
        private readonly Regex _regex;
        private readonly int _groupId;

        public static readonly PlaceholderFormatter Default = new();

        /// <summary>Initializes a <see cref="PlaceholderFormatter"/> with <see cref="FormatterInternal.DefaultRegex"/>.</summary>
        public PlaceholderFormatter() : this(FormatterInternal.DefaultRegex) { }

        /// <summary>Initializes a <see cref="PlaceholderFormatter"/> with a specified <paramref name="regex"/>.</summary>
        /// <param name="regex">Regular expression to use.</param>
        /// <remarks><paramref name="regex"/> must have a <c>name</c> capturing group.</remarks>
        public PlaceholderFormatter(Regex regex)
        {
            var groupId = regex.GroupNumberFromName("name");
            if (groupId == -1) throw new ArgumentException("Regex must have a captured groupId named \"name\"", nameof(regex));

            this._regex = regex;
            this._groupId = groupId;
        }

        /// <summary>Formats <see cref="str"/> using <paramref name="placeholders"/>.</summary>
        /// <param name="str">String to format.</param>
        /// <param name="placeholders"></param>
        /// <returns>Formatted string.</returns>
        public string Format(string str, IPlaceholderReplacementProvider placeholders)
        {
            var groupId = this._groupId;
            var curPos = 0;
            var builder = new StringBuilder();
            for (var match = this._regex.Match(str); match.Success; match = match.NextMatch())
            {
                builder.Append(str.AsSpan()[curPos..match.Index]);
                curPos = match.Index + match.Length;
                var group = match.Groups[groupId];
                if (group.Success && placeholders.TryGetValue(group.ValueSpan, out var valueSpan)) builder.Append(valueSpan);
                else builder.Append(match.ValueSpan);
            }
            builder.Append(str.AsSpan(curPos));
            return builder.ToString();
        }

        /// <summary>Formats <see cref="str"/> using <paramref name="placeholders"/>.</summary>
        /// <param name="str">String to format.</param>
        /// <param name="placeholders"></param>
        /// <returns>Formatted string.</returns>
        public string Format<T>(string str, IReadOnlyDictionary<string, T> dictionary) => this.Format(str, new DictionaryPlaceholderReplacementProvider<T>(dictionary));
    }
}
