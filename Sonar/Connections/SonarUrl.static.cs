using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Connections
{
    public sealed partial class SonarUrl
    {
        public static byte[] SerializeUrls(IEnumerable<SonarUrl> urls)
        {
            var bytes = SonarSerializer.SerializeData(urls);
            SonarObfuscator.Obfuscate(bytes);
            return bytes;
        }

        public static IEnumerable<SonarUrl> DeserializeUrls(byte[] bytes)
        {
            SonarObfuscator.Deobfuscate(bytes);
            return SonarSerializer.DeserializeData<IEnumerable<SonarUrl>>(bytes);
        }

        private static IEnumerable<SonarUrl> LoadEmbeddedUrls()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Sonar.Resources.Urls.data");
            if (stream is null) throw new FileNotFoundException($"Couldn't read url resources");

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return SonarSerializer.DeserializeData<IEnumerable<SonarUrl>>(bytes);
        }

        private static Lazy<IEnumerable<SonarUrl>> s_urls = new(LoadEmbeddedUrls);
        public static IEnumerable<SonarUrl> Urls
        {
            get => s_urls.Value;
            internal set => s_urls = new(value);
        }
    }
}
