using Sonar.Models;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Sonar
{
    public sealed partial class SonarStartInfo : ICloneable
    {
        private bool _locked;
        private string? _workingDirectory;
        private ClientSecret? _pluginSecret;

        /// <summary>Whether this <see cref="SonarStartInfo"/> is locked. This happens once its used by <see cref="SonarClient"/></summary>
        public bool Locked
        {
            get => this._locked;
            internal set => this._locked = value;
        }

        /// <summary>Sonar's working directory.</summary>
        /// <remarks>Cannot be changed once <see cref="Locked"/>.</remarks>
        public string WorkingDirectory
        {
            get => this._workingDirectory ??= GetDefaultWorkingDirectory();
            set
            {
                this.ThrowIfLocked();
                this._workingDirectory = value;
            }
        }

        public ClientSecret? PluginSecret
        {
            get => this._pluginSecret;
            set
            {
                this.ThrowIfLocked();
                this._pluginSecret = value;
            }
        }

        public SonarStartInfo Clone()
        {
            var ret = Unsafe.As<SonarStartInfo>(this.MemberwiseClone());
            ret._locked = false;
            return ret;
        }

        object ICloneable.Clone() => this.Clone();
    }
}
