using AG.EnumLocalization;
using Sonar.Data;
using Sonar.Data.Extensions;
using SonarUtils;
using SonarUtils.Text.Placeholders.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Models
{
    public partial class GamePlace : IPlaceholderReplacementProvider
    {
        public virtual bool TryGetValue(ReadOnlySpan<char> name, [MaybeNullWhen(false)] out ReadOnlySpan<char> value)
        {
            var result = name switch
            {
                "placekey" => this.PlaceKey,

                "world" => Database.Worlds.GetValueOrDefault(this.WorldId)?.Name ?? $"w{StringUtils.GetNumber(this.WorldId)}",
                "zone" => Database.Zones.GetValueOrDefault(this.ZoneId)?.Name.ToString() ?? $"w{StringUtils.GetNumber(this.ZoneId)}",
                "instance" => StringUtils.GetInstanceSymbol(this.InstanceId),

                "worldid" => StringUtils.GetNumber(this.WorldId),
                "zoneid" => StringUtils.GetNumber(this.ZoneId),
                "instanceid" => StringUtils.GetNumber(this.InstanceId),

                "expansion" => this.GetExpansion().GetLocString(),

                "hasz" => Database.Zones.GetValueOrDefault(this.ZoneId)?.HasOffsetZ.ToString(),

                _ => null
            };

            value = result;
            return result is not null;
        }
    }
}
