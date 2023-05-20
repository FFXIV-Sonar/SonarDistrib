using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Data
{
    [Flags]
    public enum MapFlagFormatFlags
    {
        // Default
        None = 0,

        // Flags
        Parenthesis = 1,
        ExtraSpaces = 2,
        ExtendedAccuracy = 4,
        IncludeZ = 8, // TODO

        // Presets
        SonarPreset = Parenthesis | ExtendedAccuracy,
        IngamePreset = Parenthesis | ExtraSpaces,
        SonarNoParenthesisPreset = ExtendedAccuracy,
        IngameNoParenthesisPreset = ExtraSpaces,
    }
}
