using AG.EnumLocalization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Enums
{
    [EnumLocStrings("BuildEnvironment")]
    public enum SonarBuildEnvironment
    {
        Debug,

        Release
    }
}
