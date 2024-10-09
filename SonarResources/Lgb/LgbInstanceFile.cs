using Lumina;
using SonarUtils;
using System;

namespace SonarResources.Lgb
{
    public sealed class LgbInstanceFile : IEquatable<LgbInstanceFile>
    {
        private string? _key;

        public GameData Data { get; }
        public string Path { get; }
        public string Key => this._key ??= $"{this.Data.DataPath}:{this.Path}";
        public uint ZoneId { get; init; } // NOTE: Currently assuming the first zone using a set lgb is the field zone.

        public LgbInstanceFile(GameData data, string path, uint zoneId)
        {
            this.Data = data;
            this.Path = path;
            this.ZoneId = zoneId;
        }

        public bool Equals(LgbInstanceFile? other) => other is not null && this.Key.Equals(other.Key);
        public override bool Equals(object? obj) => obj is LgbInstanceFile other && this.Equals(other);
        public override int GetHashCode() => this.Key.GetHashCode();
    }
}
