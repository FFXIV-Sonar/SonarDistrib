using MessagePack;
using Sonar.Messages;
using Sonar.Utilities;
using System;

namespace Sonar.Tokens
{
    [MessagePackObject]
    public abstract class SonarTokenBase : ISonarToken
    {
        /// <inheritdoc/>
        [Key(0)]
        public byte[]? Data { get; init; }

        /// <inheritdoc/>
        [IgnoreMember]
        public double IssuedAt
        {
            get
            {
                if (this.Data is null || this.Data.Length < 16) return 0;
                return BitConverter.ToDouble(this.Data.AsSpan()[^16..^8]);
            }
        }

        /// <inheritdoc/>
        [IgnoreMember]
        public double ExpiresAt
        {
            get
            {
                if (this.Data is null || this.Data.Length < 16) return 0;
                return BitConverter.ToDouble(this.Data.AsSpan()[^8..^0]);
            }
        }

        /// <inheritdoc/>
        [IgnoreMember]
        public double Duration => this.ExpiresAt - this.IssuedAt;

        /// <inheritdoc/>
        [IgnoreMember]
        public bool IsExpired
        {
            get
            {
                var now = UnixTimeHelper.SyncedUnixNow;
                return now < this.IssuedAt || now > this.ExpiresAt;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => UrlBase64.Encode(this.Data ?? Array.Empty<byte>());
    }
}
