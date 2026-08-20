using Sonar.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using System.Threading;

namespace Sonar.Models
{
    [MessagePackObject]
    public sealed class SupportMessage : ISonarMessage, ICloneable
    {
        public const int MaximumContactLength = 64;
        public const int MaximumTitleLength = 64;
        public const int MaximumPlayerNameLength = 64;
        public const int MaximumContentLength = 4096;
        public const int MaximumLogsLength = 65536;
        public const string Notes = "최대한 자세히, 그리고 가능하면 영어로 작성해 주세요.";

        /// <summary>
        /// Randomly generated for <see cref="SupportResponse"/> association.
        /// You do not need to set this value.
        /// </summary>
        [Key(0)]
        public int Id { get; set; }

        /// <summary>
        /// Server-side use only (Ignored)
        /// Message timestamp
        /// </summary>
        [Key(1)]
        public double Timestamp { get; set; }

        /// <summary>
        /// Message Type
        /// </summary>
        [Key(2)]
        public SupportType Type { get; set; }

        /// <summary>
        /// If you want to be contacted, how may we contact you.
        /// NOTE: We cannot contact you ingame
        /// </summary>
        [Key(3)]
        public string Contact { get; set; } = string.Empty;

        /// <summary>
        /// Message Title
        /// </summary>
        [Key(4)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Message Content
        /// </summary>
        [Key(5)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Player being reported (for Player Reports)
        /// </summary>
        [Key(6)]
        public string Player { get; set; } = string.Empty;

        /// <summary>
        /// Sonar and/or Chat logs
        /// </summary>
        [Key(7)]
        public string Logs { get; set; } = string.Empty;

        /// <summary>
        /// Server-side use only (Do not use!)
        /// Message metadata
        /// </summary>
        [Key(8)]
        public string Meta { get; set; } = string.Empty;

        /// <summary>
        /// Document-like string
        /// </summary>
        public string ToString(bool includeMeta)
        {
            List<string> output = new(12);
            output.Add(DateTimeOffset.FromUnixTimeMilliseconds((long)this.Timestamp).ToString("u"));
            if (includeMeta && !string.IsNullOrWhiteSpace(this.Meta)) output.Add(this.Meta);
            output.Add("========================================");
            output.Add($"Type: {this.Type}");
            if (!string.IsNullOrWhiteSpace(this.Contact)) output.Add($"Contact: {this.Contact}");
            if (!string.IsNullOrWhiteSpace(this.Title)) output.Add($"Title: {this.Title}");
            output.Add("========================================");
            output.Add(this.Body);
            if (!string.IsNullOrWhiteSpace(this.Player))
            {
                output.Add("- - - - - - - - - -  - - - - - - - - - -");
                output.Add($"Reported Player: {this.Player}");
            }
            output.Add("========================================");
            if (!string.IsNullOrWhiteSpace(this.Logs)) output.Add(this.Logs);
            return string.Join('\n', output);
        }
        /// <summary>
        /// Document-like string
        /// </summary>
        public override string ToString() => this.ToString(false);

        /// <summary>
        /// Throw if this <see cref="SupportMessage"/> isn't valid
        /// </summary>
        public void ThrowIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(this.Body)) ThrowInvalidException($"{nameof(this.Body)}은(는) 필수 작성 항목입니다.");

            ThrowIfTooLong(nameof(this.Contact), this.Contact, MaximumContactLength);
            ThrowIfTooLong(nameof(this.Title), this.Title, MaximumTitleLength);
            ThrowIfTooLong(nameof(this.Body), this.Body, MaximumContentLength);
            ThrowIfTooLong(nameof(this.Player), this.Player, MaximumPlayerNameLength);
            ThrowIfTooLong(nameof(this.Logs), this.Body, MaximumLogsLength);
            if (!string.IsNullOrEmpty(this.Meta)) ThrowInvalidException($"{nameof(this.Meta)}은(는) 비어있어야 합니다");

            ThrowIfNotText(nameof(this.Contact), this.Contact);
            ThrowIfNotText(nameof(this.Title), this.Title);
            ThrowIfNotText(nameof(this.Player), this.Player);
            ThrowIfNotText(nameof(this.Body), this.Body);
            //ThrowIfNotText(nameof(this.Logs), this.Logs); // Logs is not sanitized as its not intended to be user input.. well it is user input but logs may contain anything

            if (!Enum.IsDefined(this.Type)) ThrowInvalidException($"{nameof(this.Type)} ({this.Type}) is invalid");
            var enumType = typeof(SupportType);
            var enumName = Enum.GetName(this.Type)!;
            var meta = enumType.GetField(enumName)!.GetCustomAttributes(false).OfType<SupportTypeMetaAttribute>().First();

            if (meta.RequireContact && string.IsNullOrWhiteSpace(this.Contact)) ThrowInvalidException($"해당 유형의 문의에는 {nameof(this.Contact)}가 필요합니다");
            if (meta.RequirePlayerName && string.IsNullOrWhiteSpace(this.Player)) ThrowInvalidException($"해당 유형의 문의에는 {nameof(this.Player)}이 필요합니다");
        }

        [IgnoreMember]
        public bool FromRequired
        {
            get
            {
                if (!Enum.IsDefined(this.Type)) return false;
                var enumType = typeof(SupportType);
                var enumName = Enum.GetName(this.Type)!;
                var meta = enumType.GetField(enumName)!.GetCustomAttributes(false).OfType<SupportTypeMetaAttribute>().First();
                return meta.RequireContact;
            }
        }

        [IgnoreMember]
        public bool PlayerRequired
        {
            get
            {
                if (!Enum.IsDefined(this.Type)) return false;
                var enumType = typeof(SupportType);
                var enumName = Enum.GetName(this.Type)!;
                var meta = enumType.GetField(enumName)!.GetCustomAttributes(false).OfType<SupportTypeMetaAttribute>().First();
                return meta.RequirePlayerName;
            }
        }

        private static void ThrowIfTooLong(string name, string value, int length)
        {
            if (value.Length <= length) return;
            ThrowTooLong(name, length);
        }

        private static void ThrowIfNotText(string name, string value)
        {
            if (!IsValidText(value)) ThrowNotText(name);
        }

        private static void ThrowTooLong(string name, int length)
        {
            throw new SupportMessageException($"{name}은(는) 너무 깁니다. 최대 길이는 {length}입니다");
        }

        private static void ThrowNotText(string name)
        {
            throw new SupportMessageException($"{name}은(는) 유효한 내용이 아닙니다");
        }

        public static bool IsValidText(string text)
        {
            return text.All(c => !char.IsControl(c) || c == '\r' || c == '\n');
        }

        public static void ThrowInvalidException(string message)
        {
            throw new SupportMessageException(message);
        }

        public SupportMessage Clone() => (SupportMessage)((ICloneable)this).Clone();
        object ICloneable.Clone() => this.MemberwiseClone();

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
    public sealed class SupportMessageException : Exception
    {
        public SupportMessageException() : base() { }
        public SupportMessageException(string? message) : base(message) { }
        public SupportMessageException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
