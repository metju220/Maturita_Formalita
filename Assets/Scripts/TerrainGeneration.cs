using UnityEngine;
using UnityEngine.Serialization;

// Pomocná třída pro nastavení biomů v Inspectoru
[System.Serializable]
public class BiomeThreshold
{
    public BiomeClass biome;       // Jaký biom to je (data)
    //public float minNoise = 0f;    // Od jaké hodnoty šumu začíná
    //public float maxNoise = 1f;    // Do jaké hodnoty šumu končí
}

public class TerrainGeneration : MonoBehaviour
{
    [Header("References")]
    public WorldData worldData;
    public ChunkRenderer chunkRenderer;
    public BlockRegistry blockRegistry;
    public Transform player;

    [Header("Biomes")]
    public BiomeThreshold[] biomeThresholds; // Tady ti zůstanou přiřazené tvé biomy

    [Header("Biome Distribution (Gradient)")]
    public Gradient biomeGradient; // Barevná lajna pro určování četnosti biomů

    [Tooltip("Čím menší číslo, tím delší a větší biomy budou (původně 0.01f)")]
    public float biomeScale = 0.003f;

    [Header("Biome Preview")]
    // Tady v inspectoru uvidíš krásný 512x64 pruh, jak jdou biomy za sebou
    public Texture2D biomePreviewTexture;

    [Header("World Settings")]
    public float seed;

    [Header("Terrain Settings")]
    [Range(0.005f, 0.05f)]
    public float terrainScale = 0.01f; // Jak moc jsou kopce vysoke
    public int minTerrainHeight = 20;  // Minimální výška terénu
    public int maxTerrainHeight = 60;  // Maximální výška terénu

    // Spustí se v editoru při změně hodnot (vygeneruje náhledy jeskyní/rud v unity inspectoru)
    private void OnValidate()
    {   
        GenerateBiomePreview();

        if (biomeThresholds == null) return;

        foreach (var threshold in biomeThresholds)
        {
            if (threshold.biome == null) continue;
            threshold.biome.GenerateCavePreview(seed);

            if (threshold.biome.ores != null)
            {
                for (int i = 0; i < threshold.biome.ores.Length; i++)
                {
                    threshold.biome.ores[i].GeneratePreview(seed, i);
                }
            }
        }
    }

    private void Start()
    {
        // 1. Náhodný seed pro nový svět
        seed = Random.Range(-10000f, 10000f);

        // 2. Příprava polí pro data světa
        worldData.Initialize();

        // 3. Hlavní generování (krok za krokem)
        GenerateWorld();

        // 4. Vykreslení vygenerovaných dat do Tilemapy
        chunkRenderer.DrawWorld(worldData);

        // 5. Spawn hráče na povrch
        SpawnPlayer();
    }

    public void GenerateBiomePreview()
    {
        int texWidth = 512;
        int texHeight = 64;

        if (biomePreviewTexture == null 
            || biomePreviewTexture.width != texWidth 
            || biomePreviewTexture.height != texHeight)
        {
            biomePreviewTexture = new Texture2D(texWidth, texHeight);
            biomePreviewTexture.filterMode = FilterMode.Point;
            biomePreviewTexture.wrapMode = TextureWrapMode.Clamp;
        }

        // Projdeme texturu pixel po pixelu horizontálně
        for (int x = 0; x < texWidth; x++)
        {
            // Simulujeme horizontální šum světa (používáme x přímo jako souřadnici světa)
            float noise = Mathf.PerlinNoise((x + seed) * biomeScale, seed * 0.01f);
            
            // Vytáhneme barvu z gradientu
            Color resultColor = biomeGradient.Evaluate(noise);

            // Vybarvíme celý vertikální sloupec (od y = 0 do 64) touto barvou
            for (int y = 0; y < texHeight; y++)
            {
                biomePreviewTexture.SetPixel(x, y, resultColor);
            }
        }

        biomePreviewTexture.Apply();
    }

    // spawnuje hráče bezpečně na povrch uprostřed mapy
    void SpawnPlayer()
    {
        if (player == null) return;

        int spawnX = worldData.worldWidth / 2; // Střed mapy na ose X
        int spawnY = FindSurfaceY(spawnX);     // Najde nejvyšší pevný blok

        if (spawnY < 0) // Kdyby selhal vyhledávač, vezme výšku z heightmapy
        {
            spawnY = worldData.heightMap[spawnX] + 1;
        }

        // Posune hráče na pozici (přidá 0.5f pro střed bloku)
        player.position = new Vector3(spawnX + 0.5f, spawnY, player.position.z);
    }

    // Hledá nejbližší povrchový blok kolem středu mapy směrem dolů/nahoru
    int FindSurfaceY(int startX)
    {
        for (int offset = 0; offset < worldData.worldWidth; offset++)
        {
            foreach (int x in new int[] { startX + offset, startX - offset })
            {
                if (x < 0 || x >= worldData.worldWidth) continue;

                int terrainHeight = worldData.heightMap[x];
                string blockAtSurface = worldData.GetBlock(x, terrainHeight);

                if (!string.IsNullOrEmpty(blockAtSurface))
                {
                    return terrainHeight + 2; // Vrátí pozici kousek nad blokem
                }
            }
        }
        return -1;
    }

    void GenerateWorld()
    {
        GenerateHeightMap();
        GenerateBasicTerrain();
        GenerateCavesWorm();
        ApplyBiomes();
        ApplyOres();
    }

    // Pomocí Perlin Noise spočítá výšku terénu pro každý X sloupec
    void GenerateHeightMap()
    {
        for (int x = 0; x < worldData.worldWidth; x++)
        {
            float noise = Mathf.PerlinNoise((x + seed) * terrainScale, seed * 0.01f);
            int height = Mathf.FloorToInt(Mathf.Lerp(minTerrainHeight, maxTerrainHeight, noise));
            worldData.heightMap[x] = Mathf.Clamp(height, minTerrainHeight, maxTerrainHeight);
        }
    }

    // Vyplní svět základem: na povrchu tráva, pod ní hlína, pak kámen
    void GenerateBasicTerrain()
    {
        for (int x = 0; x < worldData.worldWidth; x++)
        {
            int terrainHeight = worldData.heightMap[x];

            for (int y = 0; y < worldData.worldHeight; y++)
            {
                if (y > terrainHeight) continue; // Nad povrchem je vzduch (přeskočit)

                string blockID = GetBlockForDepth(y, terrainHeight);
                worldData.SetBlock(x, y, blockID);
            }
        }
    }

    // Pomocná funkce pro určení základního bloku podle hloubky
    string GetBlockForDepth(int y, int terrainHeight)
    {
        int depthFromSurface = terrainHeight - y;

        if (depthFromSurface == 0) return "grass_forest"; // Úplný povrch
        if (depthFromSurface <= 5) return "dirt";         // Vrstva pod povrchem
        return "stone";                                   // Hluboké podzemí
    }

    // generátor jeskyní
    void GenerateCavesWorm()
    {
        WormCaveGenerator.GenerateCaves(worldData, seed);
    }

    // Projede svět a vymění základní bloky za specifické bloky daného biomu
    void ApplyBiomes()
    {
        for (int x = 0; x < worldData.worldWidth; x++)
        {
            BiomeClass biome = GetBiome(x); // Zjistí, jaký biom je na této X pozici
            if (biome == null) continue;

            int terrainHeight = worldData.heightMap[x];

            for (int y = 0; y <= terrainHeight; y++)
            {
                string currentBlockID = worldData.GetBlock(x, y);
                if (string.IsNullOrEmpty(currentBlockID)) continue; // Jeskyně (vzduch) nepřepisujeme

                // Nahradí původní blok biomovým ekvivalentem
                string newBlockID = GetBiomeBlock(y, terrainHeight, biome, currentBlockID);
                worldData.SetBlock(x, y, newBlockID);
            }
        }
    }

    // Vrací správný blok pro biom na základě hloubky
    string GetBiomeBlock(int y, int terrainHeight, BiomeClass biome, string currentBlock)
    {
        int depthFromSurface = terrainHeight - y;

        if (depthFromSurface == 0) return biome.blockSurface;       // Povrch biomu (např. písek v poušti)
        if (depthFromSurface <= biome.dirtLayer) return biome.blockSubsurface; // Podpovrch biomu
        return biome.blockDeep;                                      // Hluboký blok biomu
    }

    // Vygeneruje ložiska rud hluboko v zemi pomocí Perlin Noise
    void ApplyOres()
    {
        int totalOresPlaced = 0;

        for (int x = 0; x < worldData.worldWidth; x++)
        {
            BiomeClass biome = GetBiome(x);
            if (biome == null || biome.ores == null) continue;

            int terrainHeight = worldData.heightMap[x];

            for (int y = 0; y < terrainHeight; y++)
            {
                string currentBlockID = worldData.GetBlock(x, y);

                if (string.IsNullOrEmpty(currentBlockID)) continue;
                if (currentBlockID != biome.blockDeep) continue; // Rudy se generují JEN do hlubokého kamene

                // Projede všechny dostupné rudy pro tento biom
                for (int oreIndex = 0; oreIndex < biome.ores.Length; oreIndex++)
                {
                    OreClass ore = biome.ores[oreIndex];

                    if (y >= ore.maxHeightSpawn) continue; // Pokud jsme moc vysoko, ruda tu nesmí být

                    // 2D Perlin Noise pro tvorbu "žil" neboli ložisek rudy
                    float noise = Mathf.PerlinNoise(
                        (x + seed + oreIndex * 1000f) * ore.rarity,
                        (y + seed + oreIndex * 1000f) * ore.rarity
                    );

                    // Pokud šum překročí velikost žíly, umístí se ruda
                    if (noise > ore.veinSize)
                    {
                        worldData.SetBlock(x, y, ore.blockID);
                        totalOresPlaced++;
                        break; // Na jednom pixelu vygenerujeme max jednu rudu, jdeme na další
                    }
                }
            }
        }
        Debug.Log($"[Ores] Total ore blocks placed: {totalOresPlaced}");
    }

// Určí biom pro konkrétní X souřadnici podle horizontálního šumu a barvy z gradientu
    BiomeClass GetBiome(int x)
    {
        if (biomeThresholds == null || biomeThresholds.Length == 0) return null;

        // 1. Získáme hladkou hodnotu šumu pro dané X.
        // Používáme biomeScale (např. 0.003f), což ti vyřeší ty malé 10-blokové flíčky!
        float noise = Mathf.PerlinNoise((x + seed) * biomeScale, seed * 0.01f);

        // 2. Podle šumu vytáhneme barvu z našeho nastaveného Gradientu
        Color gradientColor = biomeGradient.Evaluate(noise);

        // 3. Najdeme biom, který má v BiomeClass barvu (biomeCol) nejblíže této barvě
        BiomeClass bestMatch = biomeThresholds[0].biome;
        float minDistance = float.MaxValue;

        for (int i = 0; i < biomeThresholds.Length; i++)
        {
            BiomeClass currentBiome = biomeThresholds[i].biome;
            if (currentBiome == null) continue;

            // Spočítáme rozdíl mezi barvou v gradientu a barvou v biomu
            float dist = Mathf.Sqrt(
                Mathf.Pow(gradientColor.r - currentBiome.biomeCol.r, 2) +
                Mathf.Pow(gradientColor.g - currentBiome.biomeCol.g, 2) +
                Mathf.Pow(gradientColor.b - currentBiome.biomeCol.b, 2)
            );

            if (dist < minDistance)
            {
                minDistance = dist;
                bestMatch = currentBiome;
            }
        }

        return bestMatch;
    }
}