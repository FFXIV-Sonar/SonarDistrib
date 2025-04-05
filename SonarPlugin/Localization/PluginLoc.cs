using AG.EnumLocalization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Localization
{
    [EnumLocStrings("Plugin")]
    public enum PluginLoc
    {
        #region MainWindow
        [EnumLoc(Fallback = "Sonar")]
        MainWindowTitle,
        #endregion
    }
}
