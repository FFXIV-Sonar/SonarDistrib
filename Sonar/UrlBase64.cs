using System;
using System.Collections.Generic;

namespace Sonar
{
    // https://github.com/neosmart/UrlBase64/blob/master/UrlBase64/UrlBase64.cs
    internal enum UrlBase64PaddingPolicy
    {
        Discard,
        Preserve,
    }

    internal static class UrlBase64
    {
        private static readonly char[] TwoPads = { '=', '=' };

        public static string Encode(ReadOnlySpan<byte> bytes, UrlBase64PaddingPolicy padding = UrlBase64PaddingPolicy.Discard)
        {
            var encoded = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
            if (padding == UrlBase64PaddingPolicy.Discard)
            {
                encoded = encoded.TrimEnd('=');
            }
            return encoded;
        }

        public static byte[] Decode(string encoded)
        {
            var chars = new List<char>(encoded.ToCharArray());
            for (var i = 0; i < chars.Count; ++i)
            {
                if (chars[i] == '_')
                {
                    chars[i] = '/';
                }
                else if (chars[i] == '-')
                {
                    chars[i] = '+';
                }
            }

            switch (encoded.Length % 4)
            {
                case 2:
                    chars.AddRange(TwoPads);
                    break;
                case 3:
                    chars.Add('=');
                    break;
            }

            var array = chars.ToArray();
            return Convert.FromBase64CharArray(array, 0, array.Length);
        }
    }
}
