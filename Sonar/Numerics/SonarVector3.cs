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
    public struct SonarVector3 : IEquatable<SonarVector3>
    {
        public const double MessagePackAccuracy = 10;

        public static SonarVector3 Zero => new();
        public static SonarVector3 UnitX => new(1, 0, 0);
        public static SonarVector3 UnitY => new(0, 1, 0);
        public static SonarVector3 UnitZ => new(0, 0, 1);
        public static SonarVector3 One => new(1, 1, 1);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public SonarVector3(int msgPackX, int msgPackY, int msgPackZ)
        {
            this.X = (float)Math.Round(msgPackX / MessagePackAccuracy);
            this.Y = (float)Math.Round(msgPackY / MessagePackAccuracy);
            this.Z = (float)Math.Round(msgPackZ / MessagePackAccuracy);
        }

        public SonarVector3(float x, float y, float z)
        {
            this.X = x; this.Y = y; this.Z = z;
        }

        public SonarVector3(float value) : this(value, value, value) { }
        public SonarVector3(SonarVector2 vec, float z) : this(vec.X, vec.Y, z) { }
        public SonarVector3(SonarVector3 vec) : this(vec.X, vec.Y, vec.Z) { }

        [JsonProperty]
        [JsonInclude]
        [IgnoreMember]
        public float X;
        [JsonProperty]
        [JsonInclude]
        [IgnoreMember]
        public float Y;
        [JsonProperty]
        [JsonInclude]
        [IgnoreMember]
        public float Z;

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Text.Json.Serialization.JsonIgnore]
        [Key(2)]
        public int msgPackZ
        {
            readonly get => unchecked((int)Math.Round(this.Z * MessagePackAccuracy));
            set => this.Z = (float)(value / MessagePackAccuracy);
        }

        public readonly float Length() => (float)Math.Sqrt(this.LengthSquared());
        public readonly float LengthSquared() => this * this;
        public readonly SonarVector3 Unit()
        {
            var length = this.Length();
            return new SonarVector3(this.X / length, this.Y / length, this.Z / length);
        }
        public readonly float Dot(SonarVector3 vec) => this * vec;
        public readonly SonarVector3 Cross(SonarVector3 vec) => new SonarVector3(this.Y * vec.Z - this.Z * vec.Y, this.Z * vec.X - this.X * vec.Z, this.X * vec.Y - this.Y * vec.X);
        public readonly SonarVector3 Delta(SonarVector3 vec) => vec - this;
        public readonly float DeltaAngle(SonarVector3 vec) => (float)Math.Acos((this * vec) / (this.Length() / vec.Length()));

        public readonly override string ToString() => $"({this.X}, {this.Y}, {this.Z})";

        public static implicit operator Vector3(SonarVector3 vec) => new Vector3(vec.X, vec.Y, vec.Z);

        public static implicit operator Vector2(SonarVector3 vec) => new Vector2(vec.X, vec.Y);

        public static implicit operator SonarVector3(Vector3 vec) => new SonarVector3(vec.X, vec.Y, vec.Z);

        public static implicit operator SonarVector3(SonarVector2 vec) => new SonarVector3(vec.X, vec.Y, 0);

        public static implicit operator SonarVector3(Vector2 vec) => new SonarVector3(vec.X, vec.Y, 0);

        public static SonarVector3 operator +(SonarVector3 vec) => vec;
        public static SonarVector3 operator -(SonarVector3 vec) => new SonarVector3(-vec.X, -vec.Y, -vec.Z);

        public static SonarVector3 operator +(SonarVector3 vec1, SonarVector3 vec2) => new SonarVector3(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);
        public static SonarVector3 operator -(SonarVector3 vec1, SonarVector3 vec2) => vec1 + -vec2;

        public static SonarVector3 operator *(SonarVector3 vec, float value) => new SonarVector3(vec.X * value, vec.Y * value, vec.Z * value);
        public static SonarVector3 operator /(SonarVector3 vec, float value) => new SonarVector3(vec.X / value, vec.Y / value, vec.Z / value);

        public static float operator *(SonarVector3 vec1, SonarVector3 vec2) => vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;

        public static bool operator ==(SonarVector3 vec1, SonarVector3 vec2) => vec1.X == vec2.X && vec1.Y == vec2.Y && vec1.Z == vec2.Z;
        public static bool operator !=(SonarVector3 vec1, SonarVector3 vec2) => vec1.X != vec2.X && vec1.Y != vec2.Y && vec1.Z != vec2.Z;

        public static bool operator ==(SonarVector3 vec, float value) => vec.Length() == value;
        public static bool operator !=(SonarVector3 vec, float value) => vec.Length() != value;

        public static bool operator <(SonarVector3 vec1, SonarVector3 vec2) => vec1.LengthSquared() < vec2.LengthSquared();
        public static bool operator <=(SonarVector3 vec1, SonarVector3 vec2) => vec1.LengthSquared() <= vec2.LengthSquared();
        public static bool operator >=(SonarVector3 vec1, SonarVector3 vec2) => vec1.LengthSquared() >= vec2.LengthSquared();
        public static bool operator >(SonarVector3 vec1, SonarVector3 vec2) => vec1.LengthSquared() > vec2.LengthSquared();

        public static bool operator <(SonarVector3 vec1, float value) => vec1.Length() < value;
        public static bool operator <=(SonarVector3 vec1, float value) => vec1.Length() <= value;
        public static bool operator >=(SonarVector3 vec1, float value) => vec1.Length() >= value;
        public static bool operator >(SonarVector3 vec1, float value) => vec1.Length() > value;

        public readonly override bool Equals(object? obj) => obj is SonarVector3 vec && (this.X, this.Y, this.Z).Equals((vec.X, vec.Y, vec.Z));
        public readonly bool Equals(SonarVector3 v) => this == v;
        public readonly override int GetHashCode() => (this.X, this.Y, this.Z).GetHashCode();
    }
}
