using MHWildsArmour.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWildsArmour
{
    public class ItemDatumEqualityComparer : IEqualityComparer<ItemDatum>
    {
        public bool Equals(ItemDatum? x, ItemDatum? y)
        {
            return x?.ItemId == y?.ItemId;
        }

        public int GetHashCode([DisallowNull] ItemDatum obj)
        {
            return obj.ItemId.GetHashCode();
        }
    }
}
