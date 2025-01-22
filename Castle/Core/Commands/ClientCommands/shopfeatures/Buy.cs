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
    public class Buy : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string input = string.Join(" ", arguments);
            Player player = Player.Get(sender);

            if (Physics.Raycast(player.Position, Vector3.down, out RaycastHit hit, 3, (LayerMask)1))
            {
                if (hit.transform.name == "Shop")
                {
                    if (ShopProducts.Select(x => x.Name).Contains(input))
                    {
                        Products product = ShopProducts.FirstOrDefault(x => x.Name == input);

                        if (product.Price <= player.Items.Where(x => x.Type == ItemType.Coin).Count())
                        {
                            for (int i = 0; i < product.Price; i++)
                                player.RemoveItem(player.Items.Where(x => x.Type == ItemType.Coin).FirstOrDefault());

                            product.Script.Invoke(player);

                            response = "구매 완료!";
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
                        response = $"\n<b>[상점 품목 목록]</b>\n\n{string.Join("\n", ShopProducts.Select(x => $"{x.Name}(${x.Price}) - {x.Description}"))}\n\n구매하려면 [.구매 (품목 이름)]을 입력합니다.";
                        return false;
                    }
                }
                else
                {
                    response = "상점에서만 사용할 수 있습니다. 특정 건물을 잘 살펴보세요.";
                    return false;
                }
            }
            else
            {
                response = "상점에서만 사용할 수 있습니다. 특정 건물을 잘 살펴보세요.";
                return false;
            }
        }

        public string Command { get; } = "구매";

        public string[] Aliases { get; } = { "상점" };

        public string Description { get; } = "[만남의 광장] 상점을 확인해보세요. (구매하려면 이름을 매개 변수에 입력합니다.)";

        public bool SanitizeResponse { get; } = true;
    }
}