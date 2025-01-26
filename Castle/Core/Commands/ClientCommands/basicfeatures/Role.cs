using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using MultiBroadcast.API;
using PlayerRoles;
using UnityEngine;

namespace Castle.Core.Commands.ClientCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Role : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            Dictionary<string, RoleTypeId> roles = new Dictionary<string, RoleTypeId>()
            {
                { "D계급", RoleTypeId.ClassD },
                { "과학자", RoleTypeId.Scientist },
                { "경비", RoleTypeId.FacilityGuard },
                { "NTF", RoleTypeId.NtfPrivate },
                { "반란", RoleTypeId.ChaosRifleman },
                { "튜토리얼", RoleTypeId.Tutorial }
            };

            if (arguments.Count() == 0)
            {
                response = $"\n<b>[역할 목록]</b>\n\n{string.Join("\n", roles.Keys)}";
                return false;
            }
            else
            {
                if (roles.Keys.Contains(arguments.At(0)))
                {
                    Vector3 pos = player.Position;

                    player.Role.Set(roles[arguments.At(0)], SpawnReason.ItemUsage, RoleSpawnFlags.None);
                    player.Position = new Vector3(29, 991, -28);

                    Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                    {
                        player.Position = pos;
                    });

                    response = $"당신은 이제 {arguments.At(0)}입니다.";
                    return true;
                }
                else
                {
                    response = $"\n<b>[역할 목록]</b>\n\n{string.Join("\n", roles.Keys)}";
                    return false;
                }
            }
        }

        public string Command { get; } = "역할";

        public string[] Aliases { get; } = { "룰셋" };

        public string Description { get; } = "[만남의 광장] 특정 역할로 변신합니다.";

        public bool SanitizeResponse { get; } = true;
    }
}