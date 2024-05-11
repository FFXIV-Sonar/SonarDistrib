using MessagePack;
using Sonar.Messages;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sonar.Models
{
    [MessagePackObject]
    public sealed partial class ClientSecret : ISonarMessage
    {
        /// <summary>Secret name</summary>
        [Key(0)]
        public required string? SecretName { get; init; }

        /// <summary>Secret hash</summary>
        [Key(1)]
        public required byte[]? SecretHash { get; init; } // = HashSecret(this.SecretName)

        /// <summary>Secret comment</summary>
        [Key(2)]
        public string? Comment { get; init; }

        /// <summary>Validate secret</summary>
        public bool Validate()
        {
            if (this.SecretName is null || this.SecretHash is null) return false;
            return this.SecretHash.SequenceEqual(HashSecret(this.SecretName));
        }

        public override string ToString()
        {
            return $"{this.SecretName}: {Convert.ToBase64String(this.SecretHash ?? Array.Empty<byte>())} ({(this.Comment is null ? string.Empty : this.Comment.Length > 32 ? this.Comment[..32] : this.Comment)})";
        }
    }
}
