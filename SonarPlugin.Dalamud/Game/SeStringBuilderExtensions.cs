using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Game
{
    public static class SeStringBuilderExtensions
    {
        public static void AddRange(this SeStringBuilder builder, IEnumerable<Payload> payloads)
        {
            foreach (var payload in payloads) { builder.Add(payload); }
        }
        public static void AddSeString(this SeStringBuilder builder, SeString str) => builder.AddRange(str.Payloads);
    }
}
