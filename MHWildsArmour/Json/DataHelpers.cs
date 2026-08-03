using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MHWildsArmour.Json
{
    public static class DataHelpers
    {
        // N.B.: Rarity strings are off by one, e.g. Rare 1 in-game has [18]RARE0 in the data.
        public static readonly string[] RareStrings =
        [
            "INVALID",
            "[18]RARE0",
            "[17]RARE1",
            "[16]RARE2",
            "[15]RARE3",
            "[14]RARE4",
            "[13]RARE5",
            "[12]RARE6",
            "[11]RARE7",
        ];

        public static readonly string[] SlotLevelStrings =
        [
            "[0]NONE",
            "[1]Lv1",
            "[2]Lv2",
            "[3]Lv3"
        ];

        public static readonly Dictionary<string, string> SkillWikiIconFiles = new()
        {
            { "[0]INVALID",     "INVALID" },
            { "[1]SKILL_0000",  "MHWilds-Attack Skill Icon.png" },
            { "[2]SKILL_0001",  "MHWilds-Affinity Skill Icon.png" },
            { "[3]SKILL_0002",  "MHWilds-Element Skill Icon.png" },
            { "[4]SKILL_0003",  "MHWilds-Sharpness Skill Icon.png" },
            { "[5]SKILL_0004",  "MHWilds-Ammo Skill Icon.png" },
            { "[6]SKILL_0005",  "MHWilds-Defense Skill Icon.png" },
            { "[7]SKILL_0006",  "MHWilds-Health Skill Icon.png" },
            { "[8]SKILL_0007",  "MHWilds-Stamina Skill Icon.png" },
            { "[9]SKILL_0008",  "MHWilds-Empower Skill Icon.png" },
            { "[10]SKILL_0009", "MHWilds-Reinforce Skill Icon.png" },
            { "[11]SKILL_0010", "MHWilds-Item Skill Icon.png" },
            { "[12]SKILL_0011", "MHWilds-Gathering Skill Icon.png" },
            { "[13]SKILL_0012", "MHWilds-Group Skill Icon.png" },
            { "[14]SKILL_0013", "MHWilds-Set Skill Icon.png" }
        };

        public static readonly Dictionary<string, string> SkillIconTypeStrings = new()
        {
            { "[0]INVALID",     "INVALID" },
            { "[1]SKILL_0000",  "Attack" },
            { "[2]SKILL_0001",  "Affinity" },
            { "[3]SKILL_0002",  "Element" },
            { "[4]SKILL_0003",  "Sharpness" },
            { "[5]SKILL_0004",  "Ammo" },
            { "[6]SKILL_0005",  "Defense" },
            { "[7]SKILL_0006",  "Health" },
            { "[8]SKILL_0007",  "Stamina" },
            { "[9]SKILL_0008",  "Empower" },
            { "[10]SKILL_0009", "Reinforce" },
            { "[11]SKILL_0010", "Item" },
            { "[12]SKILL_0011", "Gathering" },
            { "[13]SKILL_0012", "Group" },
            { "[14]SKILL_0013", "Set" }
        };

        public static readonly Dictionary<string, string> ItemWikiIconColors = new()
        {
            { "[0]I_NONE", "INVALID" },
            { "[1]I_WHITE", "White" },
            { "[2]I_GRAY", "Gray" },
            { "[3]I_ROSE", "Rose" },
            { "[4]I_PINK", "Pink" },
            { "[5]I_RED", "Red" },
            { "[6]I_VERMILION", "Vermilion" },
            { "[7]I_ORANGE", "Orange" },
            { "[8]I_BROWN", "Brown" },
            { "[9]I_IVORY", "Tan" },
            { "[10]I_YELLOW", "Yellow" },
            { "[11]I_LEMON", "Lemon" },
            { "[12]I_SGREEN", "Light Green" },
            { "[13]I_MOS", "Moss" },
            { "[14]I_GREEN", "Green" },
            { "[15]I_EMERALD", "Emerald" },
            { "[16]I_SKY", "Light Blue" },
            { "[17]I_BLUE", "Blue" },
            { "[18]I_ULTRAMARINE", "Dark Blue" },
            { "[19]I_BPURPLE", "Violet" },
            { "[20]I_PURPLE", "Purple" },
            { "[21]I_DPURPLE", "Dark Purple" },
        };

        public static readonly Dictionary<string, string> ItemWikiIconTypes = new()
        {
            { "[0]INVALID", "INVALID" },
            { "[1]ITEM_0000", "Pouch" },
            { "[2]ITEM_0001", "Question Mark" },
            { "[6]ITEM_0005", "Mushroom" },
            { "[7]ITEM_0006", "Egg" },
            { "[8]ITEM_0007", "Webbing" },
            { "[9]ITEM_0008", "Herb" },
            { "[10]ITEM_0009", "Medicine" },
            { "[11]ITEM_0010", "Sac" },
            { "[12]ITEM_0011", "Whetstone" },
            { "[13]ITEM_0012", "Pill" },
            { "[14]ITEM_0013", "Fish" },
            { "[15]ITEM_0014", "Meat" },
            { "[16]ITEM_0015", "Barrel" },
            { "[17]ITEM_0016", "Bomb" },
            { "[18]ITEM_0017", "Box" },
            { "[19]ITEM_0018", "Trap" },
            { "[20]ITEM_0019", "Ball" },
            { "[21]ITEM_0020", "Smoke Bomb" },
            { "[22]ITEM_0021", "Bait" },
            { "[25]ITEM_0024", "Binoculars" },
            { "[26]ITEM_0025", "Knife" },
            { "[27]ITEM_0026", "Spit" },
            { "[28]ITEM_0027", "FIXME_STEAKWEAPON" },
            { "[29]ITEM_0028", "Meal Ticket" },
            { "[30]ITEM_0029", "Ticket" },
            { "[31]ITEM_0030", "Coin" },
            { "[32]ITEM_0031", "Husk" },
            { "[33]ITEM_0032", "Ammo" },
            { "[35]ITEM_0034", "Coating" },
            { "[36]ITEM_0035", "Spiderweb" },
            { "[37]ITEM_0036", "Seed" },
            { "[38]ITEM_0037", "Ore" },
            { "[39]ITEM_0038", "Bug" },
            { "[40]ITEM_0039", "Dung" },
            { "[41]ITEM_0040", "Monster Part" },
            { "[42]ITEM_0041", "Bone" },
            { "[43]ITEM_0042", "Scale" },
            { "[44]ITEM_0043", "Hide" },
            { "[45]ITEM_0044", "Claw" },
            { "[46]ITEM_0045", "Shell" },
            { "[47]ITEM_0046", "Tail" },
            { "[48]ITEM_0047", "Wing" },
            { "[49]ITEM_0048", "Head" },
            { "[50]ITEM_0049", "Plate" },
            { "[52]ITEM_0051", "Crystal" },
            { "[54]ITEM_0053", "FIXME_GLOWINGSTONE" },
            { "[55]ITEM_0054", "Armor Sphere" },
            { "[56]ITEM_0055", "Decoration" },
            { "[61]ITEM_0060", "Tent" },
            { "[62]ITEM_0061", "Slinger" },
            { "[63]ITEM_0062", "Net" },
            { "[66]ITEM_0065", "UNKNOWN" },
            { "[67]ITEM_0066", "Ammo Spread" },
            { "[68]ITEM_0067", "Ammo Pierce" },
            { "[69]ITEM_0068", "Ammo Sticky" },
            { "[70]ITEM_0069", "Ammo Cluster" },
            { "[71]ITEM_0070", "Vial" },
            { "[72]ITEM_0071", "Drug" },
            { "[73]ITEM_0072", "Chemical" },
            { "[74]ITEM_0073", "Cape" },
            { "[77]ITEM_0076", "Ingredient Cheese" },
            { "[78]ITEM_0077", "Ingredient Mushroom" },
            { "[79]ITEM_0078", "Ingredient Shrimp" },
            { "[80]ITEM_0079", "Ingredient Garlic" },
            { "[81]ITEM_0080", "Ingredient Egg" },
            { "[87]ITEM_0086", "Corpse" },
            { "[88]ITEM_0087", "FIXME_AKUMA1" },
            { "[89]ITEM_0088", "FIXME_AKUMA2" },
            { "[90]ITEM_0089", "FIXME_AKUMA3" },
            { "[91]ITEM_0090", "FIXME_WORNPICKAXE" },
            { "[92]ITEM_0091", "FIXME_PCT" },
            { "[93]ITEM_0092", "FIXME_PCT" },
            { "[94]ITEM_0093", "FIXME_PCT" },
            { "[95]ITEM_0094", "FIXME_TBN" },
            { "[96]ITEM_0095", "FIXME_SHIELDGENERATOR" },
            { "[97]ITEM_0096", "FIXME_PCT" },
        };

        // TODO other items
        public static readonly Dictionary<string, string> ItemWikiAdditionalIconTypes = new()
        {
            { "[0]INVALID", string.Empty },
            { "[1]GREAT", "Great" },
            { "[31]INGREDIENTS", "Cooking" },
            { "[38]FOR_ATTACK", "" },
            { "[40]FOR_ARMOR", "" },
            { "[8]CLEAR_ITEM", "" },

        };

        public static IEnumerable<ArmorDatum> GetAllArmorWithSeries(string dataPath)
        {
            var armorData = ArmorDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "ArmorData.user.3.flat.json")));
            var armorMsgs = WildsMsg.FromJson(File.ReadAllText(Path.Join(dataPath, "Armor.msg.23.json")));
            //var armorSkillsOld = ArmorSkill.FromJson(File.ReadAllText(@".\skillDict.json"));
            var armorRecipeData = ArmorRecipeDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "ArmorRecipeData.user.3.flat.json")));
            //var items = Item.FromJson(File.ReadAllText(@".\items.json"));
            var itemData = GetAllItem(dataPath);
            var armorSeriesData = GetAllArmorSeries(dataPath);
            var skillCommonData = GetAllSkillCommon(dataPath);

            return armorData.Join(armorMsgs.Entries, d => d.NameGuid, m => m.Guid, (d, m) =>
            {
                d.Name = m.Content[1];
                return d;
            })
            .Join(armorMsgs.Entries, d => d.ExplainGuid, m => m.Guid, (d, m) =>
            {
                d.Explain = m.Content[1];
                return d;
            })
            .Join(armorSeriesData, d1 => d1.SeriesId, d2 => d2.Series, (d1, d2) =>
            {
                d1.Series = d2;
                return d1;
            })
            .Join(armorRecipeData, d1 => new { Series = d1.SeriesId, PartsType = d1.PartsType }, d2 => new { Series = d2.SeriesId, PartsType = d2.PartsType }, (d1, d2) =>
            {
                d1.Recipe = d2;
                return d1;
            })
            .Select(d =>
            {
                //d.Skill = d.SkillId.Select(i => armorSkillsOld.FirstOrDefault(s => i == s.SkillId)).ToArray();
                d.SkillCommon = d.SkillId.Select(i => skillCommonData.FirstOrDefault(s => i == s.SkillId)).ToArray();
                d.Recipe.Items = d.Recipe.ItemId.Select(i1 => itemData.FirstOrDefault(i2 => i1 == i2.ItemId)).ToArray();
                d.SlotLevel = d.SlotLevelString.Select(s => Array.IndexOf(SlotLevelStrings, s)).ToArray();
                return d;
            });
        }

        public static IEnumerable<ArmorSeriesDatum> GetAllArmorSeries(string dataPath)
        {
            var armorSeriesData = ArmorSeriesDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "ArmorSeriesData.user.3.flat.json")));
            var armorUpgradeData = ArmorUpgradeDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "ArmorUpgradeData.user.3.flat.json")));
            var armorTranscendCostData = ArmorTranscendCostDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "ArmorSpUpgradeCostData.user.3.flat.json")));
            var armorTranscendRecipeData = ArmorTranscendRecipeDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "ArmorUpgradeRecipeData.user.3.flat.json")));
            var armorSeriesMsgs = WildsMsg.FromJson(File.ReadAllText(Path.Join(dataPath, "ArmorSeries.msg.23.json")));
            var itemData = GetAllItem(dataPath);

            //var armorMaxLevels = armorUpgradeData.GroupBy(d => d.Rare).ToDictionary(g => g.Key, g => g.Max(d => d.MaxLevel));

            return armorSeriesData.Join(armorSeriesMsgs.Entries, d => d.NameGuid, m => m.Guid, (d, m) =>
            {
                d.Name = m.Content[1];
                return d;
            })
            .LeftJoin(armorTranscendRecipeData, d1 => d1.Series, d2 => d2.SeriesId, (d1, d2) =>
            {
                d1.TranscendRecipe = d2;
                return d1;
            })
            .Select(s => 
            {
                s.Rare = Array.IndexOf(RareStrings, s.RareString);

                var orderedUpgradeData = armorUpgradeData.Where(d => d.Rare == s.RareString).OrderByDescending(d => d.MaxLevel);
                var baseUpgradeData = orderedUpgradeData.First(d => d.IsSpecialUpgrade == false);
                var transcendUpgradeData = orderedUpgradeData.FirstOrDefault(d => d.IsSpecialUpgrade == true);
                //s.MaxLevel = (int)(armorMaxLevels[s.RareString] ?? 0);
                s.MaxLevel = (int)(baseUpgradeData.MaxLevel ?? 0);
                s.DefPerLevel = (int)(baseUpgradeData.DefUpValue ?? 0);
                s.MaxLevelTranscend = (int?)transcendUpgradeData?.MaxLevel;

                var transcendCostData = armorTranscendCostData.FirstOrDefault(d => d.Rare == s.RareString);
                s.TranscendCost = (int?)transcendCostData?.Cost;

                if (s.TranscendRecipe != null)
                {
                    s.TranscendRecipe.Items = s.TranscendRecipe.Item.Select(i1 => itemData.FirstOrDefault(i2 => i1 == i2.ItemId)).ToArray();
                }

                return s;
            });
        }

        public static IEnumerable<SkillCommonDatum> GetAllSkillCommon(string dataPath)
        {
            var skillCommonData = SkillCommonDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "SkillCommonData.user.3.flat.json")));
            var skillCommonMsgs = WildsMsg.FromJson(File.ReadAllText(Path.Join(dataPath, "SkillCommon.msg.23.json")));

            return skillCommonData.Join(skillCommonMsgs.Entries, d => d.SkillNameGuid, m => m.Guid, (d, m) =>
            {
                d.SkillName = m.Content[1];
                if (d.SkillName == "Aquatic/Oilsilt Mobility")
                {
                    d.SkillName = "Aquatic-Oilsilt Mobility";
                }
                return d;
            })
            .Join(skillCommonMsgs.Entries, d => d.SkillExplainGuid, m => m.Guid, (d, m) => 
            {
                d.SkillExplain = m.Content[1];
                return d;
            })
            .Select(s => 
            {
                s.SkillWikiIconFile = SkillWikiIconFiles[s.SkillIconType];
                s.SkillIconTypeString = SkillIconTypeStrings[s.SkillIconType];
                return s;
            });
        }

        public static IEnumerable<ItemDatum> GetAllItem(string dataPath)
        {
            var itemData = ItemDatum.FromJson(File.ReadAllText(Path.Join(dataPath, "itemData.user.3.flat.json")));
            var itemMsgs = WildsMsg.FromJson(File.ReadAllText(Path.Join(dataPath, "Item.msg.23.json")));

            return itemData.Join(itemMsgs.Entries, d => d.ItemNameGuid, m => m.Guid, (d, m) =>
            {
                d.ItemName = m.Content[1];
                return d;
            })
            .Join(itemMsgs.Entries, d => d.ItemExplainGuid, m => m.Guid, (d, m) =>
            {
                d.ItemExplain = m.Content[1];
                return d;
            })
            .Select(d => 
            {
                d.WikiIconColor = ItemWikiIconColors[d.IconColor];
                d.WikiIconType = ItemWikiIconTypes[d.IconType];
                d.WikiAdditionalIconType = ItemWikiAdditionalIconTypes[d.AddIconType];
                return d;
            });
        }
    }
}
