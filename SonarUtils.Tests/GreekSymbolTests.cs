using SonarUtils.Greek;
using System;
using System.Linq;

namespace SonarUtils.Tests
{
    public static class GreekSymbolTests
    {
        [Theory]
        [InlineData(GreekSymbol.Alpha, "Αα")]
        [InlineData(GreekSymbol.Beta, "Ββ")]
        [InlineData(GreekSymbol.Gamma, "Γγ")]
        [InlineData(GreekSymbol.Delta, "Δδ")]
        [InlineData(GreekSymbol.Epsilon, "Εε")]
        [InlineData(GreekSymbol.Zeta, "Ζζ")]
        [InlineData(GreekSymbol.Eta, "Ηη")]
        [InlineData(GreekSymbol.Theta, "Θθ")]
        [InlineData(GreekSymbol.Iota, "Ιι")]
        [InlineData(GreekSymbol.Kappa, "Κκ")]
        [InlineData(GreekSymbol.Lambda, "Λλ")]
        [InlineData(GreekSymbol.Mu, "Μμ")]
        [InlineData(GreekSymbol.Nu, "Νν")]
        [InlineData(GreekSymbol.Xi, "Ξξ")]
        [InlineData(GreekSymbol.Omicron, "Οο")]
        [InlineData(GreekSymbol.Pi, "Ππ")]
        [InlineData(GreekSymbol.Rho, "Ρρ")]
        [InlineData(GreekSymbol.Sigma, "Σσς")]
        [InlineData(GreekSymbol.Tau, "Ττ")]
        [InlineData(GreekSymbol.Upsilon, "Υυ")]
        [InlineData(GreekSymbol.Phi, "Φφ")]
        [InlineData(GreekSymbol.Chi, "Χχ")]
        [InlineData(GreekSymbol.Psi, "Ψψ")]
        [InlineData(GreekSymbol.Omega, "Ωω")]
        public static void CorrectRepresentation(GreekSymbol symbol, string chars)
        {
            Assert.Equal(chars, symbol.Chars);

            var upperChar = chars[0];
            Assert.Equal(upperChar, symbol.UpperChar);

            var lowerChar = chars[1];
            Assert.Equal(lowerChar, symbol.LowerChar);

            var finalChar = chars[^1];
            Assert.Equal(finalChar, symbol.FinalChar);

            var upperString = $"{upperChar}";
            Assert.Equal(upperString, symbol.UpperString);

            var lowerString = $"{lowerChar}";
            Assert.Equal(lowerString, symbol.LowerString);

            var finalString = $"{finalChar}";
            Assert.Equal(finalString, symbol.FinalString);
        }

        [Fact]
        public static void InvalidMustBeZeroAndEmpty()
        {
            var symbol = GreekSymbol.Invalid;
            Assert.Equal(string.Empty, symbol.Chars);
            Assert.Equal('\0', symbol.UpperChar);
            Assert.Equal('\0', symbol.LowerChar);
            Assert.Equal('\0', symbol.FinalChar);
            Assert.Equal(string.Empty, symbol.UpperString);
            Assert.Equal(string.Empty, symbol.LowerString);
        }

        [Theory]
        [MemberData(nameof(CharsToGreekSymbolSource))]
        public static void CharsCorrespondToCorrectSymbol(char ch, GreekSymbol symbol)
        {
            Assert.Equal(symbol, GreekSymbolUtils.ToGreekSymbol(ch));
        }


        public static TheoryData<char, GreekSymbol> CharsToGreekSymbolSource()
        {
            var data = new TheoryData<char, GreekSymbol>();

            var symbols = Enum.GetValues<GreekSymbol>()
                .Where(symbol => symbol is not GreekSymbol.Invalid);

            // Add all characters from all symbols for testing
            foreach (var symbol in symbols)
            {
                foreach (var ch in symbol.Chars)
                {
                    data.Add(ch, symbol);
                }
            }

            // Some invalid symbol
            data.Add('1', GreekSymbol.Invalid);

            // Return all test theory data
            return data;
        }
    }
}
