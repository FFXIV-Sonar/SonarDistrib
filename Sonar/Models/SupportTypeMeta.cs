using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class SupportTypeMetaAttribute : Attribute
    {
        public bool RequireContact { get; set; }
        public bool RequirePlayerName { get; set; }
    }
}
