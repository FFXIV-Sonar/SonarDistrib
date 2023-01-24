using System;

namespace SonarPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ShowInHelpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotShowInHelpAttribute : Attribute
    {
        // this is so i know
    }
}