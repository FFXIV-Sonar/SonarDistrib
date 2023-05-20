using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Connections
{
    internal sealed partial class SonarUrlManager
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
    }
}
