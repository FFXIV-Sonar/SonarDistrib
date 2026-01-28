using System;

namespace SonarUtils.Greek
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal sealed class GreekSymbolAttribute : Attribute
    {
        public GreekSymbolAttribute(string chars)
        {
            if (chars.Length is not 2 and not 3) throw new ArgumentException("Two or three characters are expected", nameof(chars));

            // Original chars string
            this.Chars = chars;

            // Gets the upper and lower case characters
            var upper = chars[0];
            var lower = chars[1];
            var final = chars[^1];

            // Assign char properties with their values
            this.UpperChar = upper;
            this.LowerChar = lower;
            this.FinalChar = final;

            // Assign string properties with their values
            this.UpperString = $"{upper}";
            this.LowerString = $"{lower}";
            this.FinalString = $"{final}";
        }

        /// <summary>All characters representing this symbol.</summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>If length is <c>2</c>, it contains <c>Aa</c>.</item>
        /// <item>If length is <c>3</c>, it contains <c>Aaf</c>.</item>
        /// <br/>
        /// <item><c>A</c>: Upper case.</item>
        /// <item><c>a</c>: Lower case.</item>
        /// <item><c>f</c>: End of words lower case (final symbol).</item>
        /// </list>
        /// </remarks>
        public string Chars { get; }

        /// <summary>Upper case symbol character.</summary>
        public char UpperChar { get; }

        /// <summary>Lower case symbol character.</summary>
        public char LowerChar { get; }

        /// <summary>End of words lower case symbol character.</summary>
        public char FinalChar { get; }

        /// <summary>Upper case symbol character as a <see langword="string"/>.</summary>
        public string UpperString { get; }

        /// <summary>Lower case symbol character as a <see langword="string"/>.</summary>
        public string LowerString { get; }

        /// <summary>End of words lower case symbol character as a <see langword="string"/>.</summary>
        public string FinalString { get; }

        /// <summary>Returns <see cref="Chars"/>.</summary>
        public override string ToString() => this.Chars;
    }
}
