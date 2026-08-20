using Dalamud.Game;
using DryIoc;
using Microsoft.Extensions.Logging;
using Sonar;
using Sonar.Data;
using Sonar.Enums;
using Sonar.Logging;
using SonarPlugin.Utility;
using SonarUtils.Secrets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace SonarPlugin
{
    public sealed partial class SonarPluginIoC
    {
        private SonarClient CreateClient()
        {
            this.Logger.LogInformation("Initializing Sonar");
            var startInfo = new SonarStartInfo()
            {
                WorkingDirectory = Path.Join(this.PluginInterface.GetPluginConfigDirectory(), "Sonar"),
                PluginSecretMeta = SecretUtils.GetSecretMetaBytes(typeof(SonarPluginIoC).Assembly),
                ChallengeHandler = this.ChallengeHandlerAsync
            };

            SonarLanguage DetermineLanguage(int num)
            {
                var name = Enum.GetName((ClientLanguage)num);
                if (name is "Korean") return SonarLanguage.Korean;
                if (name is "ChineseSimplified") return SonarLanguage.ChineseSimplified;
                if (name is "ChineseTraditional") return SonarLanguage.ChineseSimplified; // TODO: Change to .ChineseTraditional once done

                this.Logger.LogWarning("Unable to determine ClientLanguage: {num}", num);
                return
                    num is 4 ? SonarLanguage.ChineseSimplified :
                    num is 5 ? SonarLanguage.ChineseSimplified : // TODO: Change to .ChineseTraditional once done
                    SonarLanguage.English;
            }

            var versionInfo = VersionUtils.GetSonarVersionModel(this.Data, this.PluginInterface, this.DalamudVersion);
            var client = new SonarClient(startInfo) { VersionInfo = versionInfo };
            Database.DefaultLanguage = this.Data.Language switch
            {
                ClientLanguage.Japanese => SonarLanguage.Japanese,
                ClientLanguage.English => SonarLanguage.English,
                ClientLanguage.German => SonarLanguage.German,
                ClientLanguage.French => SonarLanguage.French,
                _ => DetermineLanguage((int)this.Data.Language), // https://github.com/ottercorp/Dalamud/blob/cn/Dalamud/ClientLanguage.cs#L31 https://github.com/yanmucorp/Dalamud/blob/master/Dalamud/Game/ClientLanguage.cs#L36
            };
            return client;
        }

        private void InitializeClient()
        {
            this.Client.ServerMessage += this.Events_OnSonarMessage;
            this.Client.LogMessage += this.ClientLogHandler;
        }

        private void DeinitializeClient()
        {
            this.Client.ServerMessage -= this.Events_OnSonarMessage;
            this.Client.LogMessage -= this.ClientLogHandler;
        }

        private void Events_OnSonarMessage(SonarClient source, string? message)
        {
            if (message is null) return;
            this.Chat.Print(new()
            {
                Type = this.Configuration.HuntOutputChannel,
                Name = "Sonar",
                Message = message
            });
            this.Logger.LogInformation("Sonar Message Received: {message}", message);
        }

        private void ClientLogHandler(SonarClient source, SonarLogMessage log) => this.LogHandler(log);

        [SuppressMessage("Minor Code Smell", "S3458", Justification = "Clarity")]
        private void LogHandler(SonarLogMessage log)
        {
            var (level, message) = (log.Level, log.Message);
            var logLevel = level switch
            {
                SonarLogLevel.Verbose => LogLevel.Trace,
                SonarLogLevel.Debug => LogLevel.Debug,
                SonarLogLevel.Information => LogLevel.Information,
                SonarLogLevel.Warning => LogLevel.Warning,
                SonarLogLevel.Error => LogLevel.Error,
                SonarLogLevel.Fatal => LogLevel.Critical,
                _ => LogLevel.Critical,
            };
            this.Logger.Log(logLevel, "{message}", message);
        }
    }
}
