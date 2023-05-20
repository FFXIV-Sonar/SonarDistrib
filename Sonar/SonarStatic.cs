using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sonar.Numerics;
using Sonar.Connections;

namespace Sonar
{
    internal static class SonarStatic
    {
        private static readonly ThreadLocal<Random> _random = new(() => new());
        internal static Random Random => _random.Value!;
    }
}
