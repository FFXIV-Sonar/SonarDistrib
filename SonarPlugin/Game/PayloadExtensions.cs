using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Sonar.Data.Extensions;
using Sonar.Enums;
using Sonar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Game
{
    public static class PayloadExtensions
    {
        private static readonly Payload[] s_crossworldIcon = new Payload[]
        {
            new UIForegroundPayload(56),
            new TextPayload($"{(char)SeIconChar.CrossWorld}"),
            UIForegroundPayload.UIForegroundOff,
        };

        public static MapLinkPayload GetMapLinkPayload(this GamePosition position)
        {
            var mapId = position.GetZone()?.MapId ?? 0;
            return new MapLinkPayload(position.ZoneId, mapId, (int)(position.Coords.X * 1000), (int)(position.Coords.Y * 1000));
        }

        public static SeString GetMapLinkSeString(this GamePosition position, bool cwIcon = false)
        {
            var mapId = position.GetZone()?.MapId ?? 0;
            SeStringBuilder builder = new();
            builder.AddSeString(SeString.CreateMapLink(position.ZoneId, mapId, (int)(position.Coords.X * 1000), (int)(position.Coords.Y * 1000)));
            if (position.InstanceId != 0) builder.AddText($" {GenerateInstanceString(position.InstanceId)}");
            builder.AddText(" <");
            if (cwIcon) builder.AddRange(s_crossworldIcon);
            builder.AddText($"{position.GetWorld()?.Name ?? "INVALID"}>");
            return builder.Build();
        }

        public static string GenerateInstanceString(uint instanceId)
        {
            return instanceId switch
            {
                1 => $"{(char)SeIconChar.Instance1}",
                2 => $"{(char)SeIconChar.Instance2}",
                3 => $"{(char)SeIconChar.Instance3}",
                4 => $"{(char)SeIconChar.Instance4}",
                5 => $"{(char)SeIconChar.Instance5}",
                6 => $"{(char)SeIconChar.Instance6}",
                7 => $"{(char)SeIconChar.Instance7}",
                8 => $"{(char)SeIconChar.Instance8}",
                9 => $"{(char)SeIconChar.Instance9}",
                _ => $"i{instanceId}", // fall-back
            };
        }

        public static SeString GetMapLinkSeString(this HuntRelay relay, bool cwIcon = false)
        {
            var info = relay.GetHunt();
            SeStringBuilder builder = new();
            builder.AddText($"Rank {info?.Rank ?? HuntRank.None}: {info?.Name.ToString() ?? "INVALID"} ");
            builder.AddSeString(((GamePosition)relay).GetMapLinkSeString(cwIcon));
            return builder.Build();
        }

        public static SeString GetMapLinkSeString(this FateRelay relay, bool cwIcon = false)
        {
            var info = relay.GetFate();
            SeStringBuilder builder = new();
            builder.AddText($"FATE: {info?.Name.ToString() ?? "INVALID"} ");
            builder.AddSeString(((GamePosition)relay).GetMapLinkSeString(cwIcon));
            switch (relay.Status)
            {
                case FateStatus.Running:
                    builder.AddSeString($" ({relay.Progress}% with {relay.GetRemainingTimeString()} remaining)");
                    break;
                case FateStatus.Preparation:
                    builder.AddSeString($" (in Preparation)");
                    break;
            }
            return builder.Build();
        }
    }
}
