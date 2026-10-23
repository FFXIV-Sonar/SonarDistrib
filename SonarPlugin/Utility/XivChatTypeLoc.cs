using Dalamud.Game.Text;
using System;
using System.Collections.Generic;

namespace SonarPlugin.Utility
{
    public static partial class XivChatTypeLoc
    {
        private static readonly Dictionary<XivChatType, string> s_names = new()
        {
            { XivChatType.None, "无" },
            { XivChatType.Echo, "回声" },
            { XivChatType.SystemMessage, "系统信息" },
            { XivChatType.SystemError, "系统错误" },
            { XivChatType.GatheringSystemMessage, "采集系统信息" },
            { XivChatType.ErrorMessage, "错误信息" },
            { XivChatType.NPCDialogue, "NPC对话" },
            { XivChatType.LootNotice, "拾取提示" },
            { XivChatType.Progress, "进度" },
            { XivChatType.LootRoll, "拾取分配" },
            { XivChatType.Crafting, "制作" },
            { XivChatType.Gathering, "采集" },
            { XivChatType.NPCDialogueAnnouncements, "NPC对话播报" },
            { XivChatType.FreeCompanyAnnouncement, "部队公告" },
            { XivChatType.FreeCompanyLoginLogout, "部队上下线" },
            { XivChatType.RetainerSale, "雇员出售" },
            { XivChatType.PeriodicRecruitmentNotification, "定期招募通知" },
            { XivChatType.Sign, "标记" },
            { XivChatType.RandomNumber, "随机数" },
            { XivChatType.NoviceNetworkSystem, "新人频道系统信息" },
            { XivChatType.Orchestrion, "演奏" },
            { XivChatType.PvpTeamAnnouncement, "战队公告" },
            { XivChatType.PvpTeamLoginLogout, "战队上下线" },
            { XivChatType.MessageBook, "传书" },
            { XivChatType.GmTell, "GM密语" },
            { XivChatType.GmSay, "GM说话" },
            { XivChatType.GmShout, "GM喊话" },
            { XivChatType.GmYell, "GM高声喊话" },
            { XivChatType.GmParty, "GM小队" },
            { XivChatType.GmFreeCompany, "GM部队" },
            { XivChatType.GmLinkshell1, "GM通讯贝1" },
            { XivChatType.GmLinkshell2, "GM通讯贝2" },
            { XivChatType.GmLinkshell3, "GM通讯贝3" },
            { XivChatType.GmLinkshell4, "GM通讯贝4" },
            { XivChatType.GmLinkshell5, "GM通讯贝5" },
            { XivChatType.GmLinkshell6, "GM通讯贝6" },
            { XivChatType.GmLinkshell7, "GM通讯贝7" },
            { XivChatType.GmLinkshell8, "GM通讯贝8" },
            { XivChatType.GmNoviceNetwork, "GM新人频道" },
            { XivChatType.CrossLinkShell1, "跨区通讯贝1" },
            { XivChatType.CrossLinkShell2, "跨区通讯贝2" },
            { XivChatType.CrossLinkShell3, "跨区通讯贝3" },
            { XivChatType.CrossLinkShell4, "跨区通讯贝4" },
            { XivChatType.CrossLinkShell5, "跨区通讯贝5" },
            { XivChatType.CrossLinkShell6, "跨区通讯贝6" },
            { XivChatType.CrossLinkShell7, "跨区通讯贝7" },
            { XivChatType.CrossLinkShell8, "跨区通讯贝8" },
            { XivChatType.NoviceNetwork, "新人频道" },
            { XivChatType.CustomEmote, "自定义情感动作" },
            { XivChatType.StandardEmote, "情感动作" },
            { XivChatType.TellOutgoing, "密语（发送）" },
            { XivChatType.TellIncoming, "密语（接收）" },
            { XivChatType.Say, "说话" },
            { XivChatType.Shout, "喊话" },
            { XivChatType.Yell, "高声喊话" },
            { XivChatType.Party, "小队" },
            { XivChatType.CrossParty, "跨区小队" },
            { XivChatType.Alliance, "联合队" },
            { XivChatType.FreeCompany, "部队" },
            { XivChatType.Ls1, "通讯贝1" },
            { XivChatType.Ls2, "通讯贝2" },
            { XivChatType.Ls3, "通讯贝3" },
            { XivChatType.Ls4, "通讯贝4" },
            { XivChatType.Ls5, "通讯贝5" },
            { XivChatType.Ls6, "通讯贝6" },
            { XivChatType.Ls7, "通讯贝7" },
            { XivChatType.Ls8, "通讯贝8" },
            { XivChatType.PvPTeam, "战队" },
            { XivChatType.Debug, "调试" },
            { XivChatType.Urgent, "紧急" },
            { XivChatType.Notice, "通知" },
            { XivChatType.Damage, "伤害信息" },
            { XivChatType.Miss, "未命中信息" },
            { XivChatType.Action, "动作信息" },
            { XivChatType.Item, "物品信息" },
            { XivChatType.Healing, "治疗信息" },
            { XivChatType.GainBuff, "获得增益" },
            { XivChatType.GainDebuff, "获得减益" },
            { XivChatType.LoseBuff, "失去增益" },
            { XivChatType.LoseDebuff, "失去减益" },
            { XivChatType.GlamourNotifications, "幻化通知" },
            { XivChatType.Alarm, "闹钟" },
        };

        /// <summary>Get the localized display name of a chat type, falling back to Dalamud's FancyName.</summary>
        public static string GetDisplayName(this XivChatType type)
        {
            var names = CheapLocSetup.CurrentCode == "tw" ? s_twNames : s_names;
            return names.GetValueOrDefault(type) ?? type.GetDetails()?.FancyName ?? type.ToString();
        }
    }
}
