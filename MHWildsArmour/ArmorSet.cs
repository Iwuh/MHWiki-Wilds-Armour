using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediawikiTranslator.Models.ArmorSets;
using MediawikiTranslator.Models.Data.MHWilds;
using MediawikiTranslator.Models.MaterialsAndDropTables;
using MHWildsArmour.Json;

namespace MHWildsArmour
{
    class ArmorSet
    {
        private static readonly Dictionary<string, string> pieceTemplateIconTypeMap = new()
        {
            { "[0]HELM", "Helmet" },
            { "[1]BODY", "Chestplate" },
            { "[2]ARM", "Armguards" },
            { "[3]WAIST", "Waist" },
            { "[4]LEG", "Leggings" }
        };

        public required string Game { get; set; }

        public required ArmorSeriesDatum Series { get; set; }

        public required ArmorDatum? HeadPiece { get; set; }

        public required ArmorDatum? ChestPiece { get; set; }

        public required ArmorDatum? ArmPiece { get; set; }

        public required ArmorDatum? WaistPiece { get; set; }

        public required ArmorDatum? LegPiece { get; set; }

        public string GenerateSetListTemplate()
        {
            var sb = new StringBuilder("{{ArmorSetListItem\n");
            sb.AppendLine($"|Game               = {Game}");
            sb.AppendLine($"|Set Rarity         = {Series.Rare}");
            sb.AppendLine($"|Set Name           = {Series.Name}");

            (string maleImage, string femaleImage) = GetArmorSetRenders();
            sb.AppendLine($"|Male Image         = {maleImage}");
            sb.AppendLine($"|Female Image       = {femaleImage}");

            sb.AppendLine($"|Head Piece Name    = {HeadPiece?.Name ?? "None"}");
            sb.AppendLine($"|Chest Piece Name   = {ChestPiece?.Name ?? "None"}");
            sb.AppendLine($"|Arm Piece Name     = {ArmPiece?.Name ?? "None"}");
            sb.AppendLine($"|Waist Piece Name   = {WaistPiece?.Name ?? "None"}");
            sb.AppendLine($"|Leg Piece Name     = {LegPiece?.Name ?? "None"}");
            sb.AppendLine("}}");
            return sb.ToString();
        }

        public string GenerateFullSetTemplate()
        {
            var validPieces = (new[] { HeadPiece, ChestPiece, ArmPiece, WaistPiece, LegPiece }).Where(p => p != null).ToList();

            var sb = new StringBuilder("{{GenericArmorSet\n");
            sb.AppendLine($"|Game                               = {Game}");
            sb.AppendLine($"|Set Name                           = {Series.Name}");

            (string maleImage, string femaleImage) = GetArmorSetRenders();
            sb.AppendLine($"|Male Image                         = {maleImage}");
            sb.AppendLine($"|Female Image                       = {femaleImage}");

            sb.AppendLine($"|Max Level                          = {Series.MaxLevel}");
            sb.AppendLine($"|Set Rarity                         = {Series.Rare}");

            // Zip each pieces' armor skills with their levels & flatten list of lists
            var allSkills = validPieces.SelectMany(p => p!.SkillCommon.Zip(p!.SkillLevel))
                .Where(t => t.First.SkillId != "[0]NONE");

            var setSkills = allSkills.Distinct()
                .Where(s => s.First.SkillCategory == "[1]SERIES")
                .ToList();
            var groupSkills = allSkills.Distinct()
                .Where(s => s.First.SkillCategory == "[2]GROUP")
                .ToList();

            if (setSkills.Count == 1)
            {
                sb.AppendLine($"|Set Skill 1                        = {{{{MHWildsSkillLink|{setSkills[0].First.SkillName}|Set}}}}");
            }
            else if (setSkills.Count == 2)
            {
                sb.AppendLine($"|Set Skill 1                        = {{{{MHWildsSkillLink|{setSkills[0].First.SkillName}|Set}}}}");
                sb.AppendLine($"|Set Skill 2                        = {{{{MHWildsSkillLink|{setSkills[1].First.SkillName}|Set}}}}");
            }

            if (groupSkills.Count == 1)
            {
                sb.AppendLine($"|Group Skill 1                      = {{{{MHWildsSkillLink|{groupSkills[0].First.SkillName}|Group}}}}");
            }
            else if (groupSkills.Count == 2)
            {
                sb.AppendLine($"|Group Skill 1                      = {{{{MHWildsSkillLink|{groupSkills[0].First.SkillName}|Group}}}}");
                sb.AppendLine($"|Group Skill 2                      = {{{{MHWildsSkillLink|{groupSkills[1].First.SkillName}|Group}}}}");
            }

            sb.AppendLine($"|Total Forging Cost                 = {Series.Price * validPieces.Count}"); 

            sb.AppendLine("|Total Skills                       = ");
            var groupedEquipSkills = allSkills
                .Where(s => s.First.SkillCategory == "[0]EQUIP")
                .GroupBy(s => s.First, g => g.Second, (k, g) => (Skill: k, TotalLevel: g.Sum()), new SkillCommonDatumEqualityComparer())
                .OrderByDescending(t => t.TotalLevel);
            foreach (var tuple in groupedEquipSkills)
            {
                sb.AppendLine($"<div>{{{{MHWildsSkillLink|{tuple.Skill.SkillName}|{tuple.Skill.SkillIconTypeString}}}}} x{tuple.TotalLevel}</div>");
            }

            sb.AppendLine("|Total Forging Materials            = ");
            var totalItems = validPieces.SelectMany(p => p!.Recipe.Items.Zip(p!.Recipe.ItemNum))
                .Where(t => t.First != null)
                .GroupBy(t => t.First, t => t.Second, (k, g) => (Item: k, TotalQuantity: g.Sum()), new ItemDatumEqualityComparer())
                .OrderByDescending(t => t.TotalQuantity);
            foreach (var (item, totalQuantity) in totalItems)
            {
                string fullIconType = string.IsNullOrEmpty(item.WikiAdditionalIconType) ?
                    item.WikiIconType :
                    $"{item.WikiIconType}-{item.WikiAdditionalIconType}";
                sb.AppendLine($"<div>{{{{GenericItemLink|{Game}|{item.ItemName}|{fullIconType}|{item.WikiIconColor}}}}} x{totalQuantity}</div>");
            }

            var lvl1SlotCount = validPieces.SelectMany(p => p!.SlotLevel).Count(s => s == "[1]Lv1");
            var lvl2SlotCount = validPieces.SelectMany(p => p!.SlotLevel).Count(s => s == "[2]Lv2");
            var lvl3SlotCount = validPieces.SelectMany(p => p!.SlotLevel).Count(s => s == "[3]Lv3");
            sb.AppendLine($"|Total Decos 1                      = {lvl1SlotCount}");
            sb.AppendLine($"|Total Decos 2                      = {lvl2SlotCount}");
            sb.AppendLine($"|Total Decos 3                      = {lvl3SlotCount}");

            sb.AppendLine($"|Total Defense                      = {validPieces.Sum(p => p!.Defense)}");
            sb.AppendLine($"|Total Fire Res                     = {validPieces.Sum(p => p!.Resistance[0])}");
            sb.AppendLine($"|Total Water Res                    = {validPieces.Sum(p => p!.Resistance[1])}");
            sb.AppendLine($"|Total Thunder Res                  = {validPieces.Sum(p => p!.Resistance[2])}");
            sb.AppendLine($"|Total Ice Res                      = {validPieces.Sum(p => p!.Resistance[3])}");
            sb.AppendLine($"|Total Dragon Res                   = {validPieces.Sum(p => p!.Resistance[4])}");

            sb.AppendLine("}}");
            return sb.ToString();
        }

        public string GenerateArmorPieceTemplate(ArmorDatum piece)
        {
            var lvl1SlotCount = piece.SlotLevel.Count(s => s == "[1]Lv1");
            var lvl2SlotCount = piece.SlotLevel.Count(s => s == "[2]Lv2");
            var lvl3SlotCount = piece.SlotLevel.Count(s => s == "[3]Lv3");


            // For now, the only use of colours in an armour piece description is the (Full Armor Set) tag used for the Akuma gear.
            // Revisit this later if that changes.
            //var cleanDescription = Regex.Replace(piece.Explain.Replace("\r\n", " "), @"<COLOR preset=\""[A-Z_]+\"">(.+)<\/COLOR>", m => m.Groups[1].Value);
            var cleanDescription = piece.Explain.Replace("\r\n", " ").Replace(@"<COLOR preset=""I_YELLOW"">(Full Armor Set)</COLOR>", @"<span style=""color:gold"">(Full Armor Set)</span>");

            var sb = new StringBuilder("{{GenericArmorSetPiece\n");
            sb.AppendLine($"|Game                  = {Game}");
            sb.AppendLine($"|Piece Name            = {piece.Name}");
            sb.AppendLine($"|Max Level             = {Series.MaxLevel}"); 
            sb.AppendLine($"|Rarity                = {Series.Rare}");
            sb.AppendLine($"|Item Icon Type        = {pieceTemplateIconTypeMap[piece.PartsType]}");
            //sb.AppendLine($"|Male Image            = "); TODO need renders
            //sb.AppendLine($"|Female Image          = "); TODO need renders
            sb.AppendLine($"|Description           = {cleanDescription}");
            sb.AppendLine($"|Level 1 Decos         = {lvl1SlotCount}");
            sb.AppendLine($"|Level 2 Decos         = {lvl2SlotCount}");
            sb.AppendLine($"|Level 3 Decos         = {lvl3SlotCount}");
            sb.AppendLine($"|Forging Cost          = {Series.Price}");
            sb.AppendLine($"|Defense               = {piece.Defense}");
            sb.AppendLine($"|Fire Res              = {piece.Resistance[0]}");
            sb.AppendLine($"|Water Res             = {piece.Resistance[1]}");
            sb.AppendLine($"|Thunder Res           = {piece.Resistance[2]}");
            sb.AppendLine($"|Ice Res               = {piece.Resistance[3]}");
            sb.AppendLine($"|Dragon Res            = {piece.Resistance[4]}");

            sb.AppendLine("|Skills                = ");
            //var validSkillData = piece.Skill
            //    .Zip(piece.SkillLevel)
            //    .Where(t => t.First.SkillId != "[0]NONE" && t.First.SkillCategory == "[0]EQUIP");
            //foreach (var (skill, level) in validSkillData)
            //{
            //    var m = Regex.Match(skill.SkillIconType, @"MHWilds-(\w+) Skill Icon\.png");
            //    sb.AppendLine($"<div>{{{{MHWildsSkillLink|{skill.Name}|{m.Groups[1]}}}}} x{level}</div>");
            //}
            var validSkillData = piece.SkillCommon
                .Zip(piece.SkillLevel)
                .Where(t => t.First.SkillId != "[0]NONE" && t.First.SkillCategory == "[0]EQUIP");
            foreach (var (skill, level) in validSkillData)
            {
                sb.AppendLine($"<div>{{{{MHWildsSkillLink|{skill.SkillName}|{skill.SkillIconTypeString}}}}} x{level}</div>");
            }

            sb.AppendLine("|Materials             = ");
            //var craftingMaterials = piece.Recipe.Item
            //    .Zip(piece.Recipe.ItemNum)
            //    .Where(t => t.First != null)
            //    .OrderByDescending(t => t.Second);
            //foreach (var (item, quantity) in craftingMaterials)
            //{
            //    sb.AppendLine($"<div>{{{{GenericItemLink|{Game}|{item.Name}|{item.Icon}|{item.IconColor}}}}} x{quantity}</div>");
            //}
            var craftingMaterials = piece.Recipe.Items
                .Zip(piece.Recipe.ItemNum)
                .Where(t => t.First != null)
                .OrderByDescending(t => t.Second);
            foreach (var (item, quantity) in craftingMaterials)
            {
                string fullIconType = string.IsNullOrEmpty(item.WikiAdditionalIconType) ?
                    item.WikiIconType :
                    $"{item.WikiIconType}-{item.WikiAdditionalIconType}";
                sb.AppendLine($"<div>{{{{GenericItemLink|{Game}|{item.ItemName}|{fullIconType}|{item.WikiIconColor}}}}} x{quantity}</div>");
            }

            sb.AppendLine("}}");

            return sb.ToString();
        }

        public string GenerateArmorSetPage()
        {
            var pageLink = (Series.Rare < 5) ? "[[MHWilds/Armor#Low Rank|Low Rank (LR)]]" : "[[MHWilds/Armor#High Rank|High Rank (HR)]]";

            var sb = new StringBuilder("{{GenericNav|MHWilds}}\n<br>\n<br>\n");
            sb.AppendLine($"The {Series.Name} Set is a {pageLink} armor set in [[Monster Hunter Wilds]].<br>");

            sb.Append(GenerateFullSetTemplate());
            foreach (var piece in new[] { HeadPiece, ChestPiece, ArmPiece, WaistPiece, LegPiece })
            {
                if (piece != null)
                {
                    sb.Append(GenerateArmorPieceTemplate(piece));
                }
            }

            sb.Append("[[Category:MHWilds Armor Sets]]");

            return sb.ToString();
        }

        public ArmorSetWikiDb GenerateArmorSetWikiDb(int order)
        {
            var validPieces = (new[] { HeadPiece, ChestPiece, ArmPiece, WaistPiece, LegPiece }).Where(p => p != null).ToList();

            var wikiSet = new ArmorSetWikiDb
            {
                Order = order,
                SetName = Series.Name,
                Game = Game,
                Rarity = Series.Rare
            };

            (string maleImage, string femaleImage) = GetArmorSetRenders();
            wikiSet.MaleFrontImg = maleImage;
            wikiSet.MaleBackImg = string.Empty;
            wikiSet.FemaleFrontImg = femaleImage;
            wikiSet.FemaleBackImg = string.Empty;

            // Zip each pieces' armor skills with their levels & flatten list of lists
            var allSkills = validPieces.SelectMany(p => p!.SkillCommon.Zip(p!.SkillLevel))
                .Where(t => t.First.SkillId != "[0]NONE");

            var setSkills = allSkills.Distinct()
                .Where(s => s.First.SkillCategory == "[1]SERIES")
                .ToList();
            var groupSkills = allSkills.Distinct()
                .Where(s => s.First.SkillCategory == "[2]GROUP")
                .ToList();

            if (setSkills.Count == 0)
            {
                wikiSet.SetSkill1Name = null;
                wikiSet.SetSkill2Name = null;
            }
            else if (setSkills.Count == 1)
            {
                wikiSet.SetSkill1Name = setSkills[0].First.SkillName;
                wikiSet.SetSkill2Name = null;
            }
            else if (setSkills.Count == 2)
            {
                wikiSet.SetSkill1Name = setSkills[0].First.SkillName;
                wikiSet.SetSkill2Name = setSkills[1].First.SkillName;
            }

            if (groupSkills.Count == 0)
            {
                wikiSet.GroupSkill1Name = null;
                wikiSet.GroupSkill2Name = null;
            }
            else if (groupSkills.Count == 1)
            {
                wikiSet.GroupSkill1Name = groupSkills[0].First.SkillName;
                wikiSet.GroupSkill2Name = null;
            }
            else if (setSkills.Count == 2)
            {
                wikiSet.GroupSkill1Name = groupSkills[0].First.SkillName;
                wikiSet.GroupSkill2Name = groupSkills[1].First.SkillName;
            }

            wikiSet.Pieces = new List<ArmorPieceWikiDb>();
            foreach (var piece in validPieces)
            {
                wikiSet.Pieces.Add(GenerateArmorPieceWikiDb(piece!));
            }

            return wikiSet;
        }

        public ArmorPieceWikiDb GenerateArmorPieceWikiDb(ArmorDatum piece)
        {
            // For now, the only use of colours in an armour piece description is the (Full Armor Set) tag used for the Akuma gear.
            // Revisit this later if that changes.
            var cleanDescription = piece.Explain.Replace("\r\n", " ").Replace(@"<COLOR preset=""I_YELLOW"">(Full Armor Set)</COLOR>", @"<span style=""color:gold"">(Full Armor Set)</span>");

            var wikiPiece = new ArmorPieceWikiDb
            {
                Name = piece.Name,
                Rarity = Series.Rare,
                ForgingCost = Series.Price ?? -1,
                IconType = pieceTemplateIconTypeMap[piece.PartsType],
                MaleImage = string.Empty, // TODO need renders
                FemaleImage = string.Empty,
                Description = cleanDescription,
                Defense = piece.Defense ?? -1,
                MaxDefense = piece.Defense + (Series.MaxLevel - 1) * 2 ?? -1, // TODO check that this is correct
                FireRes = piece.Resistance[0],
                WaterRes = piece.Resistance[1],
                ThunderRes = piece.Resistance[2],
                IceRes = piece.Resistance[3],
                DragonRes = piece.Resistance[4],
                Decos1 = piece.SlotLevel.Count(s => s == "[1]Lv1"),
                Decos2 = piece.SlotLevel.Count(s => s == "[2]Lv2"),
                Decos3 = piece.SlotLevel.Count(s => s == "[3]Lv3"),
                Decos4 = 0
            };

            var validSkillData = piece.SkillCommon
                .Zip(piece.SkillLevel)
                .Where(t => t.First.SkillId != "[0]NONE" && t.First.SkillCategory == "[0]EQUIP");
            wikiPiece.Skills = validSkillData.Select(s => new ArmorSkillWikiDb { Name = s.First.SkillName, Level = s.Second }).ToList();

            var craftingMaterials = piece.Recipe.Items
                .Zip(piece.Recipe.ItemNum)
                .Where(t => t.First != null)
                .OrderByDescending(t => t.Second);
            wikiPiece.Materials = craftingMaterials.Select(m => new ArmorMaterialWikiDb { Name = m.First.ItemName, Quantity = m.Second }).ToList();

            return wikiPiece;
        }

        private (string maleImage, string femaleImage) GetArmorSetRenders()
        {
            string maleImage, femaleImage;
            if (Series.Name == "Akuma α")
            {
                maleImage = $"{Game}-{Series.Name} Armor Render.webp";
                femaleImage = $"{Game}-{Series.Name} Armor Render.webp";
            }
            else if (Series.Name == "Hawkheart Jacket α")
            {
                maleImage = $"{Game}-{Series.Name} Armor Male Render 001.webp";
                femaleImage = $"{Game}-{Series.Name} Armor Female Render.webp";
            }
            else
            {
                maleImage = $"{Game}-{Series.Name} Armor Male Render.webp";
                femaleImage = $"{Game}-{Series.Name} Armor Female Render.webp";
            }
            return (maleImage, femaleImage);
        }
    }
}
