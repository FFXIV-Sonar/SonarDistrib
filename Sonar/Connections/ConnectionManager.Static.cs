using System;

namespace Sonar.Connections
{
    /// <summary>Manage sonar connections</summary>
    public sealed partial class SonarConnectionManager
    {
        private const int RecheckIntervalMs = 100;
        private const int MinimumIntervalMs = 2000;
        private const int MaximumIntervalMs = 300000;
        private const double IntervalVariation = 0.2; // This can break min and max interval boundaries (intentional)
        private const int InitialDelayMs = 5000; // Give enough time for URL bootstrapping
        private const int StartingIntervalMs = 500; // 500 => 750 => 1125* => 1687 => 2531 (* = Proxies Threshold)
        private const double ExponentialBase = SonarConstants.DebugBuild ? 1.0 : 1.5;
        private const int FailureThreshold = 3; // Failure threshold to consider proxies
        private const int MaxConcurrentAttempts = 16;

        private const int SocketsCapacity = 2;
        private const int ConnectTimeoutMs = 15000; // DNS => HTTPS connect => HTTPS tls neg
        private const int ReadyTimeoutMs = 5000;

        private static int GetNextAttemptDelayMs(int failureCount)
        {
            return GetVariedIntervalMs((int)Math.Max(Math.Min(StartingIntervalMs * Math.Pow(ExponentialBase, failureCount), MaximumIntervalMs), MinimumIntervalMs));
        }

        private static int GetVariedIntervalMs(int intervalMs)
        {
            return (int)(intervalMs * (1.0 - IntervalVariation + SonarStatic.Random.NextDouble() * IntervalVariation * 2));
        }
    }
}
