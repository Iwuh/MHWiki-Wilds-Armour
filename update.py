import os, json, sys, shutil

user_paths = [
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/ArmorData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/ArmorRecipeData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/ArmorSeriesData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/ArmorSpUpgradeCostData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/ArmorUpgradeData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/ArmorUpgradeRecipeData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/SkillCommonData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Equip/SkillData.user.3.json",
    "MHWs-in-json/natives/STM/GameDesign/Common/Item/itemData.user.3.json",
]

msg_paths = [
    "MHWs-in-json/natives/STM/GameDesign/Text/Excel_Equip/Armor.msg.23.json",
    "MHWs-in-json/natives/STM/GameDesign/Text/Excel_Equip/ArmorSeries.msg.23.json",
    "MHWs-in-json/natives/STM/GameDesign/Text/Excel_Equip/Skill.msg.23.json",
    "MHWs-in-json/natives/STM/GameDesign/Text/Excel_Equip/SkillCommon.msg.23.json",
    "MHWs-in-json/natives/STM/GameDesign/Text/Excel_Equip/SkillType.msg.23.json",
    "MHWs-in-json/natives/STM/GameDesign/Text/Excel_Data/Item.msg.23.json",
]

serializable_keys_one = ["_Series", "_PartsType", "_Rare", "_StoryNo", "_ModelVariety", "_Color"]
serializable_keys_array = ["_SlotLevel", "_Skill"]

def load_file(path: str):
    with open(path, 'r') as f:
        return json.load(f)
    
def write_file(data, path: str):
    with open(path, 'w') as f:
        json.dump(data, f, indent=4)

def flatten(data):
    inner1 = data[0]
    inner2key, inner2val = next(iter(inner1.items()))
    inner3 = inner2val['_Values']
    flattened = []
    for item in inner3:
        inner4 = item[inner2key + ".cData"]
        for k,v in inner4.items():
            if isinstance(v, dict) and k in serializable_keys_one:
                _,serval = next(iter(v.items()))
                inner4[k] = serval["_Value"]
            elif isinstance(v, list) and k in serializable_keys_array:
                inner4[k] = []
                for serinner in v:
                    _,serval = next(iter(serinner.items()))
                    inner4[k].append(serval["_Value"])
        flattened.append(inner4)
    return flattened


def main():
    if sys.platform == "win32":
        data_out = os.path.join("%LOCALAPPDATA%", "MHWildsArmour", "data")
    elif sys.platform == "linux":
        data_root = os.getenv("XDG_DATA_HOME")
        if not data_root:
            print("XDG_DATA_HOME not set, defaulting to cwd")
            data_root = os.path.curdir
        data_out = os.path.join(data_root, "MHWildsArmour", "data")
    else:
        # I don't know what the standard path for app data is on Mac ¯\_(ツ)_/¯
        print("Unsupported OS")
        sys.exit(1)

    os.makedirs(data_out, exist_ok=True)

    for p in user_paths:
        data = load_file(p)
        write_file(flatten(data), os.path.join(data_out, os.path.basename(p)))

    for p in msg_paths:
        shutil.copy(p, os.path.join(data_out, os.path.basename(p)))

if __name__ == "__main__":
    main()