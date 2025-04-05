using Org.BouncyCastle.Crypto.Digests;
using System;

namespace SonarUtils
{
    public static class BouncySha256
    {
        public static byte[] HashData(ReadOnlySpan<byte> bytes)
        {
            var sha256 = new Sha256Digest();
            sha256.BlockUpdate(bytes);
            var result = new byte[sha256.GetDigestSize()];
            sha256.DoFinal(result);
            return result;
        }
    }
}