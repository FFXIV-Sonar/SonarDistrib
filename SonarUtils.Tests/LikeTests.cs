using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

namespace SonarUtils.Tests
{
    public static class LikeTests
    {
        [Theory]
        [MemberData(nameof(LikeString_TestData))]
        public static void LikeString(string? source, string? pattern, bool binaryResult, bool textResult)
        {
            Assert.Equal(binaryResult, source.Like(pattern, CompareMethod.Binary));
            Assert.Equal(textResult, source.Like(pattern, CompareMethod.Text));
        }

        [Theory]
        [MemberData(nameof(LikeObject_TestData))]
        [MemberData(nameof(LikeString_TestData))]
        public static void LikeObject(object? source, object? pattern, bool expectedBinaryCompare, bool expectedTextCompare)
        {
            Assert.Equal(expectedBinaryCompare, source.Like(pattern, CompareMethod.Binary));
            Assert.Equal(expectedTextCompare, source.Like(pattern, CompareMethod.Text));
        }

        public static IEnumerable<object?[]> LikeObject_TestData()
        {
            // https://github.com/dotnet/runtime/blob/686d0748a5d2e4f078fc7247e04bf34b18435746/src/libraries/Microsoft.VisualBasic.Core/tests/LikeOperatorTests.cs
            yield return new object?[] { null, new[] { '*' }, true, true };
            yield return new object?[] { Array.Empty<char>(), null, true, true };
            yield return new object?[] { "a3", new[] { 'A', '#' }, false, true };
            yield return new object?[] { new[] { 'A', '3' }, "a#", false, true };
        }

        public static IEnumerable<object?[]> LikeString_TestData()
        {
            // https://learn.microsoft.com/en-us/dotnet/visual-basic/language-reference/operators/like-operator
            yield return new object?[] { "F", "F", true, true };
            yield return new object?[] { "F", "f", false, true };
            yield return new object?[] { "F", "FFF", false, false };
            yield return new object?[] { "aBBBa", "a*a", true, true };
            yield return new object?[] { "F", "[A-Z]", true, true };
            yield return new object?[] { "F", "[!A-Z]", false, false };
            yield return new object?[] { "a2a", "a#a", true, true };
            yield return new object?[] { "aM5b", "a[L-P]#[!c-e]", true, true };
            yield return new object?[] { "BAT123khg", "B?T*", true, true };
            yield return new object?[] { "CAT123khg", "B?T*", false, false };

            // https://github.com/dotnet/runtime/blob/686d0748a5d2e4f078fc7247e04bf34b18435746/src/libraries/Microsoft.VisualBasic.Core/tests/LikeOperatorTests.cs
            yield return new object?[] { null, null, true, true };
            yield return new object?[] { null, "*", true, true };
            yield return new object?[] { "", null, true, true };
            yield return new object?[] { "", "*", true, true };
            yield return new object?[] { "", "?", false, false };
            yield return new object?[] { "a", "?", true, true };
            yield return new object?[] { "a3", "[A-Z]#", false, true };
            yield return new object?[] { "A3", "[a-a]#", false, true };
        }
    }
}
