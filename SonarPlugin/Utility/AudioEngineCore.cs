using Dalamud.Logging;
using Dalamud.Plugin.Services;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SonarPlugin.Utility
{
    public sealed class AudioEngineCore : IDisposable
    {
        public static readonly WaveFormat Format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        private int _disposed;

        public IWavePlayer Player { get; }
        public MixingSampleProvider MixingProvider { get; }
        public VolumeSampleProvider VolumeProvider { get; }

        private IPluginLog Logger { get; }

        public AudioEngineCore(IPluginLog logger)
        {
            this.Logger = logger;

            this.MixingProvider = new(Format) { ReadFully = true };
            this.VolumeProvider = new(this.MixingProvider);
            this.Player = this.CreateWavePlayer();

            this.Player.Init(this.VolumeProvider);
            this.Player.Play();

            this.DisposingTask().ContinueWith(t => { /* Empty */ });
        }

        private async Task DisposingTask()
        {
            do
            {
                await Task.Delay(1000);
                this.Logger.Debug($"Mixer Inputs: {this.MixingProvider.MixerInputs.Count()}");
            }
            while (this.Player.PlaybackState != PlaybackState.Stopped && this.MixingProvider.MixerInputs.Any() && this._disposed == 0);
            this.Player.Dispose();
            this.Logger.Debug($"WavePlayer Disposed: {this.Player.GetType()}");
        }

        public void Dispose() => this._disposed = 1;

        private IWavePlayer CreateWavePlayer()
        {
            this.Logger.Debug($"Creating WavePlayer");
            IWavePlayer ret;
            try
            {
                this.Logger.Verbose($"Attempting to create WavePlayer using {nameof(WasapiOut)}");
                ret = new WasapiOut(AudioClientShareMode.Shared, 100);
            }
            catch (Exception ex1)
            {
                try
                {
                    this.Logger.Verbose($"Attempting to create WavePlayer using {nameof(DirectSoundOut)}");
                    ret = new DirectSoundOut(100);
                }
                catch (Exception ex2)
                {
                    try
                    {
                        this.Logger.Verbose($"Attempting to create WavePlayer using {nameof(WaveOutEvent)}");
                        ret = new WaveOutEvent();
                    }
                    catch (Exception ex3)
                    {
                        throw new AggregateException(ex1, ex2, ex3);
                    }
                }
            }
            ret.PlaybackStopped += this.PlaybackStoppedHandler;
            this.Logger.Debug($"WavePlayer type: {ret.GetType()}");
            return ret;
        }
        private void PlaybackStoppedHandler(object? _, StoppedEventArgs args) => this.PlaybackStopped?.Invoke(this, args.Exception);
        public event Action<AudioEngineCore, Exception?>? PlaybackStopped;
    }
}
