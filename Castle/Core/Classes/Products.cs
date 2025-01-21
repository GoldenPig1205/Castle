using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castle.Core.Classes
{
    public class Products
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public Action<Player> Script { get; set; }
    }
}
