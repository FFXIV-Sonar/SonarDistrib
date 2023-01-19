using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Sonar;
using Sonar.Logging;

namespace SonarGUI
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Intentional")]
    public sealed class SonarGUIService : IDisposable
    {
        private readonly Dictionary<string, ISonarWindow> _windows;
        private readonly SonarLogger _logger = new();

        public SonarClient Client { get; }
        public ISonarLogger Logger => this._logger;

        public SonarGUIService(SonarClient client)
        {
            this.Client = client;
            this._logger.LogMessage += this.LogHandler;

            var cachedArgs = new object[] { this };
            var assembly = Assembly.GetExecutingAssembly();

            var windows = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i == typeof(ISonarWindow)))
                .Select(t => (ISonarWindow?)Activator.CreateInstance(t, cachedArgs))
                .Where(o => o is not null)
                .Select(o => KeyValuePair.Create(o!.WindowId, o));

            this._windows = new(windows, StringComparer.Ordinal);
        }

        public ISonarWindow? GetWindow(string id) => this._windows.GetValueOrDefault(id);
        public IReadOnlyDictionary<string, ISonarWindow> GetWindows() => this._windows;

        internal void AddWindow(ISonarWindow window) => this._windows[window.WindowId] = window;

        private int _disposed;
        public bool IsDisposed => this._disposed != 0;

        public void Draw()
        {
            foreach (var window in this.GetWindows().Values)
            {
                this.ProcessWindow(window);
            }
        }

        private void ProcessWindow(ISonarWindow window)
        {
            try
            {
                if (window.Destroy) this.DestroyWindow(window.WindowId);
                else if (window.Visible) window.Draw();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, string.Empty);
            }
        }

        internal void DestroyWindow(string id)
        {
            if (!this._windows.TryGetValue(id, out var window)) return;
            this._windows.Remove(id);
            if (window is IDisposable disposable) disposable.Dispose();
        }
        internal void DestroyWindow(ISonarWindow window) => this.DestroyWindow(window.WindowId);

        public void LogHandler(ISonarLogger _, LogLine log)
        {
            this.LogMessage?.Invoke(this, log);
        }
        
        public event SonarGUILog? LogMessage;
        
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this._disposed, 1, 0) == 1) return;
            foreach (var id in this.GetWindows().Keys)
            {
                this.DestroyWindow(id);
            }
            this._logger.LogMessage -= this.LogHandler;
        }
    }

    public delegate void SonarGUILog(SonarGUIService source, LogLine log);
}
