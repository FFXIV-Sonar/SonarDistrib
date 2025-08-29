using Dalamud.Interface.Windowing;
using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace SonarDiagnostics.Dns
{
    [Export]
    [SingletonReuse]
    public sealed class DnsWindow : Window, IDisposable
    {
        private WindowSystem Windows { get; }
        private IPluginLog Logger { get; }

        public DnsWindow(WindowSystem windows, IPluginLog logger) : base("DNS Tests")
        {
            this.Windows = windows;
            this.Logger = logger;

            this.Windows.AddWindow(this);

            this.Size = new(400, 400);
            this.SizeCondition = ImGuiCond.FirstUseEver;
        }

        public override void Draw()
        {
            ImGui.Text("Temporarily removed");
        }

        public void Dispose()
        {
            this.Windows.RemoveWindow(this);
        }
    }
}
