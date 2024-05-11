namespace Sonar.Tokens
{
    /// <summary>Sonar Token containing authentication details</summary>
    public sealed class SonarAuthToken : SonarTokenBase
    {
        public static SonarAuthToken FromString(string token) => new() { Data = UrlBase64.Decode(token) };
    }
}
