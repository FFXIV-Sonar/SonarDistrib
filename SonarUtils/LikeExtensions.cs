using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Runtime.CompilerServices;

namespace SonarUtils
{
    public static class LikeExtensions
    {
        /// <summary>Performs binary or text string comparison given two strings.</summary>
        /// <remarks>https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.compilerservices.likeoperator.likestring?view=net-10.0</remarks>
        /// <param name="source">The string to compare.</param>
        /// <param name="pattern">The string against which <paramref name="source"/> is being compared.</param>
        /// <param name="compareOption">A <see cref="CompareMethod"/> enumeration specifying whether or not to use text comparison. If <see cref="CompareMethod.Text"/>, this method uses text comparison; if <see cref="CompareMethod.Binary"/>, this method uses binary comparison.</param>
        /// <returns><see langword="true"/> if the strings match; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Like(this string? source, string? pattern, CompareMethod compareOption = CompareMethod.Binary)
            => LikeOperator.LikeString(source, pattern, compareOption);

        /// <summary>Performs binary or text string comparison given two objects.</summary>
        /// <remarks>https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.compilerservices.likeoperator.likeobject?view=net-10.0</remarks>
        /// <param name="source">The string to compare.</param>
        /// <param name="pattern">The string against which <paramref name="source"/> is being compared.</param>
        /// <param name="compareOption">A <see cref="CompareMethod"/> enumeration specifying whether or not to use text comparison. If <see cref="CompareMethod.Text"/>, this method uses text comparison; if <see cref="CompareMethod.Binary"/>, this method uses binary comparison.</param>
        /// <returns><see langword="true"/> if the strings match; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Like(this object? source, object? pattern, CompareMethod compareOption = CompareMethod.Binary)
            => LikeOperator.LikeObject(source, pattern, compareOption);
    }
}
