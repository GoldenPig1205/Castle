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
        public static List<Products> ShopProducts = new List<Products>()
        {
            new Products()
            {
                Name = "확성기",
                Description = "30초 간 확성기가 활성화됩니다.",
                Price = 5,
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
                Description = "랜덤한 유저의 위치로 이동합니다. 투명이 3초 동안 적용됩니다.",
                Price = 10, Script = (player) =>
                {
                    player.Position = Player.List.GetRandomValue().Position;

                    player.EnableEffect(EffectType.Invisible, 1, 3);
                }
            },
            new Products()
            {
                Name = "보따리",
                Description = "즉시 랜덤한 아이템 8개가 당신 아래에 떨궈집니다.",
                Price = 15, Script = (player) => 
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Item item = Item.Create(EnumToList<ItemType>().GetRandomValue());

                        item.CreatePickup(player.Position);
                    }
                } 
            },
        };

        public static Dictionary<Player, int> Coins = new Dictionary<Player, int>();
    }
}
