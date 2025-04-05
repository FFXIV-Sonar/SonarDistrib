using MessagePack;
using Sonar.Messages;
using SonarUtils;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sonar.Models
{
    public sealed partial class ClientSecret
    {
        /// <summary>Generates a secret hash for a specified secret name</summary>
        public static byte[] HashSecret(string secretName)
        {
            return BouncySha256.HashData(Encoding.UTF8.GetBytes($"SECRET {secretName} T3RC3Z"));
        }

        /// <summary>Read embedded secret from an assembly</summary>
        public static ClientSecret? ReadEmbeddedSecret(Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null) return default;
            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes);

            try
            {
                bytes = UrlBase64.Decode(Encoding.UTF8.GetString(bytes, 0, bytes.Length).Replace("\n", string.Empty));
                SonarObfuscator.Deobfuscate(bytes);
                return SonarSerializer.DeserializeData<ClientSecret>(bytes);
            }
#pragma warning disable CS0168 // Variable is declared but never used (Justification = Debugger inspection)
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                if (Debugger.IsAttached) Debugger.Break();
                return default;
            }
        }
    }
}
