using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using DryIoc;
using DryIoc.FastExpressionCompiler.LightExpression;
using ImGuiNET;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel.Sheets;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Relays;
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
        private IRelayTracker<HuntRelay> Hunts { get; }
        private IRelayTracker<FateRelay> Fates { get; }

        public SonarTrackerWindow(WindowSystem windows, IRelayTracker<HuntRelay> hunts, IRelayTracker<FateRelay> fates, Container container) : base("Sonar 트래커 (개발중!!!)") // TODO
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
            ImGui.Text($"마물: {this.Hunts.Data.GetIndexStates(this._indexWidget.IndexKey).Count()} | 돌발: {this.Fates.Data.GetIndexStates(this._indexWidget.IndexKey).Count()}");
            ImGui.Spacing();
            ImGui.Text("이 기능을 찾으신걸 축하드립니다!");
            ImGui.TextWrapped("현재 이 창에서 쓸만한 기능을 찾으실 수는 없겠지만 추후의 버전을 기약해 주세요.\n그동안은 필터링 선택기와 마물/돌발 개수 표시기를 즐겨주시길");
        }

        public void Dispose()
        {
            this.Windows.RemoveWindow(this);
        }
    }
}
