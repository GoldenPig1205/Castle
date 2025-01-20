using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castle.Core.Variables
{
    public static class Base
    {
        public static int Hour = 0;
        public static AudioPlayer GlobalPlayer;

        public static List<Player> ChatCooldown = new List<Player>();
        public static List<Player> EmotionCooldown = new List<Player>();
    }
}
