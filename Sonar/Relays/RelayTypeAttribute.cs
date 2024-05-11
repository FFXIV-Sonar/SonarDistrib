using System;

namespace Sonar.Relays
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class RelayTypeAttribute : Attribute
    {
        public RelayType Type { get; set; }

        public RelayTypeAttribute(RelayType type)
        {
            this.Type = type;
        }

        public override string ToString() => this.Type.ToString();
    }
}
