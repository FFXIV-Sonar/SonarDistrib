using DeviceId.Encoders;
using DeviceId.Formatters;
using DeviceId;
using Sonar.Messages;
using Sonar.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using SonarUtils;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;

namespace Sonar.Utilities
{
    internal static class IdentifierUtils
    {
        internal const string ClientIdentifierFilename = "identifier";

        internal static ClientIdentifier GetClientIdentifier(SonarStartInfo startInfo)
        {
            var bytes = startInfo.ReadFileBytes(ClientIdentifierFilename, 1024);
            if (bytes is not null)
            {
                try
                {
                    return (ClientIdentifier)SonarSerializer.DeserializeData<ISonarMessage>(bytes);
                }
                catch { /* Swallow */ }
            }
            return new();
        }

        internal static void SaveClientIdentifier(ClientIdentifier identifier, SonarStartInfo startInfo)
        {
            startInfo.WriteFileBytes(ClientIdentifierFilename, identifier.SerializeData());
        }

        internal static HardwareIdentifier GetHardwareIdentifier()
        {
            var hwId = new HardwareIdentifier();
            string? identifier;
            try
            {
                identifier = new DeviceIdBuilder()
                    .OnWindows(windows => windows
                        .AddSystemUuid())
                    .OnLinux(linux => linux
                        .AddProductUuid())
                    .OnMac(mac => mac
                        .AddPlatformSerialNumber())
                    .UseFormatter(new StringDeviceIdFormatter(new PlainTextDeviceIdComponentEncoder(), ","))
                    .ToString(); // [..16].ToLowerInvariant(); // This is a big oops :/, can't remove ToLowerInvariant() now // TODO
                identifier = string.IsNullOrEmpty(identifier) ? "unknown" : Base64Url.EncodeToString(GetSecureHash(Encoding.UTF8.GetBytes(identifier)));
            }
            catch (Exception ex)
            {
                identifier = $"unknown_{ex}"; // Inform the server of the exception for later debugging
            }

            hwId.Identifier = $"h3_{identifier}";
            return hwId;
        }

        private static byte[] GetSecureHash(byte[] data, bool allowFallback = true) => GetSecureHash(data, data, allowFallback);

        [SuppressMessage("", "S5344", Justification = "Not applicable.")]
        private static byte[] GetSecureHash(byte[] data, byte[] salt, bool allowFallback = true)
        {
            var exceptions = new List<Exception>();

            try
            {
                // 65536 iterations only takes 100ms on my machine
                // Originally wanted 16 million cycles but that takes 25 seconds
                return Rfc2898DeriveBytes.Pbkdf2(data, salt, 65536, HashAlgorithmName.SHA256, 32);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            try
            {
                // Since I tend to have bad luck. Hopefully this never happens.
                if (allowFallback) return SonarHashing.Sha256(data);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // I seriously have bad luck, inform me
            throw new AggregateException(exceptions);
        }

        public static string? GenerateClientHash(string? clientId, string? clientSecret)
        {
            if (clientId is null || clientSecret is null) return null;
            var bytes = Encoding.UTF8.GetBytes($"{clientId}{clientSecret}");
            var hash = SonarHashing.Sha256(bytes);
            return Base64Url.EncodeToString(hash)[..12];
        }
    }
}
