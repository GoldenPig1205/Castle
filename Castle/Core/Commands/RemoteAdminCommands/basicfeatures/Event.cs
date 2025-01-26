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

namespace RGM.Commands.RemoteAdminCommands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Event : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string input = string.Join(" ", arguments);

            if (Events.Select(x => x.Name).Contains(input))
            {
                Castle.Core.Classes.Events ev = Events.Find(x => x.Name == input);

                ev.Script.Invoke();

                foreach (var player in Player.List)
                    player.AddBroadcast(10, $"<size=30><b>[ <color=#FAAC58>이벤트 발생</color> ]</b></size>\n<size=25>{ev.Name}ㅣ{ev.Description}</size>");

                response = $"이벤트 {input}을(를) 실행합니다.\n[Success]";
                return true;
            }
            else
            {
                response = $"이벤트 {input}을(를) 찾을 수 없습니다.\n\n{string.Join("\n", Events.Select(x => x.Name))}\n[Error]";
                return false;
            }
        }

        public string Command { get; } = "이벤트";
        public string[] Aliases { get; } = new string[] { "ev" };
        public string Description { get; } = "[만남의 광장] 이벤트를 실행하세요.";
    }
}
