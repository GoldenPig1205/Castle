using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castle.Core.HarmonyPatches
{
    public class HitboxPatchPostfix
    {
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }
}
