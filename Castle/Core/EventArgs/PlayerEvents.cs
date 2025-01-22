using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MEC;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Enums;

using static Castle.Core.Variables.Base;

using static Castle.Core.Functions.Base;

using Interactables.Interobjects.DoorUtils;
using Exiled.API.Features.DamageHandlers;
using DiscordInteraction.API.DataBases;
using Exiled.API.Extensions;
using Mirror;
using MultiBroadcast.API;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using Exiled.API.Features;

namespace Castle.Core.EventArgs
{
    public static class PlayerEvents
    {
        public static void OnVerified(VerifiedEventArgs ev)
        {
            Timing.RunCoroutine(Verified(ev.Player));
        }

        public static IEnumerator<float> Verified(Player player)
        {
            yield return Timing.WaitForSeconds(1);

            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Player - {player.UserId}", condition: (hub) =>
            {
                return hub == player.ReferenceHub;
            }
            , onIntialCreation: (p) =>
            {
                p.transform.parent = player.GameObject.transform;

                Speaker speaker = p.AddSpeaker("Main", isSpatial: false, minDistance: 0, maxDistance: 1000);

                speaker.transform.parent = player.GameObject.transform;
                speaker.transform.localPosition = Vector3.zero;
            });

            Spawn(player);

            yield return Timing.WaitForSeconds(1);

            player.Position = GameObject.Find("[SpawnPoint] Start").transform.position;
            player.EnableEffect(EffectType.Ensnared, 1, 5);
            player.IsGodModeEnabled = true;

            yield return Timing.WaitForSeconds(5);

            player.IsGodModeEnabled = false;
        }

        public static void OnLeft(LeftEventArgs ev)
        {
        }

        public static void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player.IsAlive)
            {
                Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                {
                    Vector3 pos = SpawnPoints.Select(x => x.transform.position).GetRandomValue();

                    ev.Player.Position = new Vector3(pos.x, pos.y + 2, pos.z);
                    ev.Player.EnableEffect(EffectType.FogControl, 1);
                    ev.Player.EnableEffect(EffectType.SoundtrackMute);
                    ev.Player.EnableEffect(EffectType.SilentWalk, 8);

                    int intensity = CalculateIntensity(Hour);

                    ev.Player.EnableEffect(EffectType.Blinded, (byte)intensity);
                });
            }
        }

        public static void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (GodModePlayers.Contains(ev.Player))
                ev.IsAllowed = false;
        }

        public static void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null)
                return;

            if (GodModePlayers.Contains(ev.Attacker) || GodModePlayers.Contains(ev.Player))
                ev.IsAllowed = false;
        }

        public static IEnumerator<float> OnDied(DiedEventArgs ev)
        {
            for (int i = 0; i < 5; i++)
            {
                ev.Player.ShowHint($"{5 - i}초 뒤 부활합니다.", 1.2f);

                yield return Timing.WaitForSeconds(1);
            }

            string MessageFormat()
            {
                if (ev.Attacker == null)
                    return $"💀 <color=#A4A4A4>자살</color>ㅣ{BadgeFormat(ev.Player)}<color=#F2F5A9>{ev.Player.DisplayNickname}</color>(<color={ev.TargetOldRole.GetColor().ToHex()}>{Trans.Role[ev.TargetOldRole]}</color>) - {ev.DamageHandler.Type}";
                else
                    return $"💔 <color=#FAAC58>{(ev.Player.IsCuffed ? "<b>체포킬</b>(신고 가능 여부는 규칙 확인)" : "사살")}</color>ㅣ{BadgeFormat(ev.Attacker)}<color=#F2F5A9>{ev.Attacker.DisplayNickname}</color>(<color={ev.Attacker.Role.Color.ToHex()}>{Trans.Role[ev.Attacker.Role.Type]}</color>) -> {BadgeFormat(ev.Player)}<color=#F2F5A9>{ev.Player.DisplayNickname}</color>(<color={ev.TargetOldRole.GetColor().ToHex()}>{Trans.Role[ev.TargetOldRole]}</color>) - {ev.DamageHandler.Type}";
            }
            foreach (var player in Exiled.API.Features.Player.List.Where(x => x.IsDead || x == ev.Attacker))
                player.AddBroadcast(10, $"<size=20>{MessageFormat()}</size>");

            Spawn(ev.Player);
        }

        public static void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (!ev.Door.IsOpen)
            {
                Timing.CallDelayed(3, () =>
                {
                    if (ev.Door.IsOpen)
                    {
                        ev.Player.IsBypassModeEnabled = true;

                        DoorVariant.AllDoors.Where(x => x.DoorId == ev.Door.Base.DoorId).FirstOrDefault().ServerInteract(ev.Player.ReferenceHub, 1);

                        if (!ev.Player.IsNTF)
                            ev.Player.IsBypassModeEnabled = false;
                    }
                });
            }
        }

        public static void OnFlippingCoin(FlippingCoinEventArgs ev)
        {
            ev.Player.ShowHint("이 동전으로 <b><color=#FE642E>상점</color></b>에서 아이템을 구매할 수 있습니다.", 2);
        }

        public static IEnumerator<float> OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            if (ev.Player.IsHuman && !ev.Player.IsCuffed)
            {
                if (TryGetLookPlayer(ev.Player, 2f, out Exiled.API.Features.Player player, out RaycastHit? hit))
                {
                    if (ev.Player != player && !HumanMeleeCooldown.Contains(ev.Player) && !GodModePlayers.Contains(ev.Player) && !GodModePlayers.Contains(player))
                    {
                        float damageCalcu(string pos)
                        {
                            switch (pos)
                            {
                                case "Head":
                                    return 24.1f;

                                case "Chest":
                                    return 14f;

                                default:
                                    return 12.5f;
                            }
                        }

                        float damage = damageCalcu(hit.Value.transform.name);

                        ev.Player.ShowHitMarker(damage / 14);
                        player.Hurt(ev.Player, damage, DamageType.Custom, new DamageHandlerBase.CassieAnnouncement("") { Announcement = null, SubtitleParts = null }, "무지성으로 뚜드려 맞았습니다.");

                        HumanMeleeCooldown.Add(ev.Player);

                        yield return Timing.WaitForSeconds(1);

                        HumanMeleeCooldown.Remove(ev.Player);
                    }
                }
            }
        }

        public static void OnChangedEmotion(ChangedEmotionEventArgs ev)
        {
            if (!EmotionCooldown.Contains(ev.Player))
            {
                EmotionCooldown.Add(ev.Player);

                EmotionPresetType type = ev.EmotionPresetType;

                if (type == EmotionPresetType.Neutral)
                    return;

                string emotion()
                {
                    if (type == EmotionPresetType.Happy)
                        return "행복한 표정을 짓고 있습니다";

                    else if (type == EmotionPresetType.AwkwardSmile)
                        return "뒤틀린 미소를 짓고 있습니다";

                    else if (type == EmotionPresetType.Scared)
                        return "두려운 표정을 짓고 있습니다";

                    else if (type == EmotionPresetType.Angry)
                        return "화가난 표정을 짓고 있습니다";

                    else if (type == EmotionPresetType.Chad)
                        return "꼭 채드처럼 보이는군요";

                    else
                        return "꼭 오우거같이 보이는군요";
                }

                foreach (var player in Exiled.API.Features.Player.List.Where(x => x.IsDead || Vector3.Distance(x.Position, ev.Player.Position) < 11))
                    player.AddBroadcast(5, $"<size=20>{BadgeFormat(ev.Player)}<color={ev.Player.Role.Color.ToHex()}>{ev.Player.DisplayNickname}</color>(은)는 {emotion()}.</size>");
            }
        }
    }
}
