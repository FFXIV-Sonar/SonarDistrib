using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;
using Loyc.Collections;
using Sonar.Enums;
using Sonar.Models;
using Sonar.Numerics;
using static SonarGUI.SupportWindowUtils;

namespace SonarGUI
{
    public sealed class SupportWindow : ISonarWindow
    {
        private static int s_nextId; // Interlocked

        private bool _visible;
        private bool _logsVisible;
        private bool _responseVisible;

        public string WindowId { get; }
        public string WindowTitle => "Sonar Support";

        private readonly string windowTitleWithId;
        private readonly string modalTitleWithId;

        private string? responseText;
        private string? responseException;

        public bool Visible
        {
            get => this._visible;
            internal set => this._visible = value; // Cannot be private because of OpenContactWindow extension
        }
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
        public bool Destroy => !this.Visible && this.responseText is null;
        public SupportMessage Messaage { get; } = new();

        private SonarGUIService SonarGUI { get; }

        public SupportWindow(SonarGUIService service)
        {
            this.SonarGUI = service;

            var id = Interlocked.Increment(ref s_nextId);
            this.WindowId = $"support-{id:X}";
            this.windowTitleWithId = $"Sonar Support##{id:X}";
            this.modalTitleWithId = $"Sonar Support Result##{id:X}";
        }

        public void Draw()
        {
            ImGui.SetNextWindowSize(new(0, 0));
            if (ImGui.Begin(this.windowTitleWithId, ref this._visible, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                this.DrawForm();
                if (this.AddLogs)
                {
                    ImGui.Separator();
                    this.DrawLogs();
                }
            }
            ImGui.End();
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

            ImGui.Text("* = required");
            ImGui.Combo("Type", ref supportTypeIndex, GetSupportTypesStrings(SonarLanguage.English), supportTypes.Length);
            ImGui.InputText($"Contact{(this.Messaage.FromRequired ? "*" : string.Empty)}", ref contactText, SupportMessage.MaximumContactLength);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("We cannot contact you in-game.\nProvide an external method of contact.");
            ImGui.InputText("Title", ref titleText, SupportMessage.MaximumTitleLength);
            ImGui.InputTextMultiline("Body*", ref bodyText, SupportMessage.MaximumContentLength, new(0, 0));
            ImGui.InputText($"Player Name{(this.Messaage.PlayerRequired ? "*" : string.Empty)}", ref playerText, SupportMessage.MaximumPlayerNameLength);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{(this.Messaage.PlayerRequired ? "(Required) " : string.Empty)}Provide character and world name");

            this.Messaage.Type = supportTypes[supportTypeIndex];
            this.Messaage.Contact = contactText;
            this.Messaage.Title = titleText;
            this.Messaage.Body = bodyText;
            this.Messaage.Player = playerText;

            if (ImGui.Button("Send"))
            {
                var logs = this.Messaage.Logs;
                if (!this.AddLogs) this.Messaage.Logs = string.Empty; // Respect user not wanting to add logs
                try
                {
                    this.SonarGUI.Client.Send(this.Messaage, this.ResultCallback);
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

            if (ImGui.Button("Cancel"))
            {
                this.Visible = false;
            }

            ImGui.Checkbox("Add Logs or Additional Text", ref this._logsVisible);

            ImGui.EndGroup();
        }

        private void DrawLogs()
        {
            ImGui.BeginGroup();

            var logs = this.Messaage.Logs;
            ImGui.InputTextMultiline("Logs", ref logs, SupportMessage.MaximumLogsLength, new(0, 0));
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
                if (ImGui.Button("Close")) this._responseVisible = false;
            }
            else
            {
                ImGui.SetWindowFocus(this.windowTitleWithId);
                ImGui.OpenPopup(this.modalTitleWithId);
            }
            ImGui.End();
        }

        public void ResultCallback(SupportResponse response)
        {
            if (response.Successful) this.Visible = false;
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
                    { SupportType.Feedback, "Feedback" },
                    { SupportType.Suggestion, "Suggestion" },
                    { SupportType.BugReport, "Bug Report" },
                    { SupportType.Question, "Question" },
                    { SupportType.PlayerReport, "Player Report" },
                    { SupportType.Appeal, "Appeal" },
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

    public static class SupportWindowExtensions
    {
        public static SupportWindow OpenSupportWindow(this SonarGUIService gui)
        {
            var window = new SupportWindow(gui)
            {
                Visible = true
            };
            gui.AddWindow(window);
            return window;
        }
    }
}