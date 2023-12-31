using Sonar.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarResources.Aetherytes
{
    public sealed class AetheryteData : IEquatable<AetheryteData>
    {
        public uint Id { get; init; }

        public uint ZoneId { get; set; }
        public SonarVector3 Coords { get; set; }

        public uint MarkerId { get; set; }
        public uint LgbId { get; set; }

        public override int GetHashCode() => this.Id.GetHashCode();
        public bool Equals(AetheryteData? other) => other is not null && this.Id == other.Id;
        public override bool Equals(object? obj) => obj is AetheryteData other && this.Equals(other);
    }
}
