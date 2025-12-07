using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace MHWildsArmour
{
    class ArmorPiece
    {
        [Name("Piece Name")]
        public required string PieceName { get; set; }

        [Name("Set Name")]
        public required string SetName { get; set; }

        [Name("Piece Type")]
        public required string PieceType { get; set; }

        [Name("Rarity")]
        public int Rarity { get; set; }

        [Name("Base Def.")]
        public required string BaseDefense { get; set; }

        [Name("Fire Res.")]
        public required string FireRes { get; set; }

        [Name("Water Res.")]
        public required string WaterRes { get; set; }

        [Name("Thunder Res.")]
        public required string ThunderRes { get; set; }

        [Name("Ice Res.")]
        public required string IceRes { get; set; }

        [Name("Dragon Res.")]
        public required string DragonRes { get; set; }

        [Name("Lvl 1 Deco Slots")]
        [NullValues("-")]
        public int? Lvl1Slots { get; set; }

        [Name("Lvl 2 Deco Slots")]
        [NullValues("-")]
        public int? Lvl2Slots { get; set; }

        [Name("Lvl 3 Deco Slots")]
        [NullValues("-")]
        public int? Lvl3Slots { get; set; }

        [Name("Text Description")]
        public required string Description { get; set; }

        [Name("Skill 1")]
        [NullValues("-")]
        public string? Skill1Name { get; set; }

        [Name("Skill 1 Lvl")]
        [NullValues("-")]
        public int? Skill1Lvl { get; set; }

        [Name("Skill 2")]
        [NullValues("-")]
        public string? Skill2Name { get; set; }

        [Name("Skill 2 Lvl")]
        [NullValues("-")]
        public int? Skill2Lvl { get; set; }

        [Name("Skill 3")]
        [NullValues("-")]
        public string? Skill3Name { get; set; }

        [Name("Skill 3 Lvl")]
        [NullValues("-")]
        public int? Skill3Lvl { get; set; }

        [Name("Group Skill")]
        [NullValues("-")]
        public string? GroupSkill { get; set; }

        [Name("Set Bonus")]
        [NullValues("-")]
        public string? SetBonus { get; set; }

        [Name("Forging Price")]
        public int ForgingPrice { get; set; }

        [Name("Forge Mat 1")]
        [NullValues("-")]
        public string? ForgeMat1 { get; set; }

        [Name("Forge Mat 1 Qty")]
        [NullValues("-")]
        public int? ForgeMat1Qty { get; set; }

        [Name("Forge Mat 2")]
        [NullValues("-")]
        public string? ForgeMat2 { get; set; }

        [Name("Forge Mat 2 Qty")]
        [NullValues("-")]
        public int? ForgeMat2Qty { get; set; }

        [Name("Forge Mat 3")]
        [NullValues("-")]
        public string? ForgeMat3 { get; set; }

        [Name("Forge Mat 3 Qty")]
        [NullValues("-")]
        public int? ForgeMat3Qty { get; set; }

        [Name("Forge Mat 4")]
        [NullValues("-")]
        public string? ForgeMat4 { get; set; }

        [Name("Forge Mat 4 Qty")]
        [NullValues("-")]
        public int? ForgeMat4Qty { get; set; }

        //private readonly Dictionary<string, string> pieceTemplateIconTypeMap = new()
        //{
        //    { "Head", "Helmet" },
        //    { "Chest", "Chestplate" },
        //    { "Arms", "Armguard" },
        //    { "Waist", "Waist" },
        //    { "Legs", "Leggings" }
        //};

        //public string GenerateArmorPieceTemplate()
        //{
        //    /* {{GenericArmorSetPiece
        //        |Game                  = 
        //        |Piece Name            = 
        //        |Max Level             = MISSING
        //        |Rarity                = 
        //        |Item Icon Type        = 
        //        |Male Image            = 
        //        |Female Image          = 
        //        |Description           = 
        //        |Level 1 Decos         = 
        //        |Level 2 Decos         = 
        //        |Level 3 Decos         = 
        //        |Level 4 Decos         =
        //        |Forging Cost          = 
        //        |Defense               = 
        //        |Fire Res              = 
        //        |Water Res             = 
        //        |Thunder Res           = 
        //        |Ice Res               = 
        //        |Dragon Res            = 
        //        |Skills                = 
        //        |Materials             = 
        //        }}*/

        //    const string game = "MHWilds";
        //    var sb = new StringBuilder("{{GenericArmorSetPiece\n");

        //    sb.AppendLine($"|Game                  = {game}");
        //    sb.AppendLine($"|Piece Name            = {PieceName}");
        //    //sb.AppendLine($"|Max Level             ="); TODO get from raw data
        //    sb.AppendLine($"|Rarity                = {Rarity}");
        //    sb.AppendLine($"|Item Icon Type        = {pieceTemplateIconTypeMap[PieceType]}");
        //    //sb.AppendLine($"|Male Image            = "); TODO need renders
        //    //sb.AppendLine($"|Female Image          = "); TODO need renders
        //    sb.AppendLine($"|Description           = {Description}");
        //    sb.AppendLine($"|Level 1 Decos         = {Lvl1Slots ?? 0}");
        //    sb.AppendLine($"|Level 2 Decos         = {Lvl2Slots ?? 0}");
        //    sb.AppendLine($"|Level 3 Decos         = {Lvl3Slots ?? 0}");
        //    sb.AppendLine($"|Forging Cost          = {ForgingPrice}");
        //    sb.AppendLine($"|Defense               = {BaseDefense}");
        //    sb.AppendLine($"|Fire Res              = {FireRes}");
        //    sb.AppendLine($"|Water Res             = {WaterRes}");
        //    sb.AppendLine($"|Thunder Res           = {ThunderRes}");
        //    sb.AppendLine($"|Ice Res               = {IceRes}");
        //    sb.AppendLine($"|Dragon Res            = {DragonRes}");

        //    var skillList = new List<KeyValuePair<string?, int?>>()
        //    {
        //        new(Skill1Name, Skill1Lvl),
        //        new(Skill2Name, Skill2Lvl),
        //        new(Skill3Name, Skill3Lvl)
        //    };
        //    var skillData = Json.DataHelpers.GetArmorSkills();
        //    sb.AppendLine("|Skills                = ");
        //    foreach (var pair in skillList)
        //    {
        //        if (!string.IsNullOrEmpty(pair.Key) && pair.Value.HasValue)
        //        {
        //            var m = Regex.Match(skillData.First(x => x.Name == pair.Key).SkillIconType, @"MHWilds-(\w+) Skill Icon\.png");
        //            sb.AppendLine($"<div>{{{{MHWildsSkillLink|{pair.Key}|{m.Groups[1]}}}}} x{pair.Value.Value}</div>");
        //        }
        //    }

        //    var forgeList = new List<KeyValuePair<string?, int?>>()
        //    {
        //        new(ForgeMat1, ForgeMat1Qty),
        //        new(ForgeMat2, ForgeMat2Qty),
        //        new(ForgeMat3, ForgeMat3Qty),
        //        new(ForgeMat4, ForgeMat4Qty)
        //    };
        //    // TODO skills & materials

        //    return sb.ToString();
        //}
    }
}
