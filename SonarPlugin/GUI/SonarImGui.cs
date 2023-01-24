using System;
using ImGuiNET;

namespace SonarPlugin.GUI
{
    public static class SonarImGui
    {
        public static float ScaledFloat(float value)
        {
            var scale = ImGui.GetIO().FontGlobalScale;
            return value * scale;
        }

        public static bool SliderInt(string label, int value, int min, int max, Action<int> onChange)
        {
            if (ImGui.SliderInt(label, ref value, min, max))
            {
                onChange?.Invoke(value);
                return true;
            }
            return false;
        }

        public static bool SliderInt(string label, int value, int min, int max, string format, Action<int> onChange)
        {
            if (ImGui.SliderInt(label, ref value, min, max, format))
            {
                onChange?.Invoke(value);
                return true;
            }
            return false;
        }

        public static bool SliderInt(string label, int value, int min, int max, string format, ImGuiSliderFlags flags, Action<int> onChange)
        {
            if (ImGui.SliderInt(label, ref value, min, max, format, flags))
            {
                onChange?.Invoke(value);
                return true;
            }
            return false;
        }

        public static bool SliderFloat(string label, float value, float min, float max, Action<float> onChange)
        {
            if (ImGui.SliderFloat(label, ref value, min, max))
            {
                onChange?.Invoke(value);
                return true;
            }
            return false;
        }

        public static bool SliderFloat(string label, float value, float min, float max, string format, Action<float> onChange)
        {
            if (ImGui.SliderFloat(label, ref value, min, max, format))
            {
                onChange?.Invoke(value);
                return true;
            }
            return false;
        }

        public static bool SliderFloat(string label, float value, float min, float max, string format, ImGuiSliderFlags flags, Action<float> onChange)
        {
            if (ImGui.SliderFloat(label, ref value, min, max, format, flags))
            {
                onChange?.Invoke(value);
                return true;
            }
            return false;
        }

        public static bool Checkbox(string label, bool value, Action<bool> onChange)
        {
            if (ImGui.Checkbox(label, ref value))
            {
                onChange?.Invoke(value);
                return true;
            }
            return false;
        }

        public static bool Combo(string label, int current_item, string[] items, Action<int> onChange)
        {
            if (ImGui.Combo(label, ref current_item, items, items.Length))
            {
                onChange?.Invoke(current_item);
                return true;
            }
            return false;
        }
    }
}
