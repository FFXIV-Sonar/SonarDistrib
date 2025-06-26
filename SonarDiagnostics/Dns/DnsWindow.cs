using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using DnsClient;
using SonarUtils;
using AG;
using FFXIVClientStructs;
using Dalamud.Plugin.Services;
using System.Threading;
using System.ComponentModel.Composition;
using DryIocAttributes;
using Dalamud.Interface.Utility.Raii;
using System.Net;
using System.Diagnostics;

namespace SonarDiagnostics.Dns
{
    [Export]
    [SingletonReuse]
    public sealed class DnsWindow : Window, IDisposable
    {
        private List<NameServer>? _nameservers;
        private string[]? _nameserverStrings;

        private int _index;
        private DnsWorker? _worker;
        private string _customDns = string.Empty;

        private WindowSystem Windows { get; }
        private IPluginLog Logger { get; }

        public DnsWindow(WindowSystem windows, IPluginLog logger) : base("DNS Tests")
        {
            this.Windows = windows;
            this.Logger = logger;

            this.DiscoverNameServers();

            this.Windows.AddWindow(this);

            this.Size = new(400, 400);
            this.SizeCondition = ImGuiCond.FirstUseEver;
        }

        public override void Draw()
        {
            if (ImGui.Button("Rediscover name servers"))
            {
                this.DiscoverNameServers();
                this._index = 0;
                this._worker = null;
            }
            ImGui.InputText(string.Empty, ref this._customDns, 256);
            if (!IPEndPoint.TryParse(this._customDns, out var endpoint) && IPAddress.TryParse(this._customDns, out var address))
            {
                endpoint = new(address, 53);
            }
            if (endpoint?.Port is 0) endpoint.Port = 53;

            using (ImRaii.Disabled(endpoint is null))
            {
                ImGui.SameLine();
                if (ImGui.Button("Add Custom DNS"))
                {
                    Debug.Assert(endpoint is not null); // Button would be disabled otherwise
                    (this._nameservers ??= []).Add(new(endpoint));
                    this._nameserverStrings = [.. this._nameservers.Select(ns => ns.ToString()).Prepend("None")];
                }
            }

            if (this._nameservers is not null && this._nameserverStrings is not null && this._nameservers.Count > 0)
            {
                if (ImGui.ListBox("Name Servers###nameservers", ref this._index, this._nameserverStrings, this._nameserverStrings.Length))
                {
                    if (this._index is 0) this._worker = null;
                    else this.CreateWorker(this._nameservers[this._index - 1]);
                }
            }
            else
            {
                ImGui.Text("No name servers found");
            }

            using (ImRaii.Disabled(this._worker is null || this._worker.IsRunning))
            {
                if (ImGui.Button("Run tests"))
                {
                    _ = this._worker?.RunAsync();
                }
            }
            if (this._worker?.IsRunning is true)
            {
                ImGui.SameLine();
                ImGui.Text("Running...");
            }

            ImGui.Separator();

            if (this._worker is not null)
            {
                foreach (var output in this._worker.Output)
                {
                    ImGui.TextUnformatted(output);
                }
            }
        }

        public void RunTest()
        {
            _ = this._worker?.RunAsync();
        }

        public void DiscoverNameServers()
        {
            try
            {
                this._nameservers = [.. DnsUtils.DiscoverNameservers(null)];
                this._nameserverStrings = [.. this._nameservers.Select(ns => ns.ToString()).Prepend("None")];
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Exception discovering name servers");
            }
        }

        public void CreateWorker(NameServer nameserver)
        {
            try
            {
                this._worker = new DnsWorker(nameserver, this.Logger);
            }
            catch (Exception ex)
            {
                this._worker = null;
                this.Logger.Error(ex, "Exception creating DNS Client");
            }
        }

        public void Dispose()
        {
            this.Windows.RemoveWindow(this);
        }
    }
}
