using AG.EnumLocalization;
using ImGuiNET;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SonarPlugin.GUI.Internal
{
    internal static class EnumWidgetHelper<T> where T : struct, Enum
    {
        public static readonly T[] s_values = [.. Enum.GetValues<T>()];
        [SuppressMessage("Major Code Smell", "S2743", Justification = "Intended.")]
        public static readonly string?[] s_strings = new string?[s_values.Length];
        public static readonly FrozenDictionary<T, int> s_indexes = Enumerable.Range(0, s_values.Length).Select(index => KeyValuePair.Create(s_values[index], index)).ToFrozenDictionary();

        /// <summary>ImGui combo with EnumLoc strings for <typeparamref name="T"/>.</summary>
        /// <param name="label">Combo label.</param>
        /// <param name="value"><typeparamref name="T"/> Value reference.</param>
        /// <param name="max_height">Max height. -1 = default.</param>
        /// <param name="updateStrings">Whether to call <see cref="UpdateStrings(string?)"/>.</param>
        /// <param name="langCode">Language code for <see cref="UpdateStrings(string?)"/>. Ignored if <paramref name="updateStrings"/> is <see langword="false"/>.</param>
        /// <returns>A value indicating <paramref name="value"/> has changed.</returns>
        public static bool Combo(string label, ref T value, int max_height = -1, bool updateStrings = true, string? langCode = null)
        {
            if (updateStrings) UpdateStrings(langCode);
            if (!s_indexes.TryGetValue(value, out var index)) index = -1;
            var result = ImGui.Combo(label, ref index, s_strings, s_strings.Length, max_height);
            if (result) value = s_values[index];
            return result;
        }

        public static void UpdateStrings(string? langCode = null)
        {
            for (var index = 0; index < s_values.Length; index++)
            {
                s_strings[index] = s_values[index].GetLocString(langCode);
            }
        }
    }
}
