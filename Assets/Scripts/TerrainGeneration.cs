using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [Header("References")]
    public WorldData worldData;
    public ChunkRenderer chunkRenderer;

    [Header("Biomes")]
    public BiomeClass[] biomes;

    [Header("World Settings")]
    public float seed;

    [Header("Terrain")]
    public float terrainFreq = 0.05f;
    public float heightMultiplier = 5f;
    public int heightAddition = 20;

    private void OnValidate()
    {
        if (biomes == null) return;

        foreach (var biome in biomes)
        {
            // 1. Vygeneruje náhled jeskyní pro biome
            biome.GenerateCavePreview(seed);

            // 2. Vygeneruje náhledy pro všechny rudy v tomto biomu
            if (biome.ores != null)
            {
                for (int i = 0; i < biome.ores.Length; i++)
                {
                    biome.ores[i].GeneratePreview(seed, i);
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
            int terrainHeight = Mathf.FloorToInt(
                Mathf.PerlinNoise((x + seed) * biome.terrainFreq, 0) * biome.heightMultiplier) + heightAddition;

            for (int y = 0; y < worldData.worldHeight; y++)
            {
                if (y > terrainHeight)
                    continue;

                bool cave = false;
                if (biome.generateCaves)
                {
                    if (y >= biome.minCaveHeight && y <= biome.maxCaveHeight)
                    {
                        float noiseA = Mathf.PerlinNoise((x + seed) * biome.caveFreq, (y + seed) * biome.caveFreq);
                        float caveMask = Mathf.PerlinNoise((x + seed + 100f) * (biome.caveFreq * 0.2f), (y + seed + 100f) * (biome.caveFreq * 0.2f));

                        // Pokud je hodnota blízko 0.5, "vykopeme" jeskyni
                        if (Mathf.Abs(noiseA - 0.5f) < biome.surfaceValue && caveMask > 0.45f)
                        {
                            cave = true;
                        }
                    }
                }

                if (cave && y < terrainHeight - 3)
                    continue;

                GenerateBlock(x, y, terrainHeight, biome);
            }
        }
    }

    void GenerateBlock(int x, int y, int height, BiomeClass biome)
    {
        int blockID = 0;

        if (y == height)
        {
            blockID = 1; // grass
        }
        else if (y > height - biome.dirtLayer)
        {
            blockID = 2; // dirt
        }
        else
        {
            blockID = GenerateOre(x, y, biome);
        }

        worldData.SetBlock(x, y, blockID);
    }

    int GenerateOre(int x, int y, BiomeClass biome)
    {
        for (int i = 0; i < biome.ores.Length; i++)
        {
            OreClass ore = biome.ores[i];

            if (y > ore.maxHeightSpawn) continue;

            float noise = Mathf.PerlinNoise(
                (x + seed + i * 1000) * ore.rarity,
                (y + seed + i * 1000) * ore.rarity
            );

            // Čím VYŠŠÍ veinSize, tím MÉNĚ rudy bude
            if (noise > ore.veinSize)
            {
                return 4 + i;
            }
        }

        return 3; // Stone
    }

    BiomeClass GetBiome(int x)
    {
        float noise = Mathf.PerlinNoise((x + seed) * 0.01f, seed * 0.01f);

        int index = Mathf.FloorToInt(noise * biomes.Length);

        index = Mathf.Clamp(index, 0, biomes.Length - 1);

        return biomes[index];
    }
}