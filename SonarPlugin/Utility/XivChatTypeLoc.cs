using Dalamud.Game.Text;
using System;
using System.Collections.Generic;

namespace SonarPlugin.Utility
{
    public static partial class XivChatTypeLoc
    {
        private static readonly Dictionary<XivChatType, string> s_defaults = []; // Make use of fancy or type's name

        /// <summary>Get the localized display name of a chat type, falling back to Dalamud's FancyName.</summary>
        public static string GetDisplayName(this XivChatType type)
        {
            var names = 
                CheapLocSetup.CurrentCode == "tw" ? s_twNames :
                CheapLocSetup.CurrentCode == "cn" ? s_cnNames :
                s_defaults;

            return names.GetValueOrDefault(type) ?? type.GetDetails()?.FancyName ?? type.ToString();
        }
    }
}