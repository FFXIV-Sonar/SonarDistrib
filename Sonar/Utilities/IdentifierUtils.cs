using DeviceId.Encoders;
using DeviceId.Formatters;
using DeviceId;
using Sonar.Messages;
using Sonar.Models;
using System;
using System.Security.Cryptography;

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
                    .UseFormatter(new HashDeviceIdFormatter(SHA256.Create, new Base64UrlByteArrayEncoder()))
                    .ToString()[..16].ToLowerInvariant(); // This is a big oops :/, can't remove ToLowerInvariant() now
            }
            catch (Exception ex)
            {
                identifier = $"unknown_{ex}"; // Inform the server of the exception for later debugging
                //throw;
            }

            hwId.Identifier = $"h2_{identifier}";
            return hwId;
        }

    }
}
