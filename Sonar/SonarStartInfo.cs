using Sonar.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar
{
    public sealed partial class SonarStartInfo : ICloneable
    {
        /// <summary>Whether this <see cref="SonarStartInfo"/> is locked. This happens once its used by <see cref="SonarClient"/></summary>
        public bool Locked
        {
            get => field;
            internal set => field = value;
        }

        /// <summary>Sonar's working directory.</summary>
        /// <remarks>Cannot be changed once <see cref="Locked"/>.</remarks>
        public string WorkingDirectory
        {
            get => field ??= GetDefaultWorkingDirectory();
            set
            {
                this.ThrowIfLocked();
                field = value;
            }
        }

        public ImmutableArray<byte>? PluginSecretMeta
        {
            get => field;
            set
            {
                this.ThrowIfLocked();
                field = value;
            }
        }

        public Func<ImmutableArray<byte>, CancellationToken, Task<IReadOnlyDictionary<string, ImmutableArray<byte>>?>>? ChallengeHandler
        {
            get => field;
            set
            {
                this.ThrowIfLocked();
                field = value;
            }
        }

        public SonarStartInfo Clone()
        {
            var ret = Unsafe.As<SonarStartInfo>(this.MemberwiseClone());
            ret.Locked = false;
            return ret;
        }

        object ICloneable.Clone() => this.Clone();
    }
}
