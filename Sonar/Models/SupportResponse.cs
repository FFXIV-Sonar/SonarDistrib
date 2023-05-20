using MessagePack;
using Sonar.Messages;

namespace Sonar.Models
{
    [MessagePackObject]
    public sealed class SupportResponse : ISonarMessage
    {
        [Key(0)]
        public int Id { get; set; }

        [Key(1)]
        public bool Successful { get; set; }

        [Key(2)]
        public string? Message { get; set; }

        [Key(3)]
        public string? Exception { get; set; }

        public override string ToString()
        {
            return this.Message ?? string.Empty;
        }
    }
}
