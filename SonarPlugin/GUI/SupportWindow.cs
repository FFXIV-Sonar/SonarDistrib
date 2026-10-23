using CheapLoc;
using Dalamud.Interface.Windowing;
using DryIoc.ImTools;
using Dalamud.Bindings.ImGui;
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
        public static SupportWindow CreateWindow(IWindowSystem windows, SonarClient client) => new(windows, client, Interlocked.Increment(ref s_nextId));

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

        private IWindowSystem Windows { get; }
        private SonarClient Client { get; }

        private SupportWindow(IWindowSystem windows, SonarClient client, int id) : base($"{Loc.Localize("SupportTitle", "Sonar Support")}##{id}")
        {
            this.Windows = windows;
            this.Client = client;
            this.modalTitleWithId = $"{Loc.Localize("SupportResultTitle", "Sonar Support Result")}##{id:X}";
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

            ImGui.Text(Loc.Localize("SupportRequiredHint", "* = required"));
            ImGui.Combo(Loc.Localize("SupportTypeLabel", "Type"), ref supportTypeIndex, GetSupportTypesStrings(SonarLanguage.ChineseSimplified), supportTypes.Length);
            ImGui.InputText($"{Loc.Localize("SupportContactLabel", "Contact")}{(this.Messaage.FromRequired ? "*" : string.Empty)}", ref contactText, SupportMessage.MaximumContactLength);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(Loc.Localize("SupportContactTooltip", "We cannot contact you in-game.\nProvide an external method of contact."));
            ImGui.InputText(Loc.Localize("SupportTitleFieldLabel", "Title"), ref titleText, SupportMessage.MaximumTitleLength);
            ImGui.InputTextMultiline($"{Loc.Localize("SupportBodyLabel", "Body")}*", ref bodyText, SupportMessage.MaximumContentLength, new(0, 0));
            ImGui.InputText($"{Loc.Localize("SupportPlayerNameLabel", "Player Name")}{(this.Messaage.PlayerRequired ? "*" : string.Empty)}", ref playerText, SupportMessage.MaximumPlayerNameLength);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{(this.Messaage.PlayerRequired ? Loc.Localize("SupportPlayerNameRequiredPrefix", "(Required) ") : string.Empty)}{Loc.Localize("SupportPlayerNameTooltip", "Provide character and world name")}");

            this.Messaage.Type = supportTypes[supportTypeIndex];
            this.Messaage.Contact = contactText;
            this.Messaage.Title = titleText;
            this.Messaage.Body = bodyText;
            this.Messaage.Player = playerText;

            if (ImGui.Button(Loc.Localize("SupportSend", "Send")))
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

            if (ImGui.Button(Loc.Localize("CommonCancel", "Cancel")))
            {
                this.IsOpen = false;
            }

            ImGui.Checkbox(Loc.Localize("SupportAddLogs", "Add Logs or Additional Text"), ref this._logsVisible);

            ImGui.EndGroup();
        }

        private void DrawLogs()
        {
            ImGui.BeginGroup();

            var logs = this.Messaage.Logs;
            ImGui.InputTextMultiline(Loc.Localize("SupportLogsLabel", "Logs"), ref logs, SupportMessage.MaximumLogsLength, new(0, 0));
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
                if (!string.IsNullOrWhiteSpace(this.responseException) && ImGui.CollapsingHeader(Loc.Localize("SupportExceptionDetails", "Exception details")))
                {
                    ImGui.Indent();
                    ImGui.TextUnformatted(this.responseException);
                    ImGui.Unindent();
                }
                if (ImGui.Button(Loc.Localize("CommonClose", "Close"))) this._responseVisible = false;
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
                supportTypesLanguageStrings[lang] = ret = lang is SonarLanguage.ChineseSimplified or SonarLanguage.ChineseTraditional ? new()
                {
                    { SupportType.Feedback, "反馈" },
                    { SupportType.Suggestion, "建议" },
                    { SupportType.BugReport, "Bug 反馈" },
                    { SupportType.Question, "提问" },
                    { SupportType.PlayerReport, "举报玩家" },
                    { SupportType.Appeal, "申诉" },
                } : new()
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
}
