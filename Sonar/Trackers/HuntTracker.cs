using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Messages;
using Sonar.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Sonar.Utilities.UnixTimeHelper;
using System.Threading;
using Sonar.Data;
using Sonar.Data.Rows;
using Sonar.Config;
using DryIocAttributes;
using Sonar.Relays;

namespace Sonar.Trackers
{
    /// <summary>
    /// Handles, receives and relay hunt tracking information
    /// </summary>
    [SingletonReuse]
    [ExportMany]
    public sealed class HuntTracker : RelayTracker<HuntRelay>
    {
        internal HuntTracker(SonarClient client) : base(client) { }
        public override RelayConfig Config => this.Client.Configuration.HuntConfig;
    }
}
