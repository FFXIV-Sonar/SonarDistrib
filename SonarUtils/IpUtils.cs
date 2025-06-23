using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace SonarUtils
{
    public static class IpUtils
    {
        public static bool IPv4Supported { get; private set; } = true;
        public static bool IPv6Supported { get; private set; } = true;

        public static void UpdateSupported()
        {
            var ipv4 = false; var ipv6 = false;
            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(adapter => adapter.NetworkInterfaceType is not NetworkInterfaceType.Loopback)
                .Where(adapter => adapter.OperationalStatus is OperationalStatus.Up or OperationalStatus.Unknown);
            foreach (var adapter in adapters)
            {
                if (!ipv4) ipv4 |= adapter.Supports(NetworkInterfaceComponent.IPv4);
                if (!ipv6) ipv6 |= adapter.Supports(NetworkInterfaceComponent.IPv6);
                if (ipv4 && ipv6) break; // No reason to keep looking
            }
            if (!ipv4 && !ipv6) throw new NotSupportedException("Failed to detect IPv4 and IPv6 support");
            SetSupported(ipv4, ipv6);
        }

        public static void SetSupported(bool ipv4 = true, bool ipv6 = true)
        {
            IPv4Supported = ipv4; IPv6Supported = ipv6;
        }
    }
}
