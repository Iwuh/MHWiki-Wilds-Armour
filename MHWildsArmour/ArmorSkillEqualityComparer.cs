using MHWildsArmour.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWildsArmour
{
    public class ArmorSkillEqualityComparer : IEqualityComparer<ArmorSkill>
    {
        public bool Equals(ArmorSkill? x, ArmorSkill? y)
        {
            return x?.SkillId == y?.SkillId;
        }

        public int GetHashCode([DisallowNull] ArmorSkill obj)
        {
            return obj.SkillId.GetHashCode();
        }
    }
}
