using Dalamud.Game.Text;
using System;
using System.Collections.Generic;

namespace SonarPlugin.Utility
{
    public static class XivChatTypeUtils
    {
        private static IReadOnlyDictionary<string, XivChatType>? s_chatTypes;
        public static IReadOnlyDictionary<string, XivChatType> ChatTypes
        {
            get
            {
                if (s_chatTypes is null)
                {
                    var dict = new Dictionary<string, XivChatType>(comparer: StringComparer.InvariantCultureIgnoreCase);
                    foreach (var value in Enum.GetValues<XivChatType>())
                    {
                        dict.TryAdd(value.ToString(), value);
                    }
                    foreach (var value in Enum.GetValues<XivChatType>())
                    {
                        var details = value.GetDetails();
                        if (details is null) continue;
                        dict.TryAdd(details.FancyName, value);
                    }
                    s_chatTypes = dict;
                }
                return s_chatTypes;
            }
        }
        public static XivChatType GetValueFromInfoAttribute(string name) => ChatTypes.GetValueOrDefault(name);

        public static readonly Dictionary<string, string> KoreanNames = new()
        {
            { "None", "없음" },
            { "Debug", "디버그" },
            { "Urgent", "긴급" },
            { "Notice", "공지" },
            { "Say", "말하기" },
            { "Shout", "외치기" },
            { "TellOutgoing", "보낸 귓속말" },
            { "Tell", "받은 귓속말" },
            { "Party", "파티" },
            { "Alliance", "연합 파티" },
            { "Linkshell1", "링크셸 1" },
            { "Linkshell2", "링크셸 2" },
            { "Linkshell3", "링크셸 3" },
            { "Linkshell4", "링크셸 4" },
            { "Linkshell5", "링크셸 5" },
            { "Linkshell6", "링크셸 6" },
            { "Linkshell7", "링크셸 7" },
            { "Linkshell8", "링크셸 8" },
            { "FreeCompany", "자유부대" },
            { "NoviceNetwork", "초보자" },
            { "CustomEmotes", "사용자 설정 감정표현" },
            { "StandardEmotes", "감정표현" },
            { "Yell", "떠들기" },
            { "PvPTeam", "PvP팀" },
            { "CrossworldLinkshell1", "서버 초월 링크셸 1" },
            { "Echo", "혼잣말" },
            { "SystemMessage", "시스템 메시지" },
            { "SystemError", "시스템 오류" },
            { "GatheringSystemMessage", "채집 관련 시스템 메시지" },
            { "ErrorMessage", "오류 메시지" },
            { "NPCDialogue", "NPC 대화" },
            { "NPCDialogueAnnouncements", "NPC 대화(알림)" },
            { "RetainerSale", "장터 판매 알림" },
            { "CrossworldLinkshell2", "서버 초월 링크셸 2" },
            { "CrossworldLinkshell3", "서버 초월 링크셸 3" },
            { "CrossworldLinkshell4", "서버 초월 링크셸 4" },
            { "CrossworldLinkshell5", "서버 초월 링크셸 5" },
            { "CrossworldLinkshell6", "서버 초월 링크셸 6" },
            { "CrossworldLinkshell7", "서버 초월 링크셸 7" },
            { "CrossworldLinkshell8", "서버 초월 링크셸 8" },
        };

    }
}