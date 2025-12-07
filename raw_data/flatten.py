import os, json

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
    files = [x for x in os.listdir('.') if os.path.isfile(x)]
    for f in files:
        (root, ext) = os.path.splitext(f)
        if ext == ".json" and not 'flat' in root:
            data = load_file(f)
            flat = flatten(data)
            write_file(flat, root + ".flat" + ext)

if __name__ == "__main__":
    main()