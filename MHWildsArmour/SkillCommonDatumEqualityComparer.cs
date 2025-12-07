using MHWildsArmour.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWildsArmour
{
    class SkillCommonDatumEqualityComparer : IEqualityComparer<SkillCommonDatum>
    {
        public bool Equals(SkillCommonDatum? x, SkillCommonDatum? y)
        {
            return x?.SkillId == y?.SkillId;
        }

        public int GetHashCode([DisallowNull] SkillCommonDatum obj)
        {
            return obj.SkillId.GetHashCode();
        }
    }
}
