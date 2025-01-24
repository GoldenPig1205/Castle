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
using Scp914;

using static Castle.Core.Variables.Base;
using Scp914.Processors;
using InventorySystem.Items;

namespace Castle.Core.Commands.ClientCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Upgrade : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            string input = string.Join(" ", arguments);

            Dictionary<string, Scp914KnobSetting> keyValuePairs = new Dictionary<string, Scp914KnobSetting>
            {
                { "매우 굵음", Scp914KnobSetting.Rough },
                { "굵음", Scp914KnobSetting.Coarse },
                { "1:1", Scp914KnobSetting.OneToOne },
                { "고움", Scp914KnobSetting.Fine },
                { "매우 고움", Scp914KnobSetting.VeryFine }
            };

            if (keyValuePairs.Keys.Contains(input))
            {
                if (Physics.Raycast(player.Position, Vector3.down, out RaycastHit hit, 3, (LayerMask)1))
                {
                    if (hit.transform.name == "Smithy")
                    {
                        if (player.CurrentItem != null && player.CurrentItem.Type != ItemType.None)
                        {
                            if (3 <= player.Items.Where(x => x.Type == ItemType.Coin).Count())
                            {
                                for (int i = 0; i < 3; i++)
                                    player.RemoveItem(player.Items.Where(x => x.Type == ItemType.Coin).FirstOrDefault());

                                if (Scp914Upgrader.TryGetProcessor(player.CurrentItem.Type, out Scp914ItemProcessor processor))
                                    processor.UpgradeInventoryItem(keyValuePairs[input], player.CurrentItem.Base);

                                response = "강화 완료! 결과가 어떻게 되었을까요?";
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
            else
            {
                response = $"\n<b>[SCP-914 강화 옵션]</b>\n\n{string.Join("\n", keyValuePairs.Keys)}";
                return false;
            }
        }

        public string Command { get; } = "강화";

        public string[] Aliases { get; } = { };

        public string Description { get; } = "[만남의 광장] 4원을 주고 아이템을 강화할 수 있습니다.";

        public bool SanitizeResponse { get; } = true;
    }
}