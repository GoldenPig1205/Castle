using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Classes;
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using InventorySystem.Items.Firearms.Modules;
using MultiBroadcast.API;
using PlayerRoles;
using UnityEngine;

using static Castle.Core.Variables.Base;

namespace Castle.Core.Commands.ClientCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Repair : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (Physics.Raycast(player.Position, Vector3.down, out RaycastHit hit, 3, (LayerMask)1))
            {
                if (hit.transform.name == "Smithy")
                {
                    if (player.CurrentItem != null && player.CurrentItem.Type != ItemType.None)
                    {
                        ItemType itemType = player.CurrentItem.Type;

                        if (new List<ItemType>() { ItemType.ParticleDisruptor, ItemType.MicroHID, ItemType.Jailbird }.Contains(itemType))
                        {
                            if (2 <= player.Items.Where(x => x.Type == ItemType.Coin).Count())
                            {
                                for (int i = 0; i < 2; i++)
                                    player.RemoveItem(player.Items.Where(x => x.Type == ItemType.Coin).FirstOrDefault());

                                player.RemoveItem(player.CurrentItem);
                                player.AddItem(itemType);

                                response = "수리 완료!";
                                return true;
                            }
                            else
                            {
                                response = "코인이 부족합니다.";
                                return false;
                            }
                        }
                        else
                        {
                            response = "<b>특수 아이템</b>만 수리가 가능합니다.";
                            return false;
                        }
                    }
                    else
                    {
                        response = "특정 아이템을 들고 있어야 합니다.";
                        return false;
                    }
                }
                else
                {
                    response = "대장간에서만 사용할 수 있습니다. 특정 건물을 잘 살펴보세요.";
                    return false;
                }
            }
            else
            {
                response = "대장간에서만 사용할 수 있습니다. 특정 건물을 잘 살펴보세요.";
                return false;
            }
        }

        public string Command { get; } = "수리";

        public string[] Aliases { get; } = {};

        public string Description { get; } = "[만남의 광장] 3원을 지불하여 현재 들고 있는 특수 아이템을 수리합니다.";

        public bool SanitizeResponse { get; } = true;
    }
}