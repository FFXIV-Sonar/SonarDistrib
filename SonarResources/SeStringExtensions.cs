using Lumina.Text;
using Lumina.Text.Payloads;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources
{
    public static class SeStringExtensions
    {
        /// <summary>Returns a filtered <see cref="string"/> with only the text payloads</summary>
        [return: NotNullIfNotNull(nameof(seString))]
        public static string? ToTextString(this SeString? seString)
        {
            if (seString is null) return null;
            var result = string.Join(string.Empty, seString.Payloads.OfType<TextPayload>().Select(p => p.RawString));
            Debug.Assert(result.All(c => c is not '\0')); // Why am I doing this if I'm replacing them below?
            return result.Replace('\0', ' ');
        }
    }
}
