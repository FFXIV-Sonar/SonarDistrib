using Microsoft.VisualBasic;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Xoshiro.PRNG64;
using Xunit;
using Xunit.Sdk;

namespace SonarUtils.Tests
{
    public static class StringUtilsTests
    {
        private static readonly string[] s_fruitsAndColors =
        [
            // Taken from: https://myenglishtutors.org/list-of-fruits/
            "Orange", "Apple", "Avocado", "Mango", "Peach", "Cherry", "Grape", "Banana",
            "Watermelon", "Strawberry", "Blueverry", "Guava", "Raspberry", "Kiwi", "Apricot", "Pear", // 16 fruits
            //"Fig", "Lemon", "Papaya", "Pomegranate", "Plum", "Passion fruit", "Coconut", "Lychee", // Commented to have an equal number

            // Taken from: https://byjus.com/english/names-of-colours/
            "Red", "Green", "Blue", "Indigo", "Orange", "Yellow", "Violet", "Grey",
            "Maroon", "Black", "Olive", "Cyan", "Pink", "Magenta", "Tan", "Teal", // 16 colors
        ]; // Total: 32 * 32 = 1024 pair possibilities

        [Fact]
        [SuppressMessage("Major Bug", "S2114", Justification = "Intended")]
        public static void CanInternNumbersInRange()
        {
            var random = new XoShiRo256starstar(42); // Ensure reproducible order
            var dict = GenerateNumbers().OrderBy(number => random.Next()).ToDictionary(number => number, number => StringUtils.GetNumber(number));
            foreach (var (number, str) in dict.Concat(dict).Concat(dict).Concat(dict).OrderBy(kvp => random.Next()))
            {
                // NOTE: Small numbers up to 300 are cached.
                // This interner is intended to extend that range to cover some of the Excel sheets range.
                var expected = number >= short.MinValue && number <= short.MaxValue;
                if (expected) Assert.Same(StringUtils.GetNumber(number), str);
                else Assert.NotSame(StringUtils.GetNumber(number), str);
            }
        }

        [Fact]
        [SuppressMessage("Major Bug", "S2114", Justification = "Intended")]
        public static void CanInternStrings()
        {
            var random = new XoRoShiRo128starstar(42); // Ensure reproducible order
            var strings = GenerateStringPairs(s_fruitsAndColors, 100000).Select(StringUtils.Intern).ToArray();
            foreach (var str in strings.Concat(strings).Concat(strings).Concat(strings).OrderBy(str => random.Next()))
            {
                Assert.Same(StringUtils.Intern(str), str);
            }
        }

        [Fact]
        [SuppressMessage("Major Bug", "S2114", Justification = "Intended")]
        public static void NumbersAndNumericalStringsAreTheSame()
        {
            var random = new XoShiRo256starstar(42); // Ensure reproducible order
            var dict = GenerateNumbers().OrderBy(number => random.Next()).ToDictionary(number => number, number => StringUtils.GetNumber(number));
            foreach (var (number, str) in dict.Concat(dict).Concat(dict).Concat(dict).OrderBy(kvp => random.Next()))
            {
                var expected = number >= short.MinValue && number <= short.MaxValue;

                // NOTE: This will technically mess the cached range as all numbers will end up interned.
                // This is for testing purposes only.
                if (expected) Assert.Same(StringUtils.Intern($"{number}"), str);
                else Assert.NotSame(StringUtils.Intern($"{number}"), str);
            }
        }

        [Fact]
        public static void ResetWorks()
        {
            static string[] generator() => [..GenerateStringPairs(s_fruitsAndColors, 1000).Distinct(), ..GenerateNumbers(1000, 2000).Select(number => StringUtils.GetNumber(number))];

            var strings1 = generator();
            Assert.All(strings1, str => Assert.Same(StringUtils.Intern(str), str));

            StringUtils.Reset();

            var strings2 = generator();
            Assert.Equal(strings1.Length, strings2.Length);
            for (var index = 0; index < strings1.Length; index++)
            {
                Assert.NotSame(strings1[index], strings2[index]);
            }
        }

        [Fact]
        public static void InstanceSymbolsConsistency()
        {
            for (var instanceId = 0u; instanceId < 65536; instanceId++)
            {
                var symbol = StringUtils.GetInstanceSymbol(instanceId);
                Assert.Equal(instanceId, StringUtils.GetInstanceIdFromSymbol(symbol));
            }
        }

        [Theory]
        [InlineData(40, "i40")]
        [InlineData(0, "")]
        [InlineData(3, "\uE0B3")]
        public static void InstanceSymbolTest(uint instanceId, string symbol)
        {
            Assert.Equal(symbol, StringUtils.GetInstanceSymbol(instanceId));
            Assert.Equal(instanceId, StringUtils.GetInstanceIdFromSymbol(symbol));
        }

        [Theory]
        [InlineData(60, "i60", InstanceSymbolKind.Circled)]
        [InlineData(60, "i60", InstanceSymbolKind.None)]
        [InlineData(0, "", InstanceSymbolKind.Circled)]
        [InlineData(3, "\u2462", InstanceSymbolKind.Circled)]
        public static void InstanceSymbolTestWithKind(uint instanceId, string symbol, InstanceSymbolKind kind)
        {
            Assert.Equal(symbol, StringUtils.GetInstanceSymbol(instanceId, kind));
            Assert.Equal(instanceId, StringUtils.GetInstanceIdFromSymbol(symbol, kind));
        }

        [Fact]
        public static void InstanceSymbolForZeroIsEmptyString()
        {
            Assert.Equal(string.Empty, StringUtils.GetInstanceSymbol(0));
            Assert.Equal(0u, StringUtils.GetInstanceIdFromSymbol(string.Empty));
        }

        private static int[] GenerateNumbers(int min = -100000, int max = 100000) => [..Enumerable.Range(min, max - min + 1)];

        private static string[] GenerateStringPairs(string[] strings, int pairs)
        {
            if (strings.Length == 0) throw new ArgumentException($"{nameof(strings)} array must have at least 1 string", nameof(strings));
            var random = new XoShiRo256starstar(42); // Ensure reproducible sequences
            return [..Enumerable.Range(0, pairs).Select(pair => $"{strings[random.Next(strings.Length)]} {strings[random.Next(strings.Length)]}")];
        }
    }
}
