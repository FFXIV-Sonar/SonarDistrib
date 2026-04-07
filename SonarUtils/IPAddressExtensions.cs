using NetTools;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace SonarUtils
{
    // Source - https://stackoverflow.com/a/72348199
    // Posted by angularsen, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-03-20, License - CC BY-SA 4.0
    // https://gist.github.com/angularsen/f77b53ee9966fcd914025e25a2b3a085
    // or what's left of it, modified to include multicast and broadcast addresses
    // along with usage of IPAddressRange

    /// <summary>Extension methods on <see cref="IPAddress"/></summary>
    public static class IPAddressExtensions
    {
        private static readonly IPAddressRange[] s_private4Ranges = [
            IPAddressRange.Parse("169.254.0.0/16"), // Link local (no IP assigned by DHCP): 169.254.0.0 to 169.254.255.255 (169.254.0.0/16)
            IPAddressRange.Parse("10.0.0.0/8"), // Class A private range: 10.0.0.0 – 10.255.255.255 (10.0.0.0/8)
            IPAddressRange.Parse("172.16.0.0/12"), // Class B private range: 172.16.0.0 – 172.31.255.255 (172.16.0.0/12)
            IPAddressRange.Parse("192.168.0.0/16"), // Class C private range: 192.168.0.0 – 192.168.255.255 (192.168.0.0/16)
            IPAddressRange.Parse("224.0.0.0/4"), // Multicast IP Addresses: 224.0.0.0 - 239.255.255.255 (224.0.0.0/4)
        ];

        private static readonly IPAddressRange[] s_private6Ranges = [
            // IPAddressRange.Parse("ff00::/8"), // Multicast IP Addresses (Commented: Using .IsIPv6Multicast instead)
        ];

        extension(IPAddress ipAddress)
        {
            /// <summary>Returns true if the IP address is in a private, multicast or broadcast range.</summary>
            /// <remarks>
            /// <list type="bullet">
            /// <item><b>IPv4:</b> Loopback, link local ("169.254.x.x"), class A ("10.x.x.x"), class B ("172.16.x.x" to "172.31.x.x"), class C ("192.168.x.x"), multicast("224.x.x.x" to "239.x.x.x") and broadcast ("255.255.255.255").</item>
            /// <item><b>IPv6:</b> Loopback, link local, site local, multicast, unique local and private IPv4 mapped to IPv6.</item>
            /// </list>
            /// </remarks>
            /// <param name="ipAddress">The IP address.</param>
            /// <returns><see langword="true"/> if the IP address was in a private, broadcast or multicast range.</returns>
            /// <example><code>bool isPrivate = IPAddress.Parse("127.0.0.1").IsPrivate();</code></example>
            public bool IsPrivate()
            {
                // Map back to IPv4 if mapped to IPv6, for example "::ffff:1.2.3.4" to "1.2.3.4".
                if (ipAddress.IsIPv4MappedToIPv6) ipAddress = ipAddress.MapToIPv4();

                // Checks loopback ranges for both IPv4 and IPv6.
                if (IPAddress.IsLoopback(ipAddress)) return true;

                // Address Family
                var family = ipAddress.AddressFamily;

                // IPv4
                if (family is AddressFamily.InterNetwork) return IsPrivateCoreIPv4(ipAddress);

                // IPv6
                if (family is AddressFamily.InterNetworkV6) return IsPrivateCoreIPv6(ipAddress);

                ThrowNotSupported(family);
                return default; // Not reached
            }
        }

        private static bool IsPrivateCoreIPv4(IPAddress ipAddress)
        {
            Debug.Assert(ipAddress.AddressFamily is AddressFamily.InterNetwork);
            if (ipAddress.Equals(IPAddress.Broadcast)) return true;
            foreach (var range in s_private4Ranges.AsSpan())
            {
                if (range.Contains(ipAddress)) return true;
            }
            return false;
        }

        private static bool IsPrivateCoreIPv6(IPAddress ipAddress)
        {
            Debug.Assert(ipAddress.AddressFamily is AddressFamily.InterNetworkV6);
            if (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6UniqueLocal || ipAddress.IsIPv6SiteLocal || ipAddress.IsIPv6Multicast) return true;
            foreach (var range in s_private6Ranges.AsSpan())
            {
                if (range.Contains(ipAddress)) return true;
            }
            return false;
        }

        [DoesNotReturn]
        private static void ThrowNotSupported(AddressFamily family) => throw new NotSupportedException($"IP address family {family} is not supported, expected only IPv4 ({AddressFamily.InterNetwork}) or IPv6 ({AddressFamily.InterNetworkV6}).");
    }
}