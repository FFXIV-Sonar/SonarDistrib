using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NAudio.Wave;
using SonarPlugin.NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Dalamud.Logging;
using Sonar.Threading;

namespace SonarPlugin.Utility
{
    [SingletonService]
    public sealed class AudioPlaybackEngine : IDisposable
    {
        private readonly Dictionary<string, byte[]?> _cache = new();
        private readonly ResettableLazy<AudioEngineCore> _core;
        private AudioEngineCore Core => this._core.Value;

        public AudioPlaybackEngine()
        {
            this._core = new(this.CreateEngineCore);
        }

        private float _volume = 1.0f;
        public float Volume
        {
            get => this._volume;
            set
            {
                this._volume = value;
                this.Core.VolumeProvider.Volume = value;
            }
        }

        private AudioEngineCore CreateEngineCore()
        {
            var ret = new AudioEngineCore();
            ret.VolumeProvider.Volume = this.Volume;
            ret.PlaybackStopped += this.PlaybackStoppedHandler;
            return ret;
        }

        private void PlaybackStoppedHandler(AudioEngineCore core, Exception? ex)
        {
            if (this._core.IsValueCreated && this._core.Value == core) this._core.Reset();
            core.PlaybackStopped -= this.PlaybackStoppedHandler;
            if (ex is not null) PluginLog.LogDebug(ex, string.Empty);
            core.Dispose();
        }

        public void PlaySound(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return;
            var source = this.LoadSound(filename);
            if (source is null) return;
            try
            {
                this.Core.MixingProvider.AddMixerInput(new RawSourceWaveStream(source, 0, source.Length, AudioEngineCore.Format).ToSampleProvider());
            }
            catch (Exception ex)
            {
                PluginLog.LogError($"Exception playing sound\n{ex}");
            }
        }

        private byte[]? LoadSound(string filename)
        {
            if (!this._cache.TryGetValue(filename, out var source))
            {
                if (File.Exists(filename))
                {
                    source = this.GetWaveBytesFromFile(filename);
                }
                else
                {
                    source = this.GetWaveBytesFromResource(filename);
                }
                this._cache[filename] = source;
            }
            return source;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "As Intended")]
        private byte[]? GetWaveBytesFromResource(string resourceName)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream is null) return null;

            using var reader = new Mp3FileReader(stream);
            return this.GetWaveBytes(reader);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "As Intended")]
        private byte[]? GetWaveBytesFromFile(string filename)
        {
            try
            {
                using var reader = new AudioFileReader(filename);
                return this.GetWaveBytes(reader);
            }
            catch (Exception ex1)
            {
                try
                {
                    using var reader = new Mp3FileReader(filename); // AudioFileReader skip this if OS is recent enough
                    return this.GetWaveBytes(reader);
                }
                catch (Exception ex2)
                {
                    PluginLog.LogError($"Error processing audio file: {filename}\n{new AggregateException(ex1, ex2)}");
                    return null;
                }
            }
        }

        private byte[] GetWaveBytes(WaveStream reader)
        {
            var buffer = new byte[1024];
            var exceptions = new List<Exception>();
            foreach (var method in methods)
            {
                try
                {
                    reader.Position = 0;
                    var bytes = method.Invoke(reader, buffer, AudioEngineCore.Format);
                    if (bytes is null || bytes.Length == 0) throw new Exception($"No bytes returned by {method.Method.Name}");
                    return bytes;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions); // Nothing succeeded
        }

        private static readonly Func<WaveStream, byte[], WaveFormat, byte[]>[] methods = new Func<WaveStream, byte[], WaveFormat, byte[]>[]
        {
                GetWaveBytesUsingMediaFoundation,
                GetBytesUsingWdlResampler,
                //GetBytesUsingWaveFormatConverter, // Doesn't work
        };

        private static byte[] GetWaveBytesUsingMediaFoundation(WaveStream reader, byte[] buffer, WaveFormat format)
        {
            using var sampler = new MediaFoundationResampler(reader, format);
            var waveProvider = sampler.ToSampleProvider().ToWaveProvider();
            var sourceBytes = new List<byte>();

            int samplesRead; while ((samplesRead = waveProvider.Read(buffer, 0, 1024)) > 0) sourceBytes.AddRange(buffer[0..samplesRead]);
            return sourceBytes.ToArray();
        }

        private static byte[] GetBytesUsingWdlResampler(WaveStream reader, byte[] buffer, WaveFormat format)
        {
            var sampler = new WdlResamplingSampleProvider(reader.ToSampleProvider().ToStereo(), format.SampleRate);
            var waveProvider = sampler.ToWaveProvider();
            var sourceBytes = new List<byte>();

            int samplesRead; while ((samplesRead = waveProvider.Read(buffer, 0, 1024)) > 0) sourceBytes.AddRange(buffer[0..samplesRead]);
            return sourceBytes.ToArray();
        }

        private static byte[] GetBytesUsingWaveFormatConverter(WaveStream reader, byte[] buffer, WaveFormat format)
        {
            using var sampler = new WaveFormatConversionStream(format, reader);
            var waveProvider = sampler.ToSampleProvider().ToWaveProvider();
            var sourceBytes = new List<byte>();

            int samplesRead; while ((samplesRead = waveProvider.Read(buffer, 0, 1024)) > 0) sourceBytes.AddRange(buffer[0..samplesRead]);
            return sourceBytes.ToArray();
        }

        public void Reset() => this._core.Reset();
        public void Dispose() => this._core.Dispose();
    }
}
