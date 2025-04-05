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
                    //.UseFormatter(new HashDeviceIdFormatter(SHA256.Create, new Base64UrlByteArrayEncoder()))
                    .ToString(); // [..16].ToLowerInvariant(); // This is a big oops :/, can't remove ToLowerInvariant() now // TODO
                //identifier = string.IsNullOrEmpty(identifier) ? "unknown" : UrlBase64.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(identifier)));
                identifier = string.IsNullOrEmpty(identifier) ? "unknown" : UrlBase64.Encode(GetSecureHash(Encoding.UTF8.GetBytes(identifier)));
            }
            catch (Exception ex)
            {
                identifier = $"unknown_{ex}"; // Inform the server of the exception for later debugging
                //throw;

                // DeviceId happen to swallow exceptions so this is unlikely to happen
                // Decision has been made to keep this handler anyway just in case it throws an exception in the future.
            }

            hwId.Identifier = $"h3_{identifier}";
            return hwId;
        }

        private static byte[] GetSecureHash(byte[] data, bool allowFallback = true) => GetSecureHash(data, data, allowFallback);

        private static byte[] GetSecureHash(byte[] data, byte[] salt, bool allowFallback = true)
        {
            var exceptions = new List<Exception>();

            try
            {
                // 65536 iterations only takes 100ms on my machine
                // Originally wanted 16 million cycles but that takes 25 seconds
                var pbkdf2 = new Rfc2898DeriveBytes(data, salt, 65536, HashAlgorithmName.SHA256);
                return pbkdf2.GetBytes(32);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            try
            {
                // Since I tend to have bad luck. Hopefully this never happens.
                if (allowFallback) return BouncySha256.HashData(data);
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
            var hash = BouncySha256.HashData(bytes);
            return UrlBase64.Encode(hash)[..12];
        }
    }
}
