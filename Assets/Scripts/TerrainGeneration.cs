using System.Collections.Generic;
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
    [Tooltip("Zapnuto = při každém spuštění nový náhodný svět. Vypnuto = použije se seed níže (stejný svět jde vygenerovat znovu).")]
    public bool randomSeed = true;
    public float seed;

    [Header("Terrain Settings")]
    [Range(0.005f, 0.05f)]
    public float terrainScale = 0.01f; // Jak moc jsou kopce vysoke
    public int minTerrainHeight = 20;  // Minimální výška terénu
    public int maxTerrainHeight = 60;  // Maximální výška terénu

    // Spustí se v editoru při změně hodnot (vygeneruje náhledy biomů a rud v unity inspectoru)
    private void OnValidate()
    {
        GenerateBiomePreview();

        if (biomeThresholds == null) return;

        // Při duplikaci prvku pole v Inspectoru se zkopíruje i odkaz na texturu náhledu,
        // takže by víc rud kreslilo do jedné textury. Sdílenou texturu proto zahodíme
        // a GeneratePreview vytvoří pro danou rudu novou.
        var usedPreviews = new HashSet<Texture2D>();

        foreach (var threshold in biomeThresholds)
        {
            if (threshold.biome == null || threshold.biome.ores == null) continue;

            for (int i = 0; i < threshold.biome.ores.Length; i++)
            {
                OreClass ore = threshold.biome.ores[i];

                if (ore.spread != null && !usedPreviews.Add(ore.spread))
                    ore.spread = null;

                ore.GeneratePreview(seed, i);
            }
        }
    }

    private void Start()
    {
        // 1. Seed světa - buď náhodný, nebo ten z Inspectoru
        if (randomSeed)
            seed = Random.Range(-10000f, 10000f);
        Debug.Log($"[World] Seed: {seed}");

        // 2. Příprava polí pro data světa
        worldData.Initialize();

        // 3. Hlavní generování (krok za krokem)
        GenerateWorld();

        // 4. Vykreslení vygenerovaných dat do Tilemapy
        chunkRenderer.DrawWorld(worldData);

        // 5. Neviditelné stěny na okrajích mapy
        CreateWorldBounds();

        // 6. Spawn hráče na povrch
        SpawnPlayer();
    }

    public void GenerateBiomePreview()
    {
        int texWidth = 512;
        int texHeight = 64;

        // DontSave = náhled se neukládá do scény, vždy se dopočítá znovu
        if (biomePreviewTexture == null
            || biomePreviewTexture.hideFlags != HideFlags.DontSave
            || biomePreviewTexture.width != texWidth
            || biomePreviewTexture.height != texHeight)
        {
            biomePreviewTexture = new Texture2D(texWidth, texHeight);
            biomePreviewTexture.filterMode = FilterMode.Point;
            biomePreviewTexture.wrapMode = TextureWrapMode.Clamp;
            biomePreviewTexture.hideFlags = HideFlags.DontSave;
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

    // Vytvoří neviditelné stěny vlevo a vpravo od mapy, aby hráč nemohl vypadnout ze světa
    void CreateWorldBounds()
    {
        GameObject bounds = new GameObject("WorldBounds");
        float wallHeight = worldData.worldHeight * 2f; // s rezervou nad horním okrajem mapy

        BoxCollider2D leftWall = bounds.AddComponent<BoxCollider2D>();
        leftWall.offset = new Vector2(-0.5f, wallHeight / 2f);
        leftWall.size = new Vector2(1f, wallHeight);

        BoxCollider2D rightWall = bounds.AddComponent<BoxCollider2D>();
        rightWall.offset = new Vector2(worldData.worldWidth + 0.5f, wallHeight / 2f);
        rightWall.size = new Vector2(1f, wallHeight);
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
        GenerateTerrain();
        GenerateCavesWorm();
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

    // Vyplní každý sloupec bloky podle biomu: povrch, podpovrchová vrstva, hornina a dole bedrock
    void GenerateTerrain()
    {
        for (int x = 0; x < worldData.worldWidth; x++)
        {
            BiomeClass biome = GetBiome(x); // Zjistí, jaký biom je na této X pozici
            if (biome == null)
            {
                Debug.LogError("TerrainGeneration: Není nastavený žádný biom, svět nejde vygenerovat!");
                return;
            }

            int terrainHeight = worldData.heightMap[x];

            for (int y = 0; y <= terrainHeight; y++) // Nad povrchem zůstává vzduch
            {
                worldData.SetBlock(x, y, GetBiomeBlock(y, terrainHeight, biome));
            }
        }
    }

    // generátor jeskyní
    void GenerateCavesWorm()
    {
        WormCaveGenerator.GenerateCaves(worldData, seed);
    }

    // Vrací správný blok pro biom na základě hloubky
    string GetBiomeBlock(int y, int terrainHeight, BiomeClass biome)
    {
        if (y < worldData.bedrockLayers) return biome.blockBedrock; // Nezničitelné dno světa

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

            // Od bedrocku výš - bedrock je stejný kámen jako blockDeep, ruda ho nesmí přepsat
            for (int y = worldData.bedrockLayers; y < terrainHeight; y++)
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