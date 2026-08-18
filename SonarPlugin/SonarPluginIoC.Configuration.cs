using Microsoft.Extensions.Logging;
using SonarPlugin.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarPlugin
{
    public sealed partial class SonarPluginIoC
    {
        private readonly SemaphoreSlim _configurationSemaphore = new(0, 1);
        private readonly Lock _configurationLock = new();
        private readonly Task _configurationTask;

        private async Task ConfigurationTask(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await this._configurationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    this.SaveConfigurationCore();
                }
                catch (ObjectDisposedException) { return; }
                catch (OperationCanceledException) { return; }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Exception while saving configuration");
                }
            }
        }


        [SuppressMessage("Major Code Smell", "S112", Justification = "No suitable exception")]
        public void LoadConfiguration(bool isReset = false)
        {
            try
            {
                var configuration = (SonarConfiguration?)this.PluginInterface.GetPluginConfig();
                if (configuration is null)
                {
                    if (isReset) throw new Exception("Failed resetting configuration");
                    this.ResetConfiguration();
                    return;
                }
                this.Configuration = configuration;

                this.Configuration.Sanitize();
                this.Client.Configuration.ReadFrom(this.Configuration.SonarConfig);
                this.Configuration.SonarConfig = this.Client.Configuration;

                if (this.Configuration.PerformVersionUpdate(this.Logger))
                {
                    this.SaveConfiguration(true);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to load configuration");
                if (!isReset) this.ResetConfiguration();
            }
        }

        public void SaveConfiguration(bool updateServer = false)
        {
            try
            {
                this._configurationSemaphore.Release();
            }
            catch (SemaphoreFullException) { /* Swallow */ }
        }

        private void SaveConfigurationCore()
        {
            using var scope = this._configurationLock.EnterScope();
            try
            {
                this.Configuration.SonarConfig = this.Client.Configuration;
                this.PluginInterface.SavePluginConfig(this.Configuration);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to save configuration");
            }
        }

        public void ResetConfiguration()
        {
            this.Configuration = new SonarConfiguration();
            this.Client.Configuration.ReadFrom(this.Configuration.SonarConfig);
            this.Configuration.SonarConfig = this.Client.Configuration;
            this.Client.Configuration.Contribute.Reset();
            this.SaveConfiguration(true);
            this.LoadConfiguration(true);
        }
    }
}
