namespace Sonar.Connections
{
    public sealed partial class SonarConnectionManager
    {
        // Houses connection information
        private sealed class SonarConnectionInformation
        {
            /// <summary>Server-side connection ID.</summary>
            public uint? Id { get; set; }

            /// <summary>Server-side connection type.</summary>
            /// <remarks>Should match <see cref="SonarUrl.Type"/> of the associated <see cref="Url"/>.</remarks>
            public ConnectionType Type { get; set; }
            
            /// <summary>Associated <see cref="SonarUrl"/>.</summary>
            public required SonarUrl Url { get; init; }
        }
    }
}
