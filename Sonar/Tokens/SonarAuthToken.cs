using MessagePack;

namespace Sonar.Tokens
{
    /// <summary>Sonar Token containing authentication details</summary>
    [MessagePackObject]
    public sealed class SonarAuthToken : SonarTokenBase
    {
        public static SonarAuthToken FromString(string token) => new() { Data = UrlBase64.Decode(token) };
    }
}
