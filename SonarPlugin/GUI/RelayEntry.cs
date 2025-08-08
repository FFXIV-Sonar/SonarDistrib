using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Sonar;
using Sonar.Relays;
using Sonar.Trackers;

namespace SonarPlugin.GUI
{
    public sealed class RelayEntry
    {
        private readonly RelayType _type;
        private readonly string _key;

        private string? _imGroupKey;

        public string GroupKey => this._imGroupKey ??= $"###{this._type}_{this._key}";

        public RelayEntry(SonarClient client, RelayType type, string key)
        {
            this._type = type;
            this._key = key;
        }

        public RelayEntry(SonarClient client, RelayState state)
        {
            this._type = state.GetRelayType();
            this._key = state.RelayKey;
        }

        public RelayEntry(SonarClient client, IRelay relay)
        {
            this._type = relay.GetRelayType();
            this._key = relay.RelayKey;
        }

        public void Draw()
        {
            /*
            Hunts:
                | Name | Lv | Progress | Flag |

            Fates:
                | Name | Lv | Progress | Flag |
            */

        }
    }
}
