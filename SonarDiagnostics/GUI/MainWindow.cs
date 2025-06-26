using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DryIocAttributes;
using ImGuiNET;
using SonarDiagnostics.Dns;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarDiagnostics.GUI
{
    [Export]
    [SingletonReuse]
    public sealed class MainWindow : Window, IDisposable
    {
        private Plugin Plugin { get; }
        private Lazy<DnsWindow> DnsWindow { get; }
        private WindowSystem Windows { get; }
        private IPluginLog Logger { get; }

        public MainWindow(Plugin plugin, Lazy<DnsWindow> dnsWindow, WindowSystem windows, IPluginLog logger) : base("Sonar Disgnostics")
        {
            this.Plugin = plugin;
            this.DnsWindow = dnsWindow;
            this.Windows = windows;
            this.Logger = logger;

            this.Windows.AddWindow(this);

            this.Size = new(320, 200);
            this.SizeCondition = ImGuiCond.FirstUseEver;
        }

        public override void Draw()
        {
            if (ImGui.Button("DNS Tests")) this.DnsWindow.Value.Toggle();
            ImGui.Separator();

            using (ImRaii.Disabled(this.Plugin.LogPath is null || !File.Exists(this.Plugin.LogPath)))
            {
                var logFile = this.Plugin.LogPath;
                if (ImGui.Button("Open Log File"))
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = logFile,
                        UseShellExecute = true,
                    };
                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error(ex, "Exception opening log file");
                    }
                }

                ImGui.SameLine();

                var logDir = Path.GetDirectoryName(logFile);
                if (ImGui.Button("Open Logs Directory"))
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = logDir,
                        UseShellExecute = true,
                    };
                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error(ex, "Exception opening logs directory");
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Windows.RemoveWindow(this);
        }
    }
}
