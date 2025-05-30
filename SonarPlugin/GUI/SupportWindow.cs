using Dalamud.Interface.Windowing;
using DryIoc.ImTools;
using ImGuiNET;
using Sonar;
using Sonar.Enums;
using Sonar.Models;
using Sonar.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SonarPlugin.GUI.SupportWindowUtils;

namespace SonarPlugin.GUI
{
    public sealed class SupportWindow : Window
    {
        private static int s_nextId; // Interlocked
        public static SupportWindow CreateWindow(WindowSystem windows, SonarClient client) => new(windows, client, Interlocked.Increment(ref s_nextId));

        private bool _logsVisible;
        private bool _responseVisible;

        private readonly string modalTitleWithId;

        private string? responseText;
        private string? responseException;

        public bool AddLogs
        {
            get => this._logsVisible;
            private set => this._logsVisible = value;
        }
        public bool ResponseVisible
        {
            get => this._responseVisible;
            private set => this._responseVisible = value;
        }
        public SupportMessage Messaage { get; } = new();

        private WindowSystem Windows { get; }
        private SonarClient Client { get; }

        private SupportWindow(WindowSystem windows, SonarClient client, int id) : base($"Sonar Support##{id}")
        {
            this.Windows = windows;
            this.Client = client;
            this.modalTitleWithId = $"Sonar Support 결과##{id:X}";
            this.Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings;
            this.Size = new(0, 0);

            this.Windows.AddWindow(this);
            this.IsOpen = true;
        }

        public override void OnClose()
        {
            this.Windows.RemoveWindow(this);
        }

        public override void Draw()
        {
            this.DrawForm();
            if (this.AddLogs)
            {
                ImGui.Separator();
                this.DrawLogs();
            }
       }

        public override void PostDraw()
        {
            this.DrawResponseWindow();
        }

        private void DrawForm()
        {
            ImGui.BeginGroup();

            var supportTypes = GetSupportTypes();
            var supportTypeIndex = supportTypes.IndexOf(this.Messaage.Type).Max(0);
            var contactText = this.Messaage.Contact ?? string.Empty;
            var titleText = this.Messaage.Title ?? string.Empty;
            var bodyText = this.Messaage.Body ?? string.Empty;
            var playerText = this.Messaage.Player ?? string.Empty;

            ImGui.Text("* - 필수 작성");
            ImGui.Combo("분류", ref supportTypeIndex, GetSupportTypesStrings(SonarLanguage.English), supportTypes.Length);
            ImGui.InputText($"연락처{(this.Messaage.FromRequired ? "*" : string.Empty)}", ref contactText, SupportMessage.MaximumContactLength);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("인게임에서 연락할 수는 없으므로\n게임 외부에서 연락 가능한 연락처를 작성해 주세요.");
            ImGui.InputText("제목", ref titleText, SupportMessage.MaximumTitleLength);
            ImGui.InputTextMultiline("내용*", ref bodyText, SupportMessage.MaximumContentLength, new(0, 0));
            ImGui.InputText($"플레이어 이름{(this.Messaage.PlayerRequired ? "*" : string.Empty)}", ref playerText, SupportMessage.MaximumPlayerNameLength);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{(this.Messaage.PlayerRequired ? "(필수) " : string.Empty)}캐릭터 및 서버 이름을 작성해 주세요");

            this.Messaage.Type = supportTypes[supportTypeIndex];
            this.Messaage.Contact = contactText;
            this.Messaage.Title = titleText;
            this.Messaage.Body = bodyText;
            this.Messaage.Player = playerText;

            if (ImGui.Button("전송"))
            {
                var logs = this.Messaage.Logs;
                if (!this.AddLogs) this.Messaage.Logs = string.Empty; // Respect user not wanting to add logs
                try
                {
                    this.Client.ContactSupport(this.Messaage, this.ResultCallback);
                }
                catch (Exception ex)
                {
                    this.responseText = ex.Message;
                    this.responseException = ex is not SupportMessageException ? $"{ex}" : null;
                    this.ResponseVisible = true;
                }
                this.Messaage.Logs = logs;
            }

            ImGui.SameLine();

            if (ImGui.Button("취소"))
            {
                this.IsOpen = false;
            }

            ImGui.Checkbox("로그 첨부 혹은 추가 내용 작성", ref this._logsVisible);

            ImGui.EndGroup();
        }

        private void DrawLogs()
        {
            ImGui.BeginGroup();

            var logs = this.Messaage.Logs;
            ImGui.InputTextMultiline("로그", ref logs, SupportMessage.MaximumLogsLength, new(0, 0));
            this.Messaage.Logs = logs;

            ImGui.EndGroup();
        }

        private void DrawResponseWindow()
        {
            if (!this._responseVisible) return;

            ImGui.SetNextWindowFocus();
            if (ImGui.BeginPopupModal(this.modalTitleWithId, ref this._responseVisible, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                ImGui.Spacing();
                ImGui.TextUnformatted(this.responseText);
                ImGui.Spacing();
                if (!string.IsNullOrWhiteSpace(this.responseException) && ImGui.CollapsingHeader("Exception details"))
                {
                    ImGui.Indent();
                    ImGui.TextUnformatted(this.responseException);
                    ImGui.Unindent();
                }
                if (ImGui.Button("닫기")) this._responseVisible = false;
            }
            else
            {
                ImGui.SetWindowFocus(this.WindowName);
                ImGui.OpenPopup(this.modalTitleWithId);
            }
            ImGui.End();
        }

        public void ResultCallback(SupportResponse response)
        {
            if (response.Successful) this.IsOpen = false;
            this.responseText = response.Message;
            this.responseException = response.Exception;
            this._responseVisible = !string.IsNullOrWhiteSpace(this.responseText);
        }
    }

    internal static class SupportWindowUtils
    {
        private static readonly Dictionary<SonarLanguage, Dictionary<SupportType, string>> supportTypesLanguageStrings = new();
        private static readonly Dictionary<SonarLanguage, string[]> supportTypeLanguageStringsArrays = new();
        private static readonly SupportType[] supportTypes = Enum.GetValues<SupportType>().Where(t => t != SupportType.Unspecified).OrderBy(t => t).ToArray();

        public static Dictionary<SupportType, string> GetSupportTypes(SonarLanguage lang)
        {
            if (!supportTypesLanguageStrings.TryGetValue(lang, out var ret))
            {
                supportTypesLanguageStrings[lang] = ret = new()
                {
                    { SupportType.Feedback, "피드백" },
                    { SupportType.Suggestion, "제안" },
                    { SupportType.BugReport, "버그 신고" },
                    { SupportType.Question, "질문" },
                    { SupportType.PlayerReport, "플레이어 신고" },
                    { SupportType.Appeal, "항소" },
                };
            }
            return ret;
        }

        public static SupportType[] GetSupportTypes() => supportTypes;

        public static string[] GetSupportTypesStrings(SonarLanguage lang)
        {
            if (!supportTypeLanguageStringsArrays.TryGetValue(lang, out var ret))
            {
                supportTypeLanguageStringsArrays[lang] = ret = GetSupportTypes(lang).OrderBy(t => t.Key).Select(t => t.Value).ToArray();
            }
            return ret;
        }
    }
}
