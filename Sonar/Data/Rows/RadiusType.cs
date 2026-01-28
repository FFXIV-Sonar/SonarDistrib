namespace Sonar.Data.Rows
{
    public enum RadiusType : byte
    {
        /// <summary>No radius. <c>X</c>, <c>Y</c> and <c>Z</c> are ignored.</summary>
        None,

        /// <summary>Spherical radius using <c>X</c>, <c>Y</c> and <c>Z</c> as radius.</summary>
        Spherical,

        /// <summary>Cubic radius using <c>X</c>, <c>Y</c> and <c>Z</c> as radius.</summary>
        Cubic,

        /// <summary>Cylindrical radius using <c>X</c> and <c>Y</c> as radius. <c>Z</c> is ignored.</summary>
        Cylindrical,

        /// <summary>Special value to cover everything. <c>X</c>, <c>Y</c> and <c>Z</c> are ignored.</summary>
        Global = 255,
    }
}
