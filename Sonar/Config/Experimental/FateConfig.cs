using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Config.Experimental
{
    public sealed class FateConfig<T>
    {
        public T? DefaultConfig { get; set; }
        public IDictionary<uint, T> Configs { get; set; } = new Dictionary<uint, T>();
    }
}
