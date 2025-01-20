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

using static Castle.Core.IEnumerators.Base;

using static Castle.Core.Variables.Base;
using Respawning;
using Exiled.Events.EventArgs.Server;

namespace Castle.Core.EventArgs
{
    public static class ServerEvents
    {
        public static IEnumerator<float> OnWaitingForPlayers()
        {
            yield return Timing.WaitForSeconds(1);

            Map.IsDecontaminationEnabled = false;
            foreach (var spawn in WaveManager.Waves) spawn.Destroy();
            Round.IsLocked = true;
            Round.Start();
            Server.FriendlyFire = true;
            Server.ExecuteCommand($"/mp load Castle");

            foreach (var _audioClip in System.IO.Directory.GetFiles(Paths.Configs + "/Castle/BGM/"))
                AudioClipStorage.LoadClip(_audioClip, _audioClip.Replace(Paths.Configs + "/Castle/BGM/", "").Replace(".ogg", ""));

            GlobalPlayer = AudioPlayer.CreateOrGet($"Global AudioPlayer", onIntialCreation: (p) =>
            {
                Speaker speaker = p.AddSpeaker("Main", isSpatial: false, maxDistance: 5000);
            });

            InventoryLimits.StandardCategoryLimits[ItemCategory.SpecialWeapon] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.SCPItem] = 8;
            InventoryLimits.Config.RefreshCategoryLimits();

            Timing.RunCoroutine(Timer());
            Timing.RunCoroutine(BGM());
            Timing.RunCoroutine(InputCooldown());
            Timing.RunCoroutine(ItemSpawner());
            Timing.RunCoroutine(MessagePlatform());
        }

        public static void OnRoundStarted()
        {

        }

        public static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Timing.CallDelayed(9, () =>
            {
                Server.ExecuteCommand("/sr");
            });
        }
    }
}
