using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeCol;

    // ----------------------------------------------------------------
    // Block ID klíče - musí existovat v BlockRegistry
    // ----------------------------------------------------------------
    [Header("Block IDs")]
    [Tooltip("Povrchový blok (tráva, písek, sníh...)")]
    public string blockSurface    = "Grass_block";

    [Tooltip("Povrchová vrstva pod povrchem (hlína, písek...)")]
    public string blockSubsurface = "Dirt_block";

    [Tooltip("Základní hornina")]
    public string blockDeep       = "Stone_block";

    [Tooltip("Blok pro dno světa (bedrock)")]
    public string blockBedrock    = "Stone_block";

    // ----------------------------------------------------------------
    // Generační nastavení
    // ----------------------------------------------------------------
    [Header("Generation Settings")]
    [Tooltip("Kolik bloků pod povrchem tvoří podpovrchová vrstva (blockSubsurface)")]
    public int   dirtLayer        = 4;

    // ----------------------------------------------------------------
    // Rudy
    // ----------------------------------------------------------------
    [Header("Ore Settings")]
    public OreClass[] ores;
    
    [System.NonSerialized] public ushort surfaceBlock;
    [System.NonSerialized] public ushort subsurfaceBlock;
    [System.NonSerialized] public ushort deepBlock;
    [System.NonSerialized] public ushort bedrockBlock;

    // Přeloží textová ID biomu a jeho rud na čísla z BlockRegistry.
    // Volá se jednou při startu hry, před generováním světa.
    public void ResolveBlockNumbers(BlockRegistry registry)
    {
        surfaceBlock    = registry.GetNumber(blockSurface);
        subsurfaceBlock = registry.GetNumber(blockSubsurface);
        deepBlock       = registry.GetNumber(blockDeep);
        bedrockBlock    = registry.GetNumber(blockBedrock);

        if (ores == null) return;

        foreach (var ore in ores)
            ore.blockNumber = registry.GetNumber(ore.blockID);
    }

#if UNITY_EDITOR
    // Validace v Inspectoru - upozorní pokud ID nejsou v BlockRegistry
    public void ValidateBlockIDs(BlockRegistry registry)
    {
        if (registry == null) return;

        CheckID(registry, blockSurface,    "blockSurface");
        CheckID(registry, blockSubsurface, "blockSubsurface");
        CheckID(registry, blockDeep,       "blockDeep");
        CheckID(registry, blockBedrock,    "blockBedrock");

        if (ores != null)
        {
            foreach (var ore in ores)
            {
                if (!string.IsNullOrEmpty(ore.blockID) && !registry.Exists(ore.blockID))
                    Debug.LogWarning($"BiomeClass '{biomeName}': Ore blockID '{ore.blockID}' neexistuje v BlockRegistry!");
            }
        }
    }

    private void CheckID(BlockRegistry registry, string id, string fieldName)
    {
        if (string.IsNullOrEmpty(id))
            Debug.LogWarning($"BiomeClass '{biomeName}': {fieldName} není vyplněn!");
        else if (!registry.Exists(id))
            Debug.LogWarning($"BiomeClass '{biomeName}': {fieldName} = '{id}' neexistuje v BlockRegistry!");
    }
#endif
}