using UnityEngine;
using System.Linq;

[System.Serializable]
public class BiomeEntry
{
    public BiomeClass biome;
    [Tooltip("Jak často se biom bude vyskytovat (vyšší = více)]")]
    [Range(0.1f, 10f)]
    public float weight = 1f;
}

public class TerrainGeneration : MonoBehaviour
{
    [Header("References")]
    public WorldData worldData;
    public ChunkRenderer chunkRenderer;
    public BlockRegistry blockRegistry;

    [Header("Biomes")]
    public BiomeEntry[] biomes;

    [Header("World Settings")]
    public float seed;
    public int heightAddition = 0;

    private void OnValidate()
    {
        if (biomes == null) return;

        foreach (var entry in biomes)
        {
            if (entry.biome == null) continue;

            entry.biome.GenerateCavePreview(seed);

            if (entry.biome.ores != null)
            {
                for (int i = 0; i < entry.biome.ores.Length; i++)
                {
                    entry.biome.ores[i].GeneratePreview(seed, i);
                }
            }
        }
    }

    private void Start()
    {
        seed = Random.Range(-10000f, 10000f);

        worldData.Initialize();

        GenerateWorld();

        chunkRenderer.DrawWorld(worldData);
    }

    void GenerateWorld()
    {
        for (int x = 0; x < worldData.worldWidth; x++)
        {
            BiomeClass biome = GetBiome(x);
            if (biome == null) continue;

            int terrainHeight = Mathf.FloorToInt(
                Mathf.PerlinNoise((x + seed) * biome.terrainFreq, 0f) * biome.heightMultiplier
            ) + heightAddition;

            // Clamp výšky terénu aby nepřesáhl svět
            terrainHeight = Mathf.Clamp(terrainHeight, 1, worldData.worldHeight - 1);

            for (int y = 0; y < worldData.worldHeight; y++)
            {
                // Nad terénem nic negeneruj
                if (y > terrainHeight)
                    continue;

                // Jeskyně logika - OPRAVA: přeskočíme blok (=jeskyně) jen když:
                // 1. Biom má jeskyně povoleny
                // 2. Jsme ve správné výškové zóně
                // 3. Nejsme příliš blízko povrchu (ochrana povrchu = dirtLayer + 1)
                if (biome.generateCaves
                    && y >= biome.minCaveHeight
                    && y <= Mathf.Min(biome.maxCaveHeight, terrainHeight - biome.dirtLayer - 1))
                {
                    if (IsCave(x, y, biome))
                        continue; // tento blok je jeskyně -> přeskočit (prázdno)
                }

                GenerateBlock(x, y, terrainHeight, biome);
            }
        }
    }

    // Vrátí true pokud má být na pozici (x,y) jeskyně
    bool IsCave(int x, int y, BiomeClass biome)
    {
        // Worm-cave algoritmus: hledáme hodnoty blízko 0.5 ve dvou vrstvách šumu
        float noiseA = Mathf.PerlinNoise(
            (x + seed) * biome.caveFreq,
            (y + seed) * biome.caveFreq
        );

        // Druhá vrstva slouží jako maska - zabraňuje příliš velkým otevřeným prostorám
        float caveMask = Mathf.PerlinNoise(
            (x + seed + 100f) * (biome.caveFreq * 0.2f),
            (y + seed + 100f) * (biome.caveFreq * 0.2f)
        );

        bool isWorm = Mathf.Abs(noiseA - 0.5f) < biome.surfaceValue;
        bool maskOk  = caveMask > 0.45f;

        return isWorm && maskOk;
    }

    void GenerateBlock(int x, int y, int terrainHeight, BiomeClass biome)
    {
        string blockID;

        if (y == terrainHeight)
        {
            blockID = biome.blockSurface;
        }
        else if (y > terrainHeight - biome.dirtLayer)
        {
            blockID = biome.blockSubsurface;
        }
        else
        {
            blockID = GenerateOre(x, y, biome);
        }

        worldData.SetBlock(x, y, blockID);
    }

    // Vrátí block ID pro danou pozici - buď ID rudy nebo ID kamene
    string GenerateOre(int x, int y, BiomeClass biome)
    {
        if (biome.ores == null || biome.ores.Length == 0)
            return biome.blockDeep;

        for (int i = 0; i < biome.ores.Length; i++)
        {
            OreClass ore = biome.ores[i];

            if (y > ore.maxHeightSpawn)
                continue;

            float noise = Mathf.PerlinNoise(
                (x + seed + i * 1000f) * ore.rarity,
                (y + seed + i * 1000f) * ore.rarity
            );

            if (noise > ore.veinSize)
                return ore.blockID;
        }

        return biome.blockDeep;
    }

    // Vrátí biom pro daný X coordinate pomocí Perlin noise a vah biomů
    BiomeClass GetBiome(int x)
    {
        if (biomes == null || biomes.Length == 0)
        {
            Debug.LogError("TerrainGeneration: Nejsou přiřazeny žádné biomy!");
            return null;
        }

        float noise = Mathf.PerlinNoise((x + seed) * 0.01f, seed * 0.01f);

        float totalWeight = biomes.Sum(b => b.weight);
        float target = noise * totalWeight;
        float cumulative = 0f;

        foreach (var entry in biomes)
        {
            cumulative += entry.weight;
            if (target <= cumulative)
                return entry.biome;
        }

        return biomes[biomes.Length - 1].biome;
    }
}