using SonarUtils.Text.Placeholders;
using SonarUtils.Text.Placeholders.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xoshiro.PRNG64;

namespace SonarUtils.Tests
{
    public static class PlaceholderFormatterTests
    {
        private static readonly PlaceholderFormatter s_formatter = new();

        // This is by no means an accurate representation of a relay's data, this is for testing purposes only
        private static readonly Dictionary<string, string> s_dictionary = new()
        {
            { "worldid", "62" }, { "zoneid", "818" }, { "instanceid", "3" },
            { "world", "Diabolos" }, { "zone", "The Tempest" }, { "instance", "i3" },
            { "coords", "12.1, 11.2" }, { "flag", ">The Tempest (12.1, 11.2) i3" }, // No, I did not look where this coordinates goes into
            { "name", "Baal" }, { "rank", "A" }, { "what", "<what>"}, { "this", "<that>" },
            { "this is", "not meant to work" }
        };

        [Theory]
        [InlineData("Rank <rank>: <name> <flag>", "Rank A: Baal >The Tempest (12.1, 11.2) i3")]
        [InlineData("<name> is alive!", "Baal is alive!")]
        [InlineData("<doesnotexist>", "<doesnotexist>")]
        [InlineData("<<world>>", "<Diabolos>")]
        [InlineData("<some<name>name>", "<someBaalname>")]
        [InlineData("<>", "<>")]
        [InlineData("<<name>", "<Baal")]
        [InlineData("<name>>", "Baal>")]
        [InlineData("1 2 4 <<name> 8 16 32", "1 2 4 <Baal 8 16 32")]
        [InlineData("1 2 3 <name>> 4 5 6", "1 2 3 Baal> 4 5 6")]
        [InlineData("There's nothing to replace", "There's nothing to replace")]
        [InlineData("4 < 3 == false", "4 < 3 == false")]
        [InlineData("4 > 3 == true", "4 > 3 == true")]
        [InlineData("><", "><")]
        [InlineData(">x<", ">x<")]
        [InlineData("<what>", "<what>")] // Yes this is a bit of trickery
        [InlineData("<this>", "<that>")]
        [InlineData("<this is>", "<this is>")] // Spaces are not supported by the default regex
        [InlineData("<rank><name>", "ABaal")]
        [InlineData("<World>", "<World>")] // Yep, case sensitive, <World> is not the same as <world>
        public static void FormatText(string inputText, string expectedOutput)
        {
            Assert.Equal(expectedOutput, s_formatter.Format(inputText, s_dictionary));
        }

        [Fact]
        public static void StressTest1()
        {
            var input = string.Join("", Enumerable.Repeat("<name>", 1000000));
            var expected = string.Join("", Enumerable.Repeat("Baal", 1000000));
            Assert.Equal(expected, s_formatter.Format(input, s_dictionary));
        }

        [Fact]
        public static void StressTest2()
        {
            var input = new string([.. string.Join("", Enumerable.Repeat("<", 1000000)), .. "rank", .. string.Join("", Enumerable.Repeat(">", 1000000))]);
            var expected = new string([.. string.Join("", Enumerable.Repeat("<", 999999)), .. "A", .. string.Join("", Enumerable.Repeat(">", 999999))]);
            Assert.Equal(expected, s_formatter.Format(input, s_dictionary));
        }

        [Fact]
        public static void StressTest3()
        {
            var random = new XoShiRo256starstar(42);
            var input = new string(Enumerable.Range(0, 1000000).Select(i => random.NextDouble() < 0.5 ? '<' : '>').ToArray()); // Some random text consisting of < and >
            Assert.Equal(input, s_formatter.Format(input, s_dictionary));
        }
    }
}
