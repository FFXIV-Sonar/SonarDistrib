using MessagePack;
using Sonar.Messages;

namespace Sonar.Tokens
{
    /// <summary>Sonar near-equivalent of a JWT</summary>
    public interface ISonarToken : ISonarMessage
    {
        /// <summary>Authenticated encrypted data</summary>
        public byte[]? Data { get; }

        /// <summary>Unix Epoch issued time in milliseconds</summary>
        public double IssuedAt { get; }

        /// <summary>Unix Epoch expiration time in milliseconds</summary>
        public double ExpiresAt { get; }

        /// <summary>Validity duration in milliseconds</summary>
        /// <remarks>Expired tokens are invalid</remarks>
        public double Duration { get; }

        /// <summary>Return whether token is expired</summary>
        /// <remarks>NOTE: This is not validation! Validation is only performed server-side!</remarks>
        public bool IsExpired { get; }

        /// <summary>Get text representation of this token</summary>
        public string ToString();
    }
}
