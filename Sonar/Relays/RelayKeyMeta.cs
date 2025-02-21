using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Relays
{
    public readonly record struct RelayKeyMeta(uint WorldId, uint RelayId, uint InstanceId, string? ExtendedKey);
}
