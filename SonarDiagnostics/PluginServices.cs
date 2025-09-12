using Dalamud.IoC;
using Dalamud.Plugin.Services;
using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarDiagnostics
{
    public sealed class PluginServices
    {
        [PluginService] public IGameGui GameGui { get; private set; } = default!;
    }
}
