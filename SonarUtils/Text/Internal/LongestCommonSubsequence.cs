// Source: https://github.com/feature23/StringSimilarity.NET/blob/f2b7165f6a4653e0f169c2d3717aa25f309d7f3f/src/F23.StringSimilarity/LongestCommonSubsequence.cs
// Modified to use array pools for the matrix, focusing only on the char type and made static 

/*
 * The MIT License
 *
 * Copyright 2016 feature[23]
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
// ReSharper disable SuggestVarOrType_Elsewhere

namespace SonarUtils.Text.Internal //F23.StringSimilarity
{
    /// <summary>
    /// The longest common subsequence (LCS) problem consists in finding the longest
    /// subsequence common to two (or more) sequences. It differs from problems of
    /// finding common substrings: unlike substrings, subsequences are not required
    /// to occupy consecutive positions within the original sequences.
    ///
    /// It is used by the diff utility, by Git for reconciling multiple changes, etc.
    ///
    /// The LCS distance between Strings X (length n) and Y (length m) is n + m - 2
    /// |LCS(X, Y)| min = 0 max = n + m
    ///
    /// LCS distance is equivalent to Levenshtein distance, when only insertion and
    /// deletion is allowed (no substitution), or when the cost of the substitution
    /// is the double of the cost of an insertion or deletion.
    ///
    /// ! This class currently implements the dynamic programming approach, which has
    /// a space requirement O(m * n)!
    /// </summary>
    public static class LongestCommonSubsequence //: IStringDistance, ISpanDistance
    {
        /// <summary>
        ///  Return the length of Longest Common Subsequence (LCS) between strings s1
        ///  and s2.
        /// </summary>
        /// <param name="s1">The first string to compare.</param>
        /// <param name="s2">The second string to compare.</param>
        /// <returns>The length of LCS(s2, s2)</returns>
        /// <exception cref="ArgumentNullException">If s1 or s2 is null.</exception>
        public static int Length(string? s1, string? s2)
            => Length(s1.AsSpan(), s2.AsSpan());

        public static int Length(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
        {
            if (s1.IsEmpty || s2.IsEmpty) return 0;
            if (s1.SequenceEqual(s2)) return s1.Length;

            var c = RentUtils.RentMatrix(s1.Length + 1, s2.Length + 1);
            for (var i = 0; i <= s1.Length; i++)
            {
                c[i][0] = 0;
            }
            for (var j = 0; j <= s2.Length; j++)
            {
                c[0][j] = 0;
            }

            for (var i = 1; i <= s1.Length; i++)
            {
                for (var j = 1; j <= s2.Length; j++)
                {
                    if (s1[i - 1].Equals(s2[j - 1]))
                    {
                        c[i][j] = c[i - 1][j - 1] + 1;
                    }
                    else
                    {
                        c[i][j] = Math.Max(c[i][j - 1], c[i - 1][j]);
                    }
                }
            }

            var result = c[s1.Length][s2.Length];
            RentUtils.ReturnMatrix(c);
            return result;
        }
    }
}
