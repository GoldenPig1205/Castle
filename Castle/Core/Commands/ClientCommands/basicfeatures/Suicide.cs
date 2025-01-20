using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MultiBroadcast.API;
using PlayerRoles;
using UnityEngine;

namespace Castle.Core.Commands.ClientCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Suicide : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player.IsAlive && Round.IsStarted)
            {
                player.Kill("벌레를 피하다가 사망하였습니다.");
                response = "당신의 기도는 저 하늘에 닿았습니다.";
                return true;
            }
            else
            {
                response = "이미 하늘나라에 있는 상태입니다.";
                return false;
            }
        }

        public string Command { get; } = "자살";

        public string[] Aliases { get; } = { "살자", "suicide" };

        public string Description { get; } = "[만남의 광장] 스스로 생을 마감할 수 있습니다.";

        public bool SanitizeResponse { get; } = true;
    }
}