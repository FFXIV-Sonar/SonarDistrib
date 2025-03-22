using System;
using Sonar.Numerics;
using MessagePack;
using Sonar.Data.Extensions;
using Sonar.Data;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SonarUtils;
using System.Diagnostics.CodeAnalysis;

namespace Sonar.Models
{
    /// <summary>
    /// Represent a game position (World, Zone, Instance and Coords)
    /// </summary>
    [MessagePackObject]
    [Serializable]
    public class GamePosition : GamePlace
    {
        public GamePosition() { }
        public GamePosition(GamePlace p) : base(p) {  }
        public GamePosition(GamePosition p) : base(p)
        {
            this.Coords = p.Coords;
        }

        /// <summary>
        /// Game coordinates
        /// </summary>
        [Key(3)]
        public SonarVector3 Coords { get; set; } = new();

        #region Distance Functions
        /// <summary>
        /// Calculate distance to another position
        /// </summary>
        public float GetDistanceTo(SonarVector3 coords) => (this.Coords - coords).Length();

        /// <summary>
        /// Calculate distance to another position
        /// </summary>
        public float GetDistanceTo(GamePosition position) => this.GetDistanceTo(position.Coords);

        /// <summary>
        /// Calculate horizontal distance to another position
        /// </summary>
        public float GetHorizontalDistanceTo(SonarVector2 coords) => (new SonarVector2(this.Coords.X, this.Coords.Y) - coords).Length();

        /// <summary>
        /// Calculate horizontal distance to another position
        /// </summary>
        public float GetHorizontalDistanceTo(SonarVector3 coords) => this.GetHorizontalDistanceTo(new SonarVector2(coords.X, coords.Y));

        /// <summary>
        /// Calculate horizontal distance to another position
        /// </summary>
        public float GetHorizontalDistanceTo(GamePosition position) => this.GetHorizontalDistanceTo(position.Coords);

        /// <summary>
        /// Calculate horizontal* distance to another position
        /// </summary>
        public float GetDistanceTo(SonarVector2 coords) => this.GetHorizontalDistanceTo(coords);
        #endregion

        public override bool TryGetValue(ReadOnlySpan<char> name, [MaybeNullWhen(false)] out ReadOnlySpan<char> value)
        {
            var result = name switch
            {
                "flag" => this.GetFlagString(MapFlagFormatFlags.IngamePreset),
                "coords" => MapFlagUtils.FlagToString(this.GetFlag(), MapFlagFormatFlags.IngamePreset),

                "x" => $"{this.GetFlag().X:F2}",
                "y" => $"{this.GetFlag().Y:F2}",
                "z" => $"{this.GetFlag().Z:F2}",

                "rawx" => $"{this.Coords.X:F2}",
                "rawy" => $"{this.Coords.Y:F2}",
                "rawz" => $"{this.Coords.Z:F2}",

                _ => null,
            };

            if (result is not null)
            {
                value = result;
                return true;
            }
            return base.TryGetValue(name, out value);
        }

        public override string ToString()
        {
            var ret = new List<string>() { $"{this.GetZone()}", MapFlagUtils.FlagToString(this.GetFlag()) };
            if (this.InstanceId > 0) ret.Add($"{this.InstanceId}인스");
            ret.Add($"<{this.GetWorld()}>");
            return string.Join(' ', ret);
        }
        //public override string ToString() => $"{this.GetZone()} {MapFlagUtils.FlagToString(this.GetFlag())} <{this.GetWorld()}>";

        public new GamePosition Clone() => Unsafe.As<GamePosition>(this.MemberwiseClone());
    }
}
