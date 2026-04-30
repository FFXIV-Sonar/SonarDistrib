using AG.EnumLocalization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarPlugin.Localization
{
    [EnumLocStrings("Config")]
    public enum ConfigLoc
    {
        [EnumLoc("Contribute.Global", Fallback = "모든 전파 기여")]
        GlobalContribute,

        [EnumLoc("Contribute.GlobalTooltip", Fallback = "마물과 돌발 전파 기여를 모두 비활성화 합니다.\n또한 /sonaron 와 /sonaroff 명령어로도 활성화 및 비활성화가 가능합니다.")]
        GlobalContributeTooltip,

        [EnumLoc("Contribute.Hunts", Fallback = "마물 전파 기여")]
        ContributeHunts,

        [EnumLoc("Contribute.Fates", Fallback = "돌발 전파 기여")]
        ContributeFates,

        [EnumLoc("Contribute.Tooltip", Fallback = "다른 Sonar 유저들로부터 전파 정보를 수신하려면 마물 전파 기여 활성화를 요구합니다.\n기여를 비활성화할 경우 로컬 모드로 작동하고 자신이 직접 발견한 항목만 확인할 수 있으며 다른 유저들로부터 전파 정보를 수신할 수 없습니다.")]
        ContributeTooltip,

        [EnumLoc("Contribute.GlobalDisabled", Fallback = "모든 전파 비활성화")]
        ContributeGlobalDisabled,

        [EnumLoc("TrackAll.Hunts", Fallback = "모든 마물 추적")]
        TrackAllHunts,

        [EnumLoc("TrackAll.Fates", Fallback = "모든 돌발 추적")]
        TrackAllFates,

        [EnumLoc("TrackAll.Tooltip", Fallback = "사용 시: 관할구역 설정에 상관없이 모든 전파를 추적합니다.\n미사용 시: 설정한 관할구역에 포함되는 전파만 추적합니다.")]
        TrackAllTooltip,

        [EnumLoc(Fallback = "수신 관할구역")]
        ReceiveJurisdiction,

    }
}
