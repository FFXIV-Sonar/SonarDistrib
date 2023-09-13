using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Messages;
using Sonar.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Sonar.Utilities.UnixTimeHelper;
using System.Threading;
using Sonar.Data;
using Sonar.Config;
using Sonar.Data.Rows;
using DryIocAttributes;
using Sonar.Relays;

namespace Sonar.Trackers
{
    /// <summary>
    /// Handles, receives and relay fate tracking information
    /// </summary>
    [SingletonReuse]
    [ExportMany]
    public sealed class FateTracker : RelayTracker<FateRelay>
    {
        internal FateTracker(SonarClient client) : base(client) { }
        public override RelayConfig Config => this.Client.Configuration.FateConfig;
    }
}
