using Lumina.Data.Parsing.Layer;
using Sonar.Numerics;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace SonarResources.Lgb
{
    public sealed class LgbInstance
    {
        public required string Name { get; init; }
        public required LayerEntryType Type { get; init; }
        public required IInstanceObject Object { get; init; }
        public required uint ZoneId { get; init; }
        public required SonarVector3 Coords { get; init; }
        public required SonarVector3 Scale { get; init; }
        public required SonarVector3 Rotation { get; init; }
        public required SonarVector3 Translation { get; init; }
    }
}
