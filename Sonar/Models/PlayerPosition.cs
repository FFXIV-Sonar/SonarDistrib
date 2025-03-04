using MessagePack;
using Sonar.Messages;
using System;

namespace Sonar.Models
{
    [MessagePackObject]
    [Serializable]
    public sealed class PlayerPosition : GamePosition, ISonarMessage
    {
        public PlayerPosition() { }
        public PlayerPosition(GamePlace p) : base(p) { }
        public PlayerPosition(GamePosition p) : base(p) { }
        public PlayerPosition(PlayerPosition p) : base(p) { }
    }
}
