using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Castle.Core.Functions
{
    public static class Base
    {
        public static void Spawn(Player player, RoleTypeId roleTypeId = RoleTypeId.None)
        {
            List<RoleTypeId> roles = new List<RoleTypeId>
            {
                RoleTypeId.ClassD,
                RoleTypeId.Scientist,
                RoleTypeId.FacilityGuard,
                RoleTypeId.NtfPrivate,
                RoleTypeId.ChaosRifleman,
                RoleTypeId.Tutorial
            };

            if (roleTypeId == RoleTypeId.None)
                player.Role.Set(EnumToList<RoleTypeId>().Where(roles.Contains).GetRandomValue());

            else
                player.Role.Set(roleTypeId);

            player.ClearInventory();
        }

        public static int CalculateIntensity(int hour)
        {
            if (hour <= 12)
            {
                return 72 - (hour * 6);
            }
            else
            {
                return (hour - 12) * 6;
            }
        }

        public static List<T> EnumToList<T>()
        {
            Array items = Enum.GetValues(typeof(T));
            List<T> itemList = new List<T>();

            foreach (T item in items)
            {
                if (!item.ToString().Contains("None"))
                    itemList.Add(item);
            }

            return itemList;
        }

        public static bool TryGetLookPlayer(Player player, float Distance, out Player target, out RaycastHit? raycastHit)
        {
            target = null;
            raycastHit = null;

            if (Physics.Raycast(player.ReferenceHub.PlayerCameraReference.position + player.ReferenceHub.PlayerCameraReference.forward * 0.2f, player.ReferenceHub.PlayerCameraReference.forward, out RaycastHit hit, Distance) &&
                    hit.collider.TryGetComponent<IDestructible>(out IDestructible destructible))
            {
                if (Player.TryGet(hit.collider.GetComponentInParent<ReferenceHub>(), out Player t) && player != t)
                {
                    target = t;
                    raycastHit = hit;

                    return true;
                }
            }

            return false;
        }

        public static string ColorFormat(string cn)
        {
            if (ColorUtility.TryParseHtmlString(cn, out Color color))
                return color.ToHex();

            else
            {
                Dictionary<string, string> Colors = new Dictionary<string, string>
                {
                    // {"gold", "#EFC01A"},
                    // {"teal", "#008080"},
                    // {"blue", "#005EBC"},
                    // {"purple", "#8137CE"},
                    // {"light_red", "#FD8272"},
                    {"pink", "#FF96DE"},
                    {"red", "#C50000"},
                    {"default", "#FFFFFF"},
                    {"brown", "#944710"},
                    {"silver", "#A0A0A0"},
                    {"light_green", "#32CD32"},
                    {"crimson", "#DC143C"},
                    {"cyan", "#00B7EB"},
                    {"aqua", "#00FFFF"},
                    {"deep_pink", "#FF1493"},
                    {"tomato", "#FF6448"},
                    {"yellow", "#FAFF86"},
                    {"magenta", "#FF0090"},
                    {"blue_green", "#4DFFB8"},
                    // {"silver_blue", "#666699"},
                    {"orange", "#FF9966"},
                    // {"police_blue", "#002DB3"},
                    {"lime", "#BFFF00"},
                    {"green", "#228B22"},
                    {"emerald", "#50C878"},
                    {"carmine", "#960018"},
                    {"nickel", "#727472"},
                    {"mint", "#98FB98"},
                    {"army_green", "#4B5320"},
                    {"pumpkin", "#EE7600"}
                };

                if (Colors.ContainsKey(cn))
                    return Colors[cn];

                else
                    return "#FFFFFF";
            }
        }

        public static string BadgeFormat(Player player)
        {
            if (player.RankName != null && !player.BadgeHidden)
                return $"[<color={ColorFormat(player.RankColor)}>{player.RankName}</color>] ";

            else
                return "";
        }
    }
}
