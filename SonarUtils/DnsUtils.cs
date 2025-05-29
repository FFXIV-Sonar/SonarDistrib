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

        public static LogLevel LogLevel { get; set; } =
#if DEBUG
            LogLevel.Debug;
#else
            LogLevel.Warning;
#endif
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
            add => ImmutableInterlocked.Update(ref s_logListeners, listeners => listeners.Add(value!));
            remove => ImmutableInterlocked.Update(ref s_logListeners, listeners => listeners.Remove(value!));
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

        private static LookupClient CreateLookupClient(Trilean additionalDns)
        {
            // Use a HashSet to filter duplicates
            var nameServers = new HashSet<NameServer>();

            // Default DNS Servers
            Logger.LogInformation("Resolving nameservers");
            RunAndLogExceptionIfThrown(() => nameServers.AddRange(NameServer.ResolveNameServers(true, false)), LogLevel.Warning);
            RunAndLogExceptionIfThrown(() => nameServers.AddRange(NameServer.ResolveNameServersNative()), LogLevel.Warning);
            RunAndLogExceptionIfThrown(() => nameServers.AddRange(NameServer.ResolveNameResolutionPolicyServers()), LogLevel.Warning);

            // Additional DNS Servers
            if (additionalDns.IsTrue || (additionalDns.IsNull && nameServers.Count == 0))
            {
                Logger.LogInformation("Adding Cloudflare and Google DNS servers");
                nameServers.AddRange(
                    // Cloudflare DNS 1.1.1.1
                    NameServer.Cloudflare, NameServer.Cloudflare2,
                    NameServer.CloudflareIPv6, NameServer.Cloudflare2IPv6,

                    // Google DNS 8.8.8.8
                    NameServer.GooglePublicDns, NameServer.GooglePublicDns2,
                    NameServer.GooglePublicDnsIPv6, NameServer.GooglePublicDns2IPv6
                );
            }

            Logger.LogInformation($"Nameservers: {string.Join(", ", nameServers.Select(ns => ns.ToString()))}");

            Logger.LogInformation("Creating DNS Client");
            var options = new LookupClientOptions(nameServers.ToArray())
            {
                CacheFailedResults = true,
                MinimumCacheTimeout = TimeSpan.FromSeconds(60),
                MaximumCacheTimeout = TimeSpan.FromSeconds(60),
#if DEBUG
                EnableAuditTrail = true,
#endif
            };

            // Create DNS Client
            return new LookupClient(options);
        }
    }
}
