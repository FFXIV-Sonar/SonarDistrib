using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using MessagePack;
using EditorBrowsableAttribute = System.ComponentModel.EditorBrowsableAttribute;
using EditorBrowsableState = System.ComponentModel.EditorBrowsableState;
using System.Text.Json.Serialization;

namespace Sonar.Numerics
{
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Intentional")]
    public struct SonarVector2 : IEquatable<SonarVector2>
    {
        public const double MessagePackAccuracy = 1000;
        
        public static SonarVector2 Zero => new();
        public static SonarVector2 UnitX => new(1, 0);
        public static SonarVector2 UnitY => new(0, 1);
        public static SonarVector2 One => new(1, 1);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public SonarVector2(int msgPackX, int msgPackY)
        {
            this.X = (float)Math.Round(msgPackX / MessagePackAccuracy);
            this.Y = (float)Math.Round(msgPackY / MessagePackAccuracy);
        }

        public SonarVector2(float x, float y)
        {
            this.X = x; this.Y = y;
        }

        public SonarVector2(float value) : this(value, value) { }
        public SonarVector2(SonarVector2 vec) : this(vec.X, vec.Y) { }

        [JsonProperty]
        [JsonInclude]
        [IgnoreMember]
        public float X;
        [JsonProperty]
        [JsonInclude]
        [IgnoreMember]
        public float Y;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Text.Json.Serialization.JsonIgnore]
        [Key(0)]
        public int msgPackX
        {
            readonly get => unchecked((int)Math.Round(this.X * MessagePackAccuracy));
            set => this.X = (float)(value / MessagePackAccuracy);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Text.Json.Serialization.JsonIgnore]
        [Key(1)]
        public int msgPackY
        {
            readonly get => unchecked((int)Math.Round(this.Y * MessagePackAccuracy));
            set => this.Y = (float)(value / MessagePackAccuracy);
        }

        public readonly float Length() => (float)Math.Sqrt(this.LengthSquared());
        public readonly float LengthSquared() => this * this;
        public readonly SonarVector2 Unit()
        {
            var length = this.Length();
            return new SonarVector2(this.X / length, this.Y / length);
        }
        public readonly float Dot(SonarVector2 vec) => this * vec;
        public readonly SonarVector2 Delta(SonarVector2 vec) => vec - this;
        public readonly float DeltaAngle(SonarVector2 vec) => (float)Math.Acos((this * vec) / (this.Length() / vec.Length()));

        public readonly override string ToString() => $"({this.X}, {this.Y})";

        public static implicit operator Vector2(SonarVector2 vec) => new Vector2(vec.X, vec.Y);

        public static implicit operator Vector3(SonarVector2 vec) => new Vector3(vec.X, vec.Y, 0);

        public static implicit operator SonarVector2(Vector2 vec) => new SonarVector2(vec.X, vec.Y);

        public static implicit operator SonarVector2(SonarVector3 vec) => new SonarVector2(vec.X, vec.Y);

        public static implicit operator SonarVector2(Vector3 vec) => new SonarVector2(vec.X, vec.Y);

        public static SonarVector2 operator +(SonarVector2 vec) => vec;
        public static SonarVector2 operator -(SonarVector2 vec) => new SonarVector2(-vec.X, -vec.Y);

        public static SonarVector2 operator +(SonarVector2 vec1, SonarVector2 vec2) => new SonarVector2(vec1.X + vec2.X, vec1.Y + vec2.Y);
        public static SonarVector2 operator -(SonarVector2 vec1, SonarVector2 vec2) => vec1 + -vec2;

        public static SonarVector2 operator *(SonarVector2 vec, float value) => new SonarVector2(vec.X * value, vec.Y * value);
        public static SonarVector2 operator /(SonarVector2 vec, float value) => new SonarVector2(vec.X / value, vec.Y / value);

        public static float operator *(SonarVector2 vec1, SonarVector2 vec2) => vec1.X * vec2.X + vec1.Y * vec2.Y;

        public static bool operator ==(SonarVector2 vec1, SonarVector2 vec2) => vec1.X == vec2.X && vec1.Y == vec2.Y;
        public static bool operator !=(SonarVector2 vec1, SonarVector2 vec2) => vec1.X != vec2.X && vec1.Y != vec2.Y;

        public static bool operator ==(SonarVector2 vec, float value) => vec.Length() == value;
        public static bool operator !=(SonarVector2 vec, float value) => vec.Length() != value;

        public static bool operator <(SonarVector2 vec1, SonarVector2 vec2) => vec1.LengthSquared() < vec2.LengthSquared();
        public static bool operator <=(SonarVector2 vec1, SonarVector2 vec2) => vec1.LengthSquared() <= vec2.LengthSquared();
        public static bool operator >=(SonarVector2 vec1, SonarVector2 vec2) => vec1.LengthSquared() >= vec2.LengthSquared();
        public static bool operator >(SonarVector2 vec1, SonarVector2 vec2) => vec1.LengthSquared() > vec2.LengthSquared();

        public static bool operator <(SonarVector2 vec1, float value) => vec1.Length() < value;
        public static bool operator <=(SonarVector2 vec1, float value) => vec1.Length() <= value;
        public static bool operator >=(SonarVector2 vec1, float value) => vec1.Length() >= value;
        public static bool operator >(SonarVector2 vec1, float value) => vec1.Length() > value;

        public readonly override bool Equals(object? obj) => obj is SonarVector2 vec && (this.X, this.Y).Equals((vec.X, vec.Y));
        public readonly bool Equals(SonarVector2 v) => this == v;
        public readonly override int GetHashCode() => (this.X, this.Y).GetHashCode();
    }
}
