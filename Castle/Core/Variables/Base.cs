using Castle.Core.Classes;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using static Castle.Core.Functions.Base;

namespace Castle.Core.Variables
{
    public static class Base
    {
        public static int Hour = 0;
        public static AudioPlayer GlobalPlayer;

        public static List<Player> ChatCooldown = new List<Player>();
        public static List<Player> EmotionCooldown = new List<Player>();
        public static List<Player> HumanMeleeCooldown = new List<Player>();
        public static List<Player> GodModePlayers = new List<Player>();
        public static List<Player> IntercomPlayers = new List<Player>();
        public static List<GameObject> SpawnPoints = new List<GameObject>();
        public static List<ItemType> BlockedItems = new List<ItemType>()
        {
            ItemType.Snowball,
            ItemType.Coal,
            ItemType.SpecialCoal,
            ItemType.SCP1507Tape
        };
        public static List<Products> ShopProducts = new List<Products>()
        {
            new Products()
            {
                Name = "낡은 갑옷",
                Description = "방어력이 영구히 5% 증가합니다. 죽으면 초기화됩니다. (최대 50%)",
                Price = 1,
                Script = (player) =>
                {
                    if (player.GetEffect(EffectType.DamageReduction).Intensity < 100)
                    {
                        player.GetEffect(EffectType.DamageReduction).Intensity += 10;
                        player.SendConsoleMessage($"방어 효과가 성공적으로 증가했습니다. (현재 방어력: {player.GetEffect(EffectType.DamageReduction).Intensity / 2}%)", "white");
                    }

                    else
                    {
                        player.SendConsoleMessage($"방어력이 이미 50%를 넘었습니다. 동전 1개가 반환됩니다.", "white");
                        player.AddItem(ItemType.Coin);
                    }
                }
            },
            new Products()
            {
                Name = "낡은 부츠",
                Description = "이동 속도가 영구히 5% 증가합니다. 죽으면 초기화됩니다. (최대 50%)",
                Price = 1,
                Script = (player) =>
                {
                    if (player.GetEffect(EffectType.MovementBoost).Intensity < 50)
                    {
                        player.GetEffect(EffectType.MovementBoost).Intensity += 5;
                        player.SendConsoleMessage($"추가 이동 속도가 성공적으로 증가했습니다. (현재 추가 이동 속도: {player.GetEffect(EffectType.MovementBoost).Intensity}%)", "white");
                    }

                    else
                    {
                        player.SendConsoleMessage($"추가 이동 속도가 이미 50%를 넘었습니다. 동전 1개가 반환됩니다.", "white");
                        player.AddItem(ItemType.Coin);
                    }
                }
            },
            new Products()
            {
                Name = "마법의 물약",
                Description = "몸의 크기가 3% 줄어듭니다. (제한 없음)",
                Price = 1, Script = (player) =>
                {
                    player.Scale *= 0.97f;
                }
            },
            new Products()
            {
                Name = "확성기",
                Description = "30초 간 확성기가 활성화됩니다.",
                Price = 2,
                Script = (player) =>
                {
                    Server.ExecuteCommand($"/icom {player.Id} 1");

                    IntercomPlayers.Add(player);

                    Timing.CallDelayed(30, () =>
                    {
                        Server.ExecuteCommand($"/icom {player.Id} 0");

                        IntercomPlayers.Remove(player);
                    });
                } 
            },
            new Products()
            {
                Name = "순간이동 허가증",
                Description = "랜덤한 유저의 위치로 이동합니다(본인 포함). 투명이 3초 동안 적용됩니다.",
                Price = 3, Script = (player) =>
                {
                    player.Position = Player.List.Where(x => !x.IsNPC).GetRandomValue().Position;

                    player.EnableEffect(EffectType.Invisible, 1, 3);
                }
            },
            new Products()
            {
                Name = "보따리",
                Description = "즉시 랜덤한 아이템 8개가 당신 아래에 떨궈집니다.",
                Price = 4, Script = (player) => 
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Item item = Item.Create(EnumToList<ItemType>().Where(x => !BlockedItems.Contains(x)).GetRandomValue());

                        item.CreatePickup(player.Position);
                    }
                } 
            },
        };
    }
}
