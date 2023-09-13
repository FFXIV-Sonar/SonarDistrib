using Dalamud.Game.Text;
using System;
using System.Collections.Generic;

namespace SonarPlugin.Utility
{
    public static class XivChatTypeUtils
    {
        private static IReadOnlyDictionary<string, XivChatType>? s_chatTypes;
        public static IReadOnlyDictionary<string, XivChatType> ChatTypes
        {
            get
            {
                if (s_chatTypes is null)
                {
                    var dict = new Dictionary<string, XivChatType>(comparer: StringComparer.InvariantCultureIgnoreCase);
                    foreach (var value in Enum.GetValues<XivChatType>())
                    {
                        dict.TryAdd(value.ToString(), value);
                    }
                    foreach (var value in Enum.GetValues<XivChatType>())
                    {
                        var details = value.GetDetails();
                        if (details is null) continue;
                        dict.TryAdd(details.FancyName, value);
                    }
                    s_chatTypes = dict;
                }
                return s_chatTypes;
            }
        }
        public static XivChatType GetValueFromInfoAttribute(string name) => ChatTypes.GetValueOrDefault(name);

    }
}