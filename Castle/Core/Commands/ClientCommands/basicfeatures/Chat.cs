using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MultiBroadcast.API;
using PlayerRoles;
using UnityEngine;

using static Castle.Core.Variables.Base;
using static Castle.Core.Functions.Base;

using DiscordInteraction.Discord;

using CustomPlayerEffects;
using Exiled.API.Features.Items;
using DiscordInteraction.API.DataBases;
using System.Net.Sockets;
using Mirror;

namespace RGM.Commands.ClientCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Chat : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (ChatCooldown.Contains(player))
            {
                response = "너무 빠른 간격으로 입력을 보내고 있습니다!";
                return false;
            }
            else if (player.IsMuted)
            {
                response = "뮤트된 상태입니다.";
                return false;
            }
            else
            {
                ChatCooldown.Add(player);

                string ChatFormat(string chatType)
                {
                    string text = Trans.Role[player.Role.Type];
                    string text2 = string.Concat(new string[]
                    {
                        $"<size=25><b>{chatType}</b>ㅣ{BadgeFormat(player)}<color={player.Role.Color.ToHex()}>",
                        text,
                        $"</color> ({player.DisplayNickname}) <b> | </b>",
                        string.Join(" ", arguments).Replace("=", "❤️"),
                        "</size>"
                    });

                    bool Check(Player p)
                    {
                        if (player.IsDead && p.CurrentItem is Scp1576 scp1576)
                        {
                            if (scp1576.IsUsing)
                                return true;
                        }

                        if (chatType == "전체 채팅")
                            return true;

                        if (chatType == "SCP 채팅")
                            return p.IsDead || p.IsScp || p.Role.Type == RoleTypeId.ZombieFlamingo;

                        if (chatType == "플라밍고 채팅")
                            return p.IsDead || new List<RoleTypeId>() { RoleTypeId.Flamingo, RoleTypeId.AlphaFlamingo }.Contains(p.Role.Type);

                        if (chatType == "관전자 채팅")
                            return p.IsDead;

                        if (chatType == "근거리 + 무전기 채팅")
                            return p.IsDead || Vector3.Distance(p.Position, player.Position) <= 10 || p.HasItem(ItemType.Radio);

                        if (chatType == "근거리 채팅")
                            return p.IsDead || Vector3.Distance(p.Position, player.Position) <= 10;

                        return false;
                    }

                    foreach (Player ply in Player.List)
                    {
                        if (Check(ply))
                            ply.AddBroadcast(6, text2);
                    }

                    Webhook.Send($"**{chatType}**ㅣ`{player.DisplayNickname}`[{player.IPAddress}, {player.UserId}]({Trans.Role[player.Role.Type]}) - {string.Join(" ", arguments)}");

                    return $"'{text2}'";
                }

                if (arguments.Count == 0)
                {
                    response = "보낼 메세지를 입력해주세요.";
                    return false;
                }

                if (IntercomPlayers.Contains(player))
                {
                    response = ChatFormat("전체 채팅");
                    return true;
                }
                else if (player.IsScp || player.Role.Type == RoleTypeId.ZombieFlamingo)
                {
                    response = ChatFormat("SCP 채팅");
                    return true;
                }
                else if (new List<RoleTypeId>() { RoleTypeId.Flamingo, RoleTypeId.AlphaFlamingo }.Contains(player.Role.Type))
                {
                    response = ChatFormat("플라밍고 채팅");
                    return true;
                }
                else if (player.IsDead)
                {
                    response = ChatFormat("관전자 채팅");
                    return true;
                }
                else if (player.HasItem(ItemType.Radio))
                {
                    response = ChatFormat("근거리 + 무전기 채팅");
                    return true;
                }
                else
                {
                    response = ChatFormat("근거리 채팅");
                    return true;
                }
            }
        }

        public string Command { get; } = "ㅊ";
        public string[] Aliases { get; } = new string[] { "챗", "채팅", "chat", "c" };
        public string Description { get; } = "[만남의 광장] 텍스트 채팅을 사용할 수 있습니다.";
    }
}
