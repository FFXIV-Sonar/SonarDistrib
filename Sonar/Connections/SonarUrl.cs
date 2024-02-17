using MessagePack;
using System;
using System.Threading;

namespace Sonar.Connections
{
    [MessagePackObject]
    public sealed partial class SonarUrl : IEquatable<SonarUrl>, IEquatable<string>, IEquatable<Uri>
    {
        private readonly Lazy<Uri> _uri;

        public SonarUrl()
        {
            this._uri = new(this.GenerateUri, LazyThreadSafetyMode.None);
        }

        [Key(0)]
        public required string Url { get; init; }

        [Key(5)]
        public ConnectionType Type { get; init; }

        /// <summary>Disable to remove during bootstrap process. Default: <c>true</c></summary>
        [Key(1)]
        public bool Enabled { get; init; } = true;

        /// <summary>Specifies this url is a proxy.</summary>
        [Key(2)]
        public bool Proxy { get; init; }

        /// <summary>Specifies this url should only be used during a user triggered reconnect.</summary>
        [Key(3)]
        public bool ReconnectOnly { get; init; }

        /// <summary>Debug only URL.</summary>
        [Key(4)]
        public bool Debug { get; init; }

        [IgnoreMember]
        public Uri Uri => this._uri.Value;


        private Uri GenerateUri() => new(this.Url);
        public override int GetHashCode() => this.Url.GetHashCode();
        public override string ToString() => this.Url;
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return obj switch
            {
                SonarUrl url => this.Equals(url),
                string url => this.Equals(url),
                Uri uri => this.Equals(uri),
                _ => false,
            };
        }
        public bool Equals(SonarUrl? url) => url is not null && this.Equals(url.Url);
        public bool Equals(string? url) => url is not null && this.Url.Equals(url);
        public bool Equals(Uri? uri) => uri is not null && this.Uri.Equals(uri);

        public static implicit operator string(SonarUrl url) => url.Url;
        public static implicit operator Uri(SonarUrl url) => url.Uri;

        public static bool operator ==(SonarUrl? a, SonarUrl? b) => !(a is null || b is null) ? a.Equals(b) : Equals(a, b);
        public static bool operator ==(SonarUrl? a, string? b) => !(a is null || b is null) ? a.Equals(b) : Equals(a, b);
        public static bool operator ==(string? b, SonarUrl? a) => !(a is null || b is null) ? a.Equals(b) : Equals(a, b);
        public static bool operator ==(SonarUrl? a, Uri? b) => !(a is null || b is null) ? a.Equals(b) : Equals(a, b);
        public static bool operator ==(Uri? b, SonarUrl? a) => !(a is null || b is null) ? a.Equals(b) : Equals(a, b);

        public static bool operator !=(SonarUrl? a, SonarUrl? b) => !(a == b);
        public static bool operator !=(SonarUrl? a, string? b) => !(a == b);
        public static bool operator !=(string? b, SonarUrl? a) => !(a == b);
        public static bool operator !=(SonarUrl? a, Uri? b) => !(a == b);
        public static bool operator !=(Uri? b, SonarUrl? a) => !(a == b);
    }
}
