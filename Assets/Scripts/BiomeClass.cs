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
    // Noise nastavení
    // ----------------------------------------------------------------
    [Header("Noise Settings")]
    public float terrainFreq      = 0.05f;
    public float caveFreq         = 0.05f;
    public int   maxCaveHeight    = 30;
    public int   minCaveHeight    = 5;

    // ----------------------------------------------------------------
    // Generační nastavení
    // ----------------------------------------------------------------
    [Header("Generation Settings")]
    public bool  generateCaves    = true;
    public int   dirtLayer        = 4;
    public float surfaceValue     = 0.10f;
    public float heightMultiplier = 4f;

    // ----------------------------------------------------------------
    // Rudy
    // ----------------------------------------------------------------
    [Header("Ore Settings")]
    public OreClass[] ores;

#if UNITY_EDITOR
    // Validace v Inspectoru - upozorní pokud ID nejsou v BlockRegistry
    public void ValidateBlockIDs(BlockRegistry registry)
    {
        if (registry == null) return;

        CheckID(registry, blockSurface,    "blockSurface");
        CheckID(registry, blockSubsurface, "blockSubsurface");
        CheckID(registry, blockDeep,       "blockDeep");

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