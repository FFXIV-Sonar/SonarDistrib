using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace Sonar.Relays
{
    [Union(0x00, typeof(Relay))]
    [Union(0x01, typeof(HuntRelay))]
    [Union(0x02, typeof(FateRelay))]
    [Union(0x03, typeof(ManualRelay))]
    public interface IRelay
    {
        /// <summary>Relay ID</summary>
        public uint Id { get; }

        /// <summary>Relay Type</summary>
        public string Type { get; }

        /// <summary>World ID</summary>
        public uint WorldId { get; }

        /// <summary>Zone ID</summary>
        public uint ZoneId { get; }

        /// <summary>Instance ID</summary>
        public uint InstanceId { get; }

        /// <summary>Index Keys</summary>
        public IEnumerable<string> IndexKeys { get; }

        /// <summary>Relay Key</summary>
        public string RelayKey { get; }

        /// <summary>Place Key</summary>
        public string PlaceKey { get; }

        /// <summary>Sort Key</summary>
        public string SortKey { get; }

        /// <summary>Check if its alive</summary>
        public bool IsAlive();

        /// <summary>Check if its dead</summary>
        public bool IsDead() => !this.IsAlive();
    }
}
