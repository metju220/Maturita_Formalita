using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Centrální registr všech bloků ve hře.
// Vytvořit přes: Assets > Create > World > Block Registry
// Sem přidáš každý blok jednou - bez ohledu na to kolik biomů ho používá.
[CreateAssetMenu(fileName = "BlockRegistry", menuName = "World/Block Registry")]
public class BlockRegistry : ScriptableObject
{
    [System.Serializable]
    public class BlockDefinition
    {
        public string blockID;      // Unikátní klíč, např. "grass_forest", "sand", "coal"
        public TileBase tile;       // Tile přiřazená v Inspectoru
        public bool isSolid = true;
        public bool isTransparent = false;

        [Tooltip("Zvuk při rozbití (volitelné, pro budoucí použití)")]
        public string breakSound;
    }

    [SerializeField]
    public List<BlockDefinition> blockDefinitions = new List<BlockDefinition>();

    // Runtime cache - postaví se při prvním dotazu
    private Dictionary<string, BlockDefinition> _cache;

    private void BuildCache()
    {
        _cache = new Dictionary<string, BlockDefinition>(blockDefinitions.Count);
        foreach (var def in blockDefinitions)
        {
            if (string.IsNullOrEmpty(def.blockID))
            {
                Debug.LogWarning($"BlockRegistry: Blok bez ID nalezen, přeskakuji.");
                continue;
            }
            if (_cache.ContainsKey(def.blockID))
            {
                Debug.LogWarning($"BlockRegistry: Duplicitní ID '{def.blockID}', přeskakuji.");
                continue;
            }
            _cache[def.blockID] = def;
        }
    }

    // Vrátí TileBase pro dané ID. Vrátí null pokud ID neexistuje.
    public TileBase GetTile(string blockID)
    {
        if (string.IsNullOrEmpty(blockID)) return null;

        if (_cache == null) BuildCache();

        if (_cache.TryGetValue(blockID, out BlockDefinition def))
            return def.tile;

        Debug.LogWarning($"BlockRegistry: Neznámé block ID '{blockID}'");
        return null;
    }

    // Vrátí definici bloku (pro budoucí použití - zvuky, vlastnosti, atd.)
    public BlockDefinition GetDefinition(string blockID)
    {
        if (string.IsNullOrEmpty(blockID)) return null;

        if (_cache == null) BuildCache();

        _cache.TryGetValue(blockID, out BlockDefinition def);
        return def;
    }

    public bool Exists(string blockID)
    {
        if (_cache == null) BuildCache();
        return !string.IsNullOrEmpty(blockID) && _cache.ContainsKey(blockID);
    }

    // Invaliduje cache - zavolej pokud změníš definice za runtime
    public void InvalidateCache() => _cache = null;

    private void OnValidate() => _cache = null;
}