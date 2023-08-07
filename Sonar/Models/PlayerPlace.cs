using MessagePack;
using Newtonsoft.Json;
using Sonar.Messages;
using Sonar.Relays;
using System;

namespace Sonar.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    [MessagePackObject]
    [Serializable]
    public sealed class PlayerPlace : GamePlace, ISonarMessage
    {
        public PlayerPlace() { }
        public PlayerPlace(GamePlace p) : base(p) { }
        public PlayerPlace(PlayerPlace p) : base(p) { }
    }
}
