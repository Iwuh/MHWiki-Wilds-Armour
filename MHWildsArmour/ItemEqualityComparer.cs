using MHWildsArmour.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWildsArmour
{
    public class ItemEqualityComparer : IEqualityComparer<Item>
    {
        public bool Equals(Item? x, Item? y)
        {
            return x?.ItemId == y?.ItemId;
        }

        public int GetHashCode([DisallowNull] Item obj)
        {
            return obj.ItemId.GetHashCode();
        }
    }
}
