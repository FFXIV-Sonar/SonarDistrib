using Dalamud.Interface.Windowing;
using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using System.ComponentModel.Composition;
using DryIocAttributes;
using System.Threading;

namespace SonarDiagnostics.Dns
{
    [Export]
    [SingletonReuse]
    public sealed class DnsWindow : Window, IDisposable
    {
        private DnsWorker? _worker;
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
            var worker = this._worker;
            if (ImGui.Button("Perform DNS Tests"))
            {
                worker = new DnsWorker(this.Logger);
                this.ReplaceWorker(worker);
                _ = worker.CreateOrGetTask();
            }
            if (worker is not null)
            {
                ImGui.TextUnformatted(worker.Output);
            }
        }

        private void ReplaceWorker(DnsWorker? worker)
        {
            var oldWorker = Interlocked.Exchange(ref this._worker, worker);
            oldWorker?.Dispose();
        }

        public void Dispose()
        {
            this.Windows.RemoveWindow(this);
            this.ReplaceWorker(null);
        }
    }
}
