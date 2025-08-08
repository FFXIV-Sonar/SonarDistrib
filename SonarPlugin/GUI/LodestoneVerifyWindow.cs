using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Sonar;
using Sonar.Config;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Models;
using Sonar.Utilities;
using SonarPlugin.Config;
using SonarPlugin.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin.GUI
{
    public sealed class LodestoneVerifyWindow : Window, IHostedService
    {
        private LodestoneVerificationNeeded? _need;
        private LodestoneVerificationResult? _result;
        private double _requestTimestamp;

        private SonarMeta Meta => this.Client.Meta;
        private SonarConfig Config { get; }
        private SonarConfiguration Configuration { get; }
        private SonarClient Client { get; }
        private WindowSystem Windows { get; }

        [SuppressMessage("Critical Code Smell", "S3265", Justification = "Its a flag!")]
        public LodestoneVerifyWindow(SonarConfig config, SonarPlugin plugin, SonarClient client, WindowSystem windows) : base("Sonar Lodestone Verification")
        {
            this.Config = config;
            this.Configuration = plugin.Configuration;
            this.Client = client;
            this.Windows = windows;

            this.Flags |= ImGuiWindowFlags.NoSavedSettings;

            this.Size = new(320, 480);
            this.SizeCondition |= ImGuiCond.Appearing;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.Windows.AddWindow(this);
            this.Meta.VerificationNeeded += this.NeededHandler;
            this.Meta.VerificationResult += this.ResultHandler;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Windows.RemoveWindow(this);
            this.Meta.VerificationNeeded -= this.NeededHandler;
            this.Meta.VerificationResult -= this.ResultHandler;
            return Task.CompletedTask;
        }

        public override void PreOpenCheck()
        {
            if (this.ShouldSuppress()) this.OnClose();
        }

        private bool ShouldSuppress()
        {
            if (this._need is null || !this.Config.Contribute.Global) return true; // Nothing to show and no need to nag user
            if (this.Configuration.SuppressVerification is SuppressVerification.Always) return true;
            if (this.Configuration.SuppressVerification is SuppressVerification.UnlessRequired) return !this._need.Required;
            return false;
        }

        public override void Draw()
        {
            ImGui.TextWrapped("Lodestone Verification is needed for your currently logged in character:");
            ImGui.Indent(); this.DrawPlayerInfo(); ImGui.Unindent();

            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            this.DrawReason();

            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            this.DrawVerification();

            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.TextWrapped("You can suppress this dialog at /sonarconfig General tab, Lodestone Settings.");
        }

        private void DrawPlayerInfo()
        {
            Debug.Assert(this._need is not null);

            ImGui.TextUnformatted($"{this.Meta.PlayerInfo}");
            if (this._need.LodestoneId != -1)
            {
                ImGui.SameLine();
                ImGui.TextUnformatted($" (ID: {this._need.LodestoneId})");
            }
        }

        [SuppressMessage("Minor Code Smell", "S1075", Justification = "Well-known URLs")]
        private void DrawReason()
        {
            Debug.Assert(this._need is not null);

            switch (this._need.Reason)
            {
                case LodestoneVerificationReason.Unknown:
                    {
                        ImGui.TextWrapped("Unspecified Reason.");
                        ImGui.Spacing();
                        ImGui.TextWrapped("Please contact Sonar support for assistance.");
                    }
                    break;

                case LodestoneVerificationReason.NotVerified:
                    {
                        ImGui.TextWrapped("Your character is currently not verified.");
                    }
                    break;

                case LodestoneVerificationReason.NotFound:
                    {
                        ImGui.TextWrapped("Your character lodestone profile is not found.");
                        ImGui.Spacing();
                        ImGui.TextWrapped("Make your character's Lodestone profile searchable and public. You can set it back to private after verification is done.");
                    }
                    break;

                case LodestoneVerificationReason.PrivateProfile:
                    {
                        ImGui.TextWrapped("Your character lodestone profile is private.");
                        ImGui.Spacing();
                        ImGui.TextWrapped("Make your Lodestone profile public to be able to verify. You can set it back to private after verification is done.");
                    }
                    break;

                case LodestoneVerificationReason.Renamed:
                    {
                        ImGui.TextWrapped("Your character has been renamed and your character's lodestone profile is private.");
                        ImGui.Spacing();
                        ImGui.TextWrapped("Make your Lodestone profile public to be able to verify. You can set it back to private after verification is done.");
                    }
                    break;

                case LodestoneVerificationReason.HashMismatch:
                    {
                        ImGui.TextWrapped("Your character hash mismatch and your character's lodestone profile is private.");
                        ImGui.Spacing();
                        ImGui.TextWrapped("Make your Lodestone profile public to be able to verify. You can set it back to private after verification is done.");
                    }
                    break;

                case LodestoneVerificationReason.Stale:
                    {
                        ImGui.TextWrapped("Your character lodestone information stored at Sonar is stale.");
                        ImGui.Spacing();
                        ImGui.TextWrapped("Make your Lodestone profile public to be able to verify. You can set it back to private after verification is done.");
                    }
                    break;

                default:
                    {
                        ImGui.TextWrapped("Unable to provide reason.");
                        ImGui.Spacing();
                        ImGui.TextWrapped("Update your Sonar plugin or contact support for assistance.");
                    }
                    break;
            }

            if (this._need.Code is not null)
            {
                ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
                ImGui.TextWrapped("Add the following code into your chracter's lodestone profile bio:");
                ImGui.Indent();
                ImGui.TextUnformatted(this._need.Code);
                ImGui.SameLine();
                if (ImGui.Button("Copy")) ImGui.SetClipboardText(this._need.Code);
                ImGui.Unindent();
            }

            var world = this.Meta.PlayerInfo?.GetHomeWorld();
            var baseUrl = (world?.RegionId ?? 0) switch
            {
                1 => "https://jp.finalfantasyxiv.com/lodestone",
                3 => "https://eu.finalfantasyxiv.com",
                _ => "https://na.finalfantasyxiv.com",
            };


            if (this._need.LodestoneId !=-1)
            {
                if (ImGui.Button("Open Lodestone Profile")) _ = Task.Run(() => ShellUtils.ShellExecute($"{baseUrl}/character/{this._need.LodestoneId}/"));
                ImGui.SameLine();
            }
            if (ImGui.Button("Open Lodestone Website")) _ = Task.Run(() => ShellUtils.ShellExecute($"{baseUrl}"));
        }

        private void DrawVerification()
        {
            Debug.Assert(this._need is not null);

            var curTime = UnixTimeHelper.SyncedUnixNow;
            var reqTime = this._need.RequiredTime;

            if (this._need.Required) ImGui.TextWrapped("This verification is a requirement.");
            else if (reqTime > 0)
            {
                var remTime = reqTime - curTime;
                if (remTime < SonarConstants.EarthMinute * 1) // This is to avoid confusion of being allowed a few seconds (might still cause confusion anyway....)
                {
                    ImGui.TextWrapped("This verification is a requirement.");
                }
                else if (remTime > SonarConstants.EarthDay * 2)
                {
                    ImGui.TextWrapped($"This verification will become a requirement in {(int)(remTime / SonarConstants.EarthDay)} days.");
                }
                else if (remTime > SonarConstants.EarthHour * 1)
                {
                    ImGui.TextWrapped($"This verification will become a requirement in {(int)(remTime / SonarConstants.EarthHour)} hours.");
                }
                else if (remTime > SonarConstants.EarthMinute * 5)
                {
                    ImGui.TextWrapped($"This verification will become a requirement in {(int)(remTime / SonarConstants.EarthMinute)} minutes.");
                }
                else
                {
                    ImGui.TextWrapped("This verification will become a requirement in less than 5 minutes.");
                }
            }
            else
            {
                ImGui.TextWrapped("This verification is not a requirement at this time.");
            }

            if (ImGui.Button("Verify"))
            {
                this.Meta.RequestVerification();
                this._requestTimestamp = curTime;
            }

            if (this._requestTimestamp > 0)
            {
                ImGui.SameLine();
                if (this._result is null)
                {
                    var runTime = curTime - this._requestTimestamp;
                    if (runTime > SonarConstants.EarthSecond * 30)
                    {
                        ImGui.TextWrapped("Verification timeout");
                    }
                    else
                    {
                        var dots = new string(Enumerable.Repeat('.', ((int)(runTime / SonarConstants.EarthSecond * 3) % 3) + 1).ToArray());
                        ImGui.TextWrapped($"Verification in progress{dots}");
                    }
                }
                else
                {
                    ImGui.TextWrapped("Verification failed"); // Window wouldn't be visible if succeded
                }
            }
            ImGui.TextWrapped("You'll only be able to use Sonar in local mode if not verified by the time this becomes a requirement.");
        }

        private void NeededHandler(SonarMeta _, LodestoneVerificationNeeded need)
        {
            this._need = need;
            this.IsOpen = true;
        }

        private void ResultHandler(SonarMeta _, LodestoneVerificationResult result)
        {
            this._result = result;
            if (this._result.Success) this.OnClose();
        }

        public override void OnClose()
        {
            // Poof and reset all variables
            this.IsOpen = false; // This is here because its sometimes called manually
            this._need = null;
            this._result = null;
            this._requestTimestamp = 0;

            base.OnClose();
        }

        public override void OnOpen()
        {
            // Make sure its brought to front
            this.BringToFront();

            base.OnOpen();
        }
    }
}
