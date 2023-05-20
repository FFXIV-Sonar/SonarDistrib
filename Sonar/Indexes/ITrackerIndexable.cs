using System.Collections.Generic;

namespace Sonar.Indexes
{
    public interface ITrackerIndexable
    {
        public string GetIndexKey(IndexType type);
        public IEnumerable<string> IndexKeys { get; }
    }
}
