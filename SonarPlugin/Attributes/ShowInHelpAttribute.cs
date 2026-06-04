using System;

namespace SonarPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ShowInHelpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DoNotShowInHelpAttribute : Attribute
    {
        // this is so i know
    }
}