using Sonar.Data;
using Sonar.Models;
using Sonar.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar
{
    public sealed partial class SonarClient
    {
        private const int MaxInstances = 1; // Max SonarClient instances
        private static int s_instances; // Interlocked count

        /// <summary>Executed once a <see cref="SonarClient"/> is created</summary>
        [SuppressMessage("Critical Code Smell", "S1215")]
        private static void ClientCreated(int attemptedCount = 0)
        {
            if (Interlocked.Increment(ref s_instances) > MaxInstances)
            {
                Interlocked.Decrement(ref s_instances);
                if (attemptedCount >= 2) throw new InvalidOperationException($"Only {MaxInstances} {nameof(SonarClient)} instances can exist at a time");
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect();
                ClientCreated(attemptedCount + 1);
            }
        }

        /// <summary>Executed once a <see cref="SonarClient"/> is finalized</summary>
        private static void ClientFinalized()
        {
            if (Interlocked.Decrement(ref s_instances) == 0)
            {
                FinalDispose();
            }
        }


        /// <summary>Executed once all instances of <see cref="SonarClient"/> are disposed</summary>
        private static void FinalDispose()
        {
            Database.Reset();
            StringUtils.Reset();
            GamePlace.ResetIndexCache();
            PlayerInfo.ResetIndexCache();
        }
    }
}
