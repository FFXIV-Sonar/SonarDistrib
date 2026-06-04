using System;

namespace SonarPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        public string Command { get; }

        public CommandAttribute(string command)
        {
            this.Command = command;
        }
    }
}