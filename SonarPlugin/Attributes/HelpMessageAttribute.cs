using System;

namespace SonarPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HelpMessageAttribute : Attribute
    {
        /// <summary>CheapLoc key of this help message.</summary>
        public string Key { get; }

        /// <summary>English fallback of this help message.</summary>
        public string HelpMessage { get; }

        public HelpMessageAttribute(string key, string helpMessage)
        {
            this.Key = key;
            this.HelpMessage = helpMessage;
        }
    }
}