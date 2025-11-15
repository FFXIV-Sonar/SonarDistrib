using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.GUI.Internal
{
    public sealed record FileDialogResult(string Id, string Path, /* Localization export only */ bool Fallbacks = false);
}
