using Castle.Core.Classes;
using CommandSystem.Commands.RemoteAdmin.Dummies;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using Exiled.CustomItems.API.EventArgs;
using Exiled.Events.EventArgs.Player;
using GameCore;
using InventorySystem.Items.Coin;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using MapEditorReborn.Commands.ModifyingCommands.Position;
using MEC;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Subroutines;
using PlayerStatsSystem;
using RelativePositioning;
using RemoteAdmin;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static List<ItemType> SpeicalWeapons = new List<ItemType>()
        {
            ItemType.ParticleDisruptor,
            ItemType.MicroHID,
            ItemType.Jailbird
        };
        public static List<Products> ShopProducts = new List<Products>()
        {
            new Products()
            {
                Name = "낡은 갑옷",
                Description = "방어력이 영구히 2% 증가합니다. 죽으면 초기화됩니다. (최대 50%)",
                Price = 1,
                Script = (player) =>
                {
                    if (player.GetEffect(EffectType.DamageReduction).Intensity < 100)
                    {
                        player.GetEffect(EffectType.DamageReduction).Intensity += 4;
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
                Description = "이동 속도가 영구히 2% 증가합니다. 죽으면 초기화됩니다. (최대 50%)",
                Price = 1,
                Script = (player) =>
                {
                    if (player.GetEffect(EffectType.MovementBoost).Intensity < 50)
                    {
                        player.GetEffect(EffectType.MovementBoost).Intensity += 2;
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
                Description = "몸의 크기가 2% 줄어듭니다. 죽으면 초기화됩니다. (최대 50%)",
                Price = 1, Script = (player) =>
                {
                    if (player.Scale.x > 0.5f)
                        player.Scale *= 0.98f;

                    else
                    {
                        player.SendConsoleMessage($"몸의 크기가 이미 50% 이상 줄어들었습니다. 동전 1개가 반환됩니다.", "white");
                        player.AddItem(ItemType.Coin);
                    }
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
                Description = "즉시 랜덤한 아이템 3개를 얻습니다. 모든 아이템의 등장 확률은 동일합니다.",
                Price = 4, Script = (player) => 
                {
                    for (int i = 0; i < 3; i++)
                        player.AddItem(EnumToList<ItemType>().Where(x => !BlockedItems.Contains(x)).GetRandomValue());
                } 
            },
        };
        public static List<Events> Events = new List<Events>()
        {
            new Events()
            {
                Name = "코인 샤워",
                Description = "3분 동안 더 많은 코인이 맵 곳곳에 스폰됩니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        for (int _ = 0; _ < 180; _++)
                        {
                            for (int i = 0; i < Random.Range(1, 3); i++)
                            {
                                Item coin = Item.Create(ItemType.Coin);

                                coin.CreatePickup(new Vector3(Random.Range(-45, 42), Random.Range(2019, 2001), Random.Range(0, 254)), new Quaternion(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)));
                            }

                            yield return Timing.WaitForSeconds(1);
                        }
                    }

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
            new Events()
            {
                Name = "보급",
                Description = "강력한 전리품이 든 보급품이 즉시 맵 어딘가에 떨어집니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        SchematicObject distribution = ObjectSpawner.SpawnSchematic("Distribution", new Vector3(Random.Range(-45, 42), 2050, Random.Range(0, 254)), new Quaternion(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)), null, null);

                        yield return Timing.WaitForSeconds(180);

                        distribution.Destroy();
                        GameObject.Destroy(distribution.gameObject);
                        GameObject.DestroyImmediate(distribution.gameObject);
                    }

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
            new Events()
            {
                Name = "아포칼립스",
                Description = "좀비 때가 왕국을 덮칩니다. 좀비를 잡으면 전리품을 떨어트립니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        IEnumerator<float> onSpawnZombie(ReferenceHub hub)
                        {
                            Player owner = Player.Get(hub);
                            Scp0492Role scp0492Role =  owner.Role.Cast<Scp0492Role>();
                            MethodInfo attackMethod = typeof(ScpAttackAbilityBase<ZombieRole>).GetMethod("ServerPerformAttack", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                            yield return Timing.WaitForSeconds(1f);

                            hub.playerStats.GetModule<HealthStat>().CurValue = 500;
                            owner.RankName = "무서운";

                            List<ItemType> poll = new List<ItemType>();
                            List<ItemType> items = new List<ItemType>()
                            {
                                ItemType.GrenadeHE,
                                ItemType.GrenadeFlash,
                                ItemType.Coin,
                                ItemType.GunA7,
                                ItemType.GunFSP9,
                                ItemType.GunCrossvec,
                                ItemType.Adrenaline,
                                ItemType.SCP500
                            };

                            poll.Add(items.GetRandomValue());
                            poll.Add(ItemType.Coin);

                            foreach (var item in poll)
                                owner.AddItem(item);

                            while (hub.playerStats.GetModule<HealthStat>().CurValue > 0)
                            {
                                Player target = Player.List.Where(x => !x.IsNPC).OrderBy(x => Vector3.Distance(hub.transform.position, x.Position)).FirstOrDefault();
                                PlayerFollower obj;
                                float distance = Vector3.Distance(target.Position, hub.GetPosition());

                                if (hub.TryGetComponent(out obj))
                                    Object.Destroy(obj);

                                hub.gameObject.AddComponent<PlayerFollower>().Init(target.ReferenceHub, 125f, 1, 30f);

                                if (distance < 4)
                                    owner.EnableEffect(EffectType.Slowness, 20, 2);

                                if (distance < 2) 
                                {
                                    var attack = owner.Role.As<Scp0492Role>().AttackAbility;

                                    var writer = new NetworkWriter();

                                    writer.WriteRelativePosition(default);

                                    attack.ServerProcessCmd(new NetworkReader(new System.ArraySegment<byte>(writer.ToArray())));
                                    attack.ServerPerformAttack();

                                    attackMethod.Invoke(scp0492Role.AttackAbility, null);
                                }

                                Vector3 pos = hub.transform.position;
                                
                                hub.transform.position = new Vector3(pos.x, pos.y + 2f, pos.z);

                                yield return Timing.WaitForSeconds(2);
                            }

                            NetworkServer.Destroy(hub.gameObject);
                        }

                        for (int i = 0; i < Player.List.Count() * 2; i++)
                        {
                            ReferenceHub zombie = DummyUtils.SpawnDummy("좀비");

                            zombie.roleManager.ServerSetRole(PlayerRoles.RoleTypeId.Scp0492, PlayerRoles.RoleChangeReason.ItemUsage);
                            zombie.transform.position = new Vector3(Random.Range(-45, 42), 2030, Random.Range(0, 254));

                            Timing.RunCoroutine(onSpawnZombie(zombie));

                            yield return Timing.WaitForSeconds(Random.Range(1, 6));
                        }
                    }

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
            new Events()
            {
                Name = "범죄와의 전쟁",
                Description = "왕국 치안 경비대가 치안을 강화합니다. 3분간 아무도 서로를 죽일 수 없습니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        for (int i = 0; i < 180; i++)
                        {
                            foreach (var player in Player.List)
                            {
                                if (!GodModePlayers.Contains(player))
                                    GodModePlayers.Add(player);
                            }

                            yield return Timing.WaitForSeconds(1);
                        }
                    }

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
            new Events()
            {
                Name = "밀수입",
                Description = "2분 간, 12개의 무기가 맵 곳곳에 스폰됩니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            Item weapon = Item.Create(EnumToList<ItemType>().Where(x => x.IsWeapon()).GetRandomValue());

                            weapon.CreatePickup(new Vector3(Random.Range(-45, 42), Random.Range(2019, 2001), Random.Range(0, 254)), new Quaternion(Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180), Random.Range(0, 180)));

                            yield return Timing.WaitForSeconds(10);
                        }
                    }

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
            new Events()
            {
                Name = "무료 봉사",
                Description = "교회에서 사람들을 위해 거리 곳곳에 회복 포션을 뿌렸습니다. 이 효과는 3분 간 지속됩니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        for (int i = 0; i < 180; i++)
                        {
                            foreach (var player in Player.List)
                                player.Heal(2);

                            yield return Timing.WaitForSeconds(1);
                        }
                    }

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
            new Events()
            {
                Name = "노름판",
                Description = "1분 간, 아이템을 버리면 새로운 아이템으로 교환됩니다. 단, 5% 확률로 손이 잘립니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        void OnDroppingItem(DroppingItemEventArgs ev)
                        {
                            if (ev.Player.IsScp || ev.Player.Role.Type.ToString().Contains("Flamingo"))
                                return;

                            List<ItemType> ItemList = EnumToList<ItemType>();
                            ItemType Item = ItemList.GetRandomValue();

                            int rand = Random.Range(0, 100);

                            if (rand < 5)
                                ev.Player.EnableEffect(EffectType.SeveredHands);

                            else
                            {
                                ev.Item.Destroy();
                                Item CurrentItem = ev.Player.AddItem(Item);
                                ev.Player.DropItem(CurrentItem);
                            }
                        }

                        Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;

                        yield return Timing.WaitForSeconds(60);

                        Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
    }

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
            new Events()
            {
                Name = "광장",
                Description = "2분 간, 유저들의 채팅과 마이크가 모두에게 공유됩니다.",
                Script = () =>
                {
                    IEnumerator<float> OnEventStarted()
                    {
                        foreach (var player in Player.List)
                        {
                            Server.ExecuteCommand($"/icom {player.Id} 1");

                            IntercomPlayers.Add(player);

                            Timing.CallDelayed(120, () =>
                            {
                                Server.ExecuteCommand($"/icom {player.Id} 0");

                                IntercomPlayers.Remove(player);
                            });
                        }

                        yield break;
                    };

                    Timing.RunCoroutine(OnEventStarted());
                }
            },
        };

        public static Dictionary<Player, int> KillCounts = new Dictionary<Player, int>();
    }
}
