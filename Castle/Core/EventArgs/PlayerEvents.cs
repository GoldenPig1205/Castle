using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MapEditorReborn.API.Features;
using UnityEngine;
using MEC;
using InventorySystem.Configs;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Enums;
using Exiled.Events.Handlers;
using PlayerRoles;

using static Castle.Core.Variables.Base;

using static Castle.Core.Functions.Base;
using Interactables.Interobjects.DoorUtils;
using Exiled.API.Extensions;

namespace Castle.Core.EventArgs
{
    public static class PlayerEvents
    {
        public static IEnumerator<float> OnVerified(VerifiedEventArgs ev)
        {
            yield return Timing.WaitForSeconds(1);

            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Player - {ev.Player.UserId}", condition: (hub) =>
            {
                return hub == ev.Player.ReferenceHub;
            }
            , onIntialCreation: (p) =>
            {
                p.transform.parent = ev.Player.GameObject.transform;

                Speaker speaker = p.AddSpeaker("Main", isSpatial: false, minDistance: 0, maxDistance: 1000);

                speaker.transform.parent = ev.Player.GameObject.transform;
                speaker.transform.localPosition = Vector3.zero;
            });

            ev.Player.Role.Set(EnumToList<RoleTypeId>().Where(x => x.IsHuman()).GetRandomValue());
            ev.Player.ClearInventory();
        }

        public static void OnLeft(LeftEventArgs ev)
        {
        }

        public static void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player.IsAlive)
            {
                ev.Player.Position = GameObject.Find("[SpawnPoint] Start").transform.position;
                ev.Player.EnableEffect(EffectType.FogControl, 1);
                ev.Player.EnableEffect(EffectType.SoundtrackMute);
                ev.Player.EnableEffect(EffectType.SilentWalk, 8);

                int intensity = CalculateIntensity(Hour);

                ev.Player.EnableEffect(EffectType.Blinded, (byte)intensity);
            }
        }

        public static IEnumerator<float> OnDied(DiedEventArgs ev)
        {
            for (int i = 0; i < 5; i++)
            {
                ev.Player.ShowHint($"{5 - i}초 뒤 부활합니다.", 1.2f);

                yield return Timing.WaitForSeconds(1);
            }

            ev.Player.Role.Set(EnumToList<RoleTypeId>().Where(x => x.IsHuman()).GetRandomValue());
            ev.Player.ClearInventory();
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
    }
}
