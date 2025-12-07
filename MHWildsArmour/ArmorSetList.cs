using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWildsArmour
{
    class ArmorSetList
    {
        private const string HEADER = @"{{Meta
|MetaTitle     = MHWilds Armor Sets
|MetaDesc      = A list of all Armor Sets from Monster Hunter Wilds
|MetaKeywords  = MHWilds, Monster Hunter Wilds, Armor, Armor Sets
|MetaImage     = MHWilds-Logo.png
}}
{{GenericNav|MHWilds}}
The following is a list of all armor sets that appear in [[Monster Hunter Wilds]] and their corresponding armor pieces.
__TOC__
";

        private readonly List<ArmorSet> sets = [];

        public void AddSet(ArmorSet set)
        {
            sets.Add(set);
        }

        public string GeneratePage()
        {
            var sb = new StringBuilder(HEADER);

            sb.AppendLine("=Low Rank=");
            for (int rarity = 1; rarity <= 4; rarity++)
            {
                sb.AppendLine($"==Rarity {rarity}==");
                sb.AppendLine(@"<div style=""display:flex; flex-wrap:wrap; height: 100%; align-items:stretch; justify-content:space-around; margin-bottom:20px;"">");
                var sorted = sets.Where(x => x.Series.Rare == rarity).OrderBy(x => x.Series.Name);
                foreach (var set in sorted)
                {
                    sb.Append(set.GenerateSetListTemplate());
                }
                sb.AppendLine("</div>");
            }

            sb.AppendLine("=High Rank=");
            for (int rarity = 5; rarity <= 8; rarity++)
            {
                sb.AppendLine($"==Rarity {rarity}==");
                sb.AppendLine(@"<div style=""display:flex; flex-wrap:wrap; height: 100%; align-items:stretch; justify-content:space-around; margin-bottom:20px;"">");
                var sorted = sets.Where(x => x.Series.Rare == rarity).OrderBy(x => x.Series.Name);
                foreach (var set in sorted)
                {
                    sb.Append(set.GenerateSetListTemplate());
                }
                sb.AppendLine("</div>");
            }

            sb.AppendLine("[[Category:Armor Sets by Game Appearance]]");

            return sb.ToString();
        }
    }
}
