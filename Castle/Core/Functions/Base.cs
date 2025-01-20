using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castle.Core.Functions
{
    public static class Base
    {
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
    }
}
