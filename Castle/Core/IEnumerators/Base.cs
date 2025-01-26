using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using static Castle.Core.Variables.Base;

using static Castle.Core.Functions.Base;
using Exiled.API.Extensions;
using PlayerRoles;
using Exiled.API.Features.Items;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Roles;
using Exiled.Events.Commands.Hub;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;
using Exiled.API.Features.Pickups;
using MapEditorReborn.API.Extensions;
using PluginAPI.Core.Attributes;
using MultiBroadcast.API;

namespace Castle.Core.IEnumerators
{
    public static class Base
    {
        public static IEnumerator<float> Timer()
        {
            while (true)
            {
                if (Hour == 24)
                    Hour = 0;

                Hour++;

                int intensity = CalculateIntensity(Hour);

                foreach (var player in Player.List)
                {
                    player.EnableEffect(EffectType.Blinded, (byte)intensity);
                    player.AddBroadcast(10, $"<size=25><b>현재 시간은 {Hour}시입니다.</b></size>");
                }

                foreach (string method in new List<string>() { "bulletholes", "blood", "ragdolls" })
                    Server.ExecuteCommand($"/cleanup {method}");

                foreach (var door in Door.List)
                {
                    if (door is BreakableDoor breakableDoor)
                        breakableDoor.Repair();
                }

                yield return Timing.WaitForSeconds(180);
            }
        }

        public static IEnumerator<float> BGM()
        {
            while (true)
            {
                AudioClipPlayback clip = GlobalPlayer.AddClip(AudioClipStorage.AudioClips.Keys.GetRandomValue(), 0.2f, false);

                yield return Timing.WaitForSeconds((int)clip.Duration.TotalSeconds + Random.Range(1, 21));
            }
        }

        public static IEnumerator<float> InputCooldown()
        {
            while (true)
            {
                ChatCooldown.Clear();
                EmotionCooldown.Clear();

                yield return Timing.WaitForSeconds(2f);
            }
        }

        public static IEnumerator<float> ItemSpawner()
        {
            while (true)
            {
                try
                {
                    switch (Random.Range(1, 4)) 
                    {
                        case 1:
                            for (int i = 0; i < Random.Range(1, 3); i++)
                            {
                                Item coin = Item.Create(ItemType.Coin);

                                coin.CreatePickup(new Vector3(Random.Range(-45, 42), Random.Range(2019, 2001), Random.Range(0, 254)), new Quaternion(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)));
                            }
                            break;

                        case 2:
                            Item ammo = Item.Create(EnumToList<ItemType>().GetRandomValue(x => x.IsAmmo()));

                            ammo.CreatePickup(new Vector3(Random.Range(-45, 42), Random.Range(2019, 2001), Random.Range(0, 254)), new Quaternion(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)));
                            break;

                        case 3:
                            Item medical = Item.Create(EnumToList<ItemType>().GetRandomValue(x => new List<ItemType>() { ItemType.Medkit, ItemType.Painkillers }.Contains(x)));

                            medical.CreatePickup(new Vector3(Random.Range(-45, 42), Random.Range(2019, 2001), Random.Range(0, 254)), new Quaternion(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)));
                            break;
                    }

                    List<ItemType> pistols = new List<ItemType>()
                    { 
                        ItemType.GunCOM15,
                        ItemType.GunCOM18,
                        ItemType.GunRevolver
                    };

                    ItemType itemType = EnumToList<ItemType>().GetRandomValue();

                    if (itemType.ToString().Contains("Gun") || SpeicalWeapons.Contains(itemType))
                    {
                        if (Random.Range(1, 101) == 1)
                        {

                        }    
                        else if (Random.Range(1, 4) > 1)
                        {
                            itemType = EnumToList<ItemType>().Where(x => !(x.ToString().Contains("Gun") || SpeicalWeapons.Contains(x))).GetRandomValue();
                        }
                        else
                        {
                            itemType = pistols.GetRandomValue();
                        }
                    }

                    Pickup.CreateAndSpawn(itemType, new Vector3(Random.Range(-45, 42), Random.Range(2019, 2001), Random.Range(0, 254)), new Quaternion(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)));
                }
                catch {}

                yield return Timing.WaitForSeconds(1);
            }
        }

        public static IEnumerator<float> Platform()
        {
            while (true)
            {
                foreach (var player in Player.List)
                {
                    if (Physics.Raycast(player.Position, Vector3.down, out RaycastHit hit, 3, (LayerMask)1))
                    {
                        string name = hit.transform.name;

                        if (name == "[Platform] Start")
                        {
                            player.ShowHint($"""
<b><size=60>Welcome to <color=#F5D0A9>만남의 광장</color>!</size></b>

<size=20>[ESC] -> [Server Info]로 이동하여 서버 설명을 읽어보세요.</size>





""", 1.2f);
                        }
                        else if (name == "[Platform] Hidden")
                            player.ShowHint($"어떻게 하면 더 멀리 바라볼 수 있을까?", 1.2f);

                        else if (name == "Peace")
                            player.ShowHint($"이 지역은 <b><color=#F5ECCE>평화 구역</color></b>입니다. 무적이 적용됩니다.", 1.2f);

                        else if (name == "Shop")
                            player.ShowHint($"이 건물은 <b><color=#FE642E>상점</color></b>입니다. [.구매] 명령어를 사용해보세요.", 1.2f);

                        else if (name == "Smithy")
                            player.ShowHint($"이 건물은 <b><color=#A9D0F5>대장간</color></b>입니다. [.수리] 또는 [.강화] 명령어를 사용해보세요.", 1.2f);

                        else if (name == "Church")
                        {
                            if (KillCounts[player] > 0)
                            {
                                if (GodModePlayers.Contains(player))
                                    GodModePlayers.Remove(player);

                                player.ShowHint($"<color=#FA5858>살인자</color>는 신성한 가호를 받을 수 없습니다.", 1.2f);
                            }
                            else
                            {
                                if (!GodModePlayers.Contains(player))
                                    GodModePlayers.Add(player);

                                player.ShowHint($"이 건물은 <b><color=#BDBDBD>교회</color></b>입니다. 신성한 곳에서는 싸움을 금합니다.", 1.2f);
                            }
                        }
                        else
                        {
                            if (GodModePlayers.Contains(player))
                                GodModePlayers.Remove(player);
                        }
                    }
                }

                yield return Timing.WaitForSeconds(1);
            }
        }

        public static IEnumerator<float> Guide()
        {
            while (true)
            {
                var wantedPlayers = KillCounts.Where(kvp => kvp.Value >= 3).Select(kvp => $"{kvp.Key.Nickname}({kvp.Value}킬)").ToList();

                if (wantedPlayers.Count() > 0)
                {
                    foreach (var player in Player.List)
                        player.AddBroadcast(1, $"<size=25><b>[ <color=red>현상수배</color> ]</b></size>\n<size=20>{string.Join(", ", wantedPlayers)}</size>");
                }

                yield return Timing.WaitForSeconds(1);
            }
        }

        public static IEnumerator<float> Event()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(Random.Range(60 * 3, 60 * 10));

                Classes.Events ev = Events.GetRandomValue();

                ev.Script.Invoke();
                
                foreach (var player in Player.List)
                    player.AddBroadcast(10, $"<size=30><b>[ <color=#FAAC58>이벤트 발생</color> ]</b></size>\n<size=25>{ev.Name}ㅣ{ev.Description}</size>");
            }
        }
    }
}
