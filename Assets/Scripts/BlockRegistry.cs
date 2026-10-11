using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Centrální registr všech bloků ve hře.
// Vytvořit přes: Assets > Create > World > Block Registry
// Sem přidáš každý blok jednou - bez ohledu na to kolik biomů ho používá.
[CreateAssetMenu(fileName = "BlockRegistry", menuName = "World/Block Registry")]
public class BlockRegistry : ScriptableObject
{
    public const ushort Air = 0;

    [System.Serializable]
    public class BlockDefinition
    {
        public string blockID;      // Unikátní klíč, např. "Grass_block", "Sand", "Coal_ore"
        public TileBase tile;       // Tile přiřazená v Inspectoru
    }

    [SerializeField]
    private List<BlockDefinition> blockDefinitions = new();
    private Dictionary<string, ushort> _nameToNum;
    private List<BlockDefinition> _numberToDef;

    private void BuildCache()
    {
        _nameToNum = new Dictionary<string, ushort>();
        _numberToDef = new List<BlockDefinition> { null };

        foreach (var def in blockDefinitions)
        {
            if (string.IsNullOrEmpty(def.blockID))
            {
                Debug.LogWarning("BlockRegistry: Blok bez ID nalezen, přeskakuji.");
                continue;
            }
            if (_nameToNum.ContainsKey(def.blockID))
            {
                Debug.LogWarning($"BlockRegistry: Duplicitní ID '{def.blockID}', přeskakuji.");
                continue;
            }
            
            ushort num = (ushort)_numberToDef.Count;
            _nameToNum[def.blockID] = num;
            _numberToDef.Add(def);
        }
    }

    // Text na cislo
    public ushort GetNumber(string blockID)
    {
        if (_nameToNum == null) BuildCache();

        if (!string.IsNullOrEmpty(blockID) && _nameToNum.TryGetValue(blockID, out ushort num))
            return num;
            
        Debug.LogWarning($"BlockRegistry: Neznámé block ID '{blockID}', použiji vzduch.");
        return Air;
    }

    // Cislo na text
    public string GetName(ushort number)
    {
        if (_numberToDef == null) BuildCache();

        if (number == Air || number >= _numberToDef.Count) return "Air";
        return _numberToDef[number].blockID;
    }

    //Cislo na grafiku
    public TileBase GetTile(ushort number)
    {
        if (_numberToDef == null) BuildCache();

        if (number == Air || number >= _numberToDef.Count) return null;
        return _numberToDef[number].tile;
    }

    public bool Exists(string blockID)
    {
        if (_nameToNum == null) BuildCache();
        return !string.IsNullOrEmpty(blockID) && _nameToNum.ContainsKey(blockID);
    }

    // Při změně registru v Inspectoru se cache zahodí a příště postaví znovu
    private void OnValidate() 
    {
        _nameToNum = null;
        _numberToDef = null;
    }
}