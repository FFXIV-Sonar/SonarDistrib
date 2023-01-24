using Dalamud.Game.Text;
using System.Reflection;

namespace SonarPlugin.Utility
{
    public static class XivChatTypeUtils
    {
        public static XivChatType GetValueFromInfoAttribute(string name)
        {
            var type = typeof(XivChatType);
            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<XivChatTypeInfoAttribute>();
                if (attribute != null)
                {
                    if (attribute.FancyName == name)
                        return (XivChatType)field.GetValue(null)!;
                }
                else
                {
                    if (field.Name == name)
                        return (XivChatType)field.GetValue(null)!;
                }
            }
            return default;
        }
    }
}