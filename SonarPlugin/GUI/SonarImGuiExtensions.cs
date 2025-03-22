using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar.Relays;
using SonarPlugin.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.GUI
{
    public static class SonarImGuiExtensions
    {
        public const string DefaultFoundHunt = "랭크 <rank>: <name> <flagfull>";
        public const string DefaultFoundFate = "돌발: <name> <flagfull>";
        public const string DefaultDeadHunt = "랭크 <rank>: <name> <flagfull> 토벌 완료";
        public const string DefaultDeadFate = "돌발: <name> <flagfull> 완료";
        private static IDictionary<string, string> GetPlaceholdersBase(this Relay relay, bool cwIcon = false)
        {
            var dict = new Dictionary<string, string>(comparer: StringComparer.InvariantCultureIgnoreCase)
            {
                { "world", $"{relay.GetWorld()?.Name ?? $"Ivalid world ({relay.WorldId})" }" },
                { "zone", $"{relay.GetZone()?.Name?.ToString() ?? $"Invalid zone ({relay.ZoneId})" }" },
                { "instance", $"{PayloadExtensions.GenerateInstanceString(relay.InstanceId)}" },
                { "coords", $"{relay.Coords.ToFlagString(MapFlagFormatFlags.IngamePreset)}" },
                { "coordx", $"{relay.Coords.X:F1}" },
                { "coordy", $"{relay.Coords.Y:F1}" },
                { "coordz", $"{relay.Coords.Z:F1}" },
                { "zcoord", $"Z: {relay.Coords.Z:F1}" },

                { "cwIcon", $"{(cwIcon ? (char)SeIconChar.CrossWorld : string.Empty)}" },

                { "key", $"{relay.RelayKey}" },
                { "id", $"{relay.Id}" },
            };
            dict.Add("flag", $"{dict["zone"]}{dict["instance"]} {dict["coords"]}");
            dict.Add("flagfull", $"{dict["flag"]} <{dict["cwicon"]}{dict["world"]}>");
            return dict;
        }

        public static IDictionary<string, string> GetPlaceholders(this HuntRelay relay, bool cwIcon = false)
        {
            var dict = relay.GetPlaceholdersBase(cwIcon);
            dict.Add("name", relay.GetHunt()?.Name?.ToString() ?? $"Invalid hunt ({relay.Id})");
            dict.Add("rank", relay.GetRank().ToString());
            dict.Add("hpp", $"{relay.HpPercent:F1}%");
            dict.Add("progress", $"{relay.Progress:F1}%");
            return dict;
        }

        public static IDictionary<string, string> GetPlaceholders(this FateRelay relay, bool cwIcon = false)
        {
            var dict = relay.GetPlaceholdersBase(cwIcon);
            dict.Add("name", relay.GetZone()?.Name?.ToString() ?? $"Invalid fate ({relay.Id})");
            dict.Add("level", relay.GetZone()?.ToString() ?? "??");
            dict.Add("progress", $"{relay.Progress}%");
            dict.Add("time", $"{(relay.Status == Sonar.Enums.FateStatus.진행중 ? relay.GetRemainingTimeString() : string.Empty)}");
            dict.Add("progressfull", $"{relay.GetRemainingTimeAndProgressString()}");
            return dict;
        }
    }
}
