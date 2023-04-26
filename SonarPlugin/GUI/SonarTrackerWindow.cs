using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using DryIoc;
using DryIoc.FastExpressionCompiler.LightExpression;
using ImGuiNET;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.GeneratedSheets;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.GUI
{
    [SingletonService]
    public sealed class SonarTrackerWindow : Window, IDisposable
    {
        private readonly IndexSelectionWidget _indexWidget;

        private WindowSystem Windows { get; }
        private HuntTracker Hunts { get; }
        private FateTracker Fates { get; }

        public SonarTrackerWindow(WindowSystem windows, HuntTracker hunts, FateTracker fates, Container container) : base("Sonar Tracker (UNDER DEVELOPMENT!!!)") // TODO
        {
            this._indexWidget = container.Resolve<IndexSelectionWidget>();
            this.Windows = windows;
            this.Hunts = hunts;
            this.Fates = fates;
            this.Windows.AddWindow(this);
        }

        public override void Draw()
        {
            this._indexWidget.DrawBreadcrumb();
            ImGui.Text($"Index key: {this._indexWidget.IndexKey}");
            ImGui.Text($"Hunts: {this.Hunts.Data.GetIndexEntries(this._indexWidget.IndexKey).Count()} | Fates: {this.Fates.Data.GetIndexEntries(this._indexWidget.IndexKey).Count()}");
            ImGui.Spacing();
            ImGui.Text("Congratulations finding this!");
            ImGui.TextWrapped("Right now there's nothing useful in this window but I'll be working on this next release. In the meantime enjoy the filtering selector and hunt/fates counts");
        }

        public void Dispose()
        {
            this.Windows.RemoveWindow(this);
        }
    }
}
