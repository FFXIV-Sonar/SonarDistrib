using System;
using SonarUtils.Text.Internal;

namespace SonarUtils.Text
{
    public static class SearchUtils
    {
        /// <summary>Compares <paramref name="value"/> with <paramref name="comparand"/> and returns a score, with <c>0</c> being an exact match.</summary>
        /// <remarks>Returned distance is normalized from <c>0.0</c> to <c>1.0</c>, adjusted for autocomplete usage.</remarks>
        /// <param name="value"><see cref="string"/> to compare from.</param>
        /// <param name="comparand"><see cref="string"/> to compare with.</param>
        /// <returns>Match distance.</returns>
        public static double Distance(ReadOnlySpan<char> value, ReadOnlySpan<char> comparand)
        {
            // Determine string lengths
            var length1 = value.Length;
            var length2 = comparand.Length;
            if (length1 is 0 || length2 is 0) return 1;

            // Determine min and max lengths
            var minLength = Math.Min(length1, length2);
            var maxLength = Math.Max(length1, length2);

            // Figure out Damerau Levenshtein similarly (opposite of distance)
            // then compute an adjusted distance and normalize into result
            var similarly = maxLength - Damerau.Distance(value, comparand);
            var distance = minLength - similarly;
            var result = (double)distance / minLength;

            // Compute length differences and normalize into result (NOTE: Commented since this is meant to be used for autocompletion)
            //var lengthDiff = maxLength - minLength;
            //result += (double)lengthDiff / maxLength;

            // Compute reversed LCS (distance) then normalize into result
            var reversedLcs = minLength - LongestCommonSubsequence.Length(value, comparand);
            result += (double)reversedLcs / minLength;

            // Prioritize beginning of the string matches
            var startIndex = comparand.IndexOf(value[0]);
            if (startIndex is -1) startIndex = maxLength;
            result += (double)startIndex / maxLength;

            // Return normaized result
            return result / 3;
        }
    }
}
