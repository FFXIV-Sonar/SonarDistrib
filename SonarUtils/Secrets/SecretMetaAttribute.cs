using System;
using System.Buffers.Text;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SonarUtils.Secrets
{
    /// <summary>Sonar secret meta attribute.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class SecretMetaAttribute : Attribute
    {
        /// <summary>Raw bytes.</summary>
        public ImmutableArray<byte>? Bytes { get; }

        public SecretMetaAttribute(string? base64UrlBytes)
        {
            try
            {
                this.Bytes = ImmutableCollectionsMarshal.AsImmutableArray(Base64Url.DecodeFromChars(base64UrlBytes));
            }
            catch
            {
                /* Swallow */
            }
        }

        public SecretMetaAttribute(byte[]? bytes)
        {
            this.Bytes = bytes?.ToImmutableArray();
        }
    }
}
