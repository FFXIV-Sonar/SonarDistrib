using System.Collections.Generic;
using System.Net;
using System.Numerics;

namespace NetTools.Internals
{
    public interface IRangeOperator : ICollection<IPAddress>
    {
        bool Contains(IPAddressRange range);

        BigInteger BigCount { get; }
    }
}
