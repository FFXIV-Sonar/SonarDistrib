using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Config
{
    public enum CityState
    {
        [CityStateMeta(8, 129)]
        Limsa,

        [CityStateMeta(2, 132)]
        Gridania,

        [CityStateMeta(9, 130)]
        Uldah,
    }
}
