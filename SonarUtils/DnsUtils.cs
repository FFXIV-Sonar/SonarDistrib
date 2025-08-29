using AG;
using DnsClient;
using DnsClient.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using SonarUtils.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace SonarUtils
{
    // I don't believe I have to do this!
    /// <summary>DNS utility methods.</summary>
    public static class DnsUtils
    {
        private static SpinLock s_lock = new(false);
        private static Trilean s_additionalNameServers = Trilean.Null;
        private static ImmutableArray<Action<string, LogLevel, int, Exception?, string, object[]>> s_logListeners = [];
        private static Lazy<ILookupClient> s_client;

        public static ILookupClient Client => s_client.Value;

        /// <summary>Gets or sets a value indicating if additional name servers, cloudflare and google, should be used.</summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><see langword="null"/>: Only if no name servers were detected. <b>Default</b></item>
        /// <item><see langword="true"/>: Always add the additional name servers.</item>
        /// <item><see langword="false"/>: Never add the additional name servers.</item>
        /// </list>
        /// </remarks>
        public static Trilean AddAdditionalNameServers
        {
            get => s_additionalNameServers;
            set
            {
                if (s_additionalNameServers == value) return; // nothing to do
                var locked = false;
                s_lock.Enter(ref locked);
                try
                {
                    s_client = new(() => CreateLookupClient(value));
                    s_additionalNameServers = value;
                }
                finally
                {
                    s_lock.Exit();
                }
            }
        }

        public static LogLevel LogLevel { get; set; } = LogLevel.Trace;
        public static ILogger Logger { get; }

        // Yes, I'm aware of performance implications of a static constructor.
        // DNS queries however, are not performance critical.
        [SuppressMessage("Minor Code Smell", "S3963", Justification = "Extra steps")]
        static DnsUtils()
        {
            var factory = new DnsLoggerFactory();
            DnsClient.Logging.LoggerFactory = factory;
            Logger = factory.CreateLogger("DnsUtils");
            s_client = new(() => CreateLookupClient(s_additionalNameServers));
        }

        public static event Action<string, LogLevel, int, Exception?, string, object[]>? Log
        {
            add
            {
                if (value is not null) ImmutableInterlocked.Update(ref s_logListeners, listeners => listeners.Add(value!));
            }
            remove
            {
                if (value is not null) ImmutableInterlocked.Update(ref s_logListeners, listeners => listeners.Remove(value!));
            }
        }

        internal static void DispatchLogEvent(string categoryName, LogLevel logLevel, int eventId, Exception? exception, string message, params object[] args)
        {
            foreach (var listener in s_logListeners.AsSpan())
            {
                try
                {
                    listener(categoryName, logLevel, eventId, exception, message, args);
                }
                catch (Exception ex)
                {
                    // TODO: Proper way to log
                    GC.KeepAlive(ex); // Place a breakpoint for now... :D
                }
            }
        }

        private static void RunAndLogExceptionIfThrown(Action action, LogLevel logLevel = LogLevel.Error)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Log(logLevel, ex, string.Empty);
            }
        }

        public static IEnumerable<NameServer> DiscoverNameservers(Trilean additionalDns, bool skipIPv6SiteLocal = true)
        {
            // Use a HashSet to filter duplicates
            var nameServers = new List<NameServer>();

            // Check for IPv4 and IPv6 support
            try
            {
                IpUtils.UpdateSupported();
                Logger.LogInformation($"Network Support: (IPv4 = {IpUtils.IPv4Supported} | IPv6 = {IpUtils.IPv6Supported})");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to determine IPv4 and IPv6 support, forcing both");
                IpUtils.SetSupported(true, true);
            }

            // Default DNS Servers
            Logger.LogInformation("Resolving nameservers");
            RunAndLogExceptionIfThrown(() => nameServers.AddRange(NameServer.ResolveNameServersNet()), LogLevel.Warning);
            RunAndLogExceptionIfThrown(() => nameServers.AddRange(NameServer.ResolveNameServersNative()), LogLevel.Warning);
            RunAndLogExceptionIfThrown(() => nameServers.AddRange(NameServer.ResolveNameServersNrpt()), LogLevel.Warning);

            // Remove unsupported DNS servers (NOTE: Done twice intentionally)
            nameServers.RemoveAll(ns => !IsSupported(ns) || (skipIPv6SiteLocal && IPAddress.Parse(ns.Address).IsIPv6SiteLocal));

            // Additional DNS Servers
            if (additionalDns.IsTrue || (additionalDns.IsNull && nameServers.Count == 0))
            {
                Logger.LogInformation("Adding additional DNS servers");
                nameServers.AddRange(NameServer.DefaultFallback);
            }

            // Remove unsupported DNS servers (NOTE: Done twice intentionally)
            nameServers.RemoveAll(ns => !IsSupported(ns) || (skipIPv6SiteLocal && IPAddress.Parse(ns.Address).IsIPv6SiteLocal));

            // Log discovered nameservers and return them
            var result = nameServers.ToHashSet();
            Logger.LogInformation($"Nameservers: {string.Join(", ", result.Select(ns => ns.ToString()))}");
            return result;
        }

        public static LookupClient CreateLookupClient(Trilean additionalDns)
        {
            var nameServers = DiscoverNameservers(additionalDns);
            return CreateLookupClient(nameServers);
        }

        public static LookupClient CreateLookupClient(IEnumerable<NameServer> nameServers)
        {
            Logger.LogInformation("Creating DNS Client");
            var options = new LookupClientOptions([.. nameServers])
            {
                CacheFailedResults = true,
                MinimumCacheTimeout = TimeSpan.FromSeconds(60),
                MaximumCacheTimeout = TimeSpan.FromSeconds(60),
                EnableAuditTrail = true,
            };

            // Create DNS Client
            return new LookupClient(options);
        }

        public static bool IsSupported(NameServer nameServer)
        {
            if (!IpUtils.IPv4Supported && nameServer.AddressFamily is AddressFamily.InterNetwork) return false;
            if (!IpUtils.IPv6Supported && nameServer.AddressFamily is AddressFamily.InterNetworkV6) return false;
            return true;
        }

        /// <summary>Runs a query in a dedicated thread asynchronously using <see cref="LookupClient.Query(string, QueryType, QueryClass)"/> as the backend.</summary>
        /// <param name="cancellationToken">Cancellation task to cancel the await. The thread itself continues running.</param>
        /// <returns>A <see cref="Task"/> that can be awaited for a <see cref="IDnsQueryResponse"/>.</returns>
        public static Task<IDnsQueryResponse> QueryAsync(string query, QueryType queryType, QueryClass queryClass = QueryClass.IN, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() => Client.Query(query, queryType, queryClass), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
