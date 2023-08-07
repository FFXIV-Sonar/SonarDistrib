using Sonar.Enums;
using Sonar.Relays;
using Sonar.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sonar.Utilities.UnixTimeHelper;
using static Sonar.SonarConstants;
using Sonar.Utilities;
using Sonar.Data;
using Sonar.Data.Extensions;
using Sonar;
using Lumina.Excel.GeneratedSheets;

namespace SonarPlugin.Trackers
{
    [SingletonService]
    public sealed class RelayTrackerViews : IDisposable
    {
        private readonly Dictionary<HuntRank, IRelayTracker<HuntRelay>> _huntViews = new();
        private readonly IRelayTracker<FateRelay> _fateView;

        private SonarPlugin Plugin { get; }
        private SonarClient Client { get; }
        public HuntTracker HuntsTracker { get; }
        public FateTracker FatesTracker { get; }
        
        public IRelayTracker<HuntRelay> Hunts => this._huntViews[HuntRank.None];
        public IReadOnlyDictionary<HuntRank, IRelayTracker<HuntRelay>> HuntsByRank => this._huntViews;
        public IRelayTracker<FateRelay> Fates => this._fateView;


        public RelayTrackerViews(SonarPlugin plugin, SonarClient client, HuntTracker hunts, FateTracker fates)
        {
            this.Plugin = plugin;
            this.Client = client;
            this.HuntsTracker = hunts;
            this.FatesTracker = fates;

            foreach (var rank in Enum.GetValues<HuntRank>()) this._huntViews[rank] = hunts.CreateView(state => this.HuntViewPredicate(state, rank));
            this._fateView = fates.CreateView(this.FateViewPredicate);
        }

        private bool HuntViewPredicate(RelayState<HuntRelay> state, HuntRank rank)
        {
            var relay = state.Relay;
            var info = relay.GetHunt()!;
            if (rank != HuntRank.None && info.Rank != rank) return false;
            if (!this.Client.PlayerPlace.IsWithinJurisdiction(relay, this.HuntsTracker.Config.GetReportJurisdiction(state.Id))) return false;
            var now = SyncedUnixNow;

            // List decaying
            if (state.IsAlive())
            {
                if (info.Rank == HuntRank.B && (now - state.LastSeen) > EarthSecond * this.Plugin.Configuration.DisplayHuntUpdateTimerOther)
                    return false;
                else if (info.Rank != HuntRank.B && (now - state.LastSeen) > EarthSecond * this.Plugin.Configuration.DisplayHuntUpdateTimer)
                    return false;
            }
            else
            {
                // B ranks respawn after roughly 5 seconds no need to keep them around longer than that
                if (info.Rank == HuntRank.B && (now - state.LastKilled) > EarthSecond * Math.Min(5, this.Plugin.Configuration.DisplayHuntDeadTimer))
                    return false;
                else if ((now - state.LastKilled) > EarthSecond * this.Plugin.Configuration.DisplayHuntDeadTimer)
                    return false;
            }

            // Approve
            return true;
        }

        public bool FateViewPredicate(RelayState<FateRelay> state)
        {
            var relay = state.Relay;
            if (!this.Client.PlayerPlace.IsWithinJurisdiction(relay, this.FatesTracker.Config.GetReportJurisdiction(state.Id))) return false;
            var now = SyncedUnixNow;

            // List decaying
            if (state.IsAlive())
            {
                if ((now - state.LastUpdated) > (EarthSecond * this.Plugin.Configuration.DisplayFateUpdateTimer))
                    return false;
            }
            else
            {
                if ((now - state.LastUpdated) > (EarthSecond * this.Plugin.Configuration.DisplayFateDeadTimer) + (state.Relay.Status == FateStatus.Unknown ? state.Relay.Duration : 0))
                    return false;
            }

            // Approve
            return true;
        }

        public void Dispose()
        {
            foreach (var rank in Enum.GetValues<HuntRank>()) this._huntViews[rank].Dispose();
            this._fateView.Dispose();
        }
    }
}
