using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;
    
    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFreq;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Generation Settings")]
    public int worldSize = 128;
    public int chunkSize = 16;
    public bool caveGen = true;
    public int dirtLayer = 4;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;

    [Header("Noise Settings")]
    public float caveFreq = 0.05f;
    public float terrainFreq = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();
    private BiomeClass curBiome;

    void OnValidate()
    {
        DrawTextures();
    }

    private void Start()
    {
        seed = Random.Range(-10000, 10000);

        DrawTextures();
        CreateChunks();
        GenerateTerrain();
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            if (biomes[i].biomeCol == biomeMap.GetPixel(x, y))
            {
                return biomes[i];
            }
        }
        return curBiome;
    }

    public void DrawTextures()
    {
            biomeMap = new Texture2D(worldSize, worldSize);
            DrawBiomeTexture();

        for(int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for(int o = 0; o< biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spread = new Texture2D(worldSize, worldSize);
            }

            GenerateNoiseTexture(biomes[i].caveFreq, biomes[i].surfaceValue, biomes[i].caveNoiseTexture);

            //Ores
            for(int o = 0; o< biomes[i].ores.Length; o++)
            {
                GenerateNoiseTexture(biomes[i].ores[o].rarity, biomes[i].ores[o].veinSize, biomes[i].ores[o].spread);
            }
        }
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];
        for(int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject(name = i.ToString());
            newChunk.name = i.ToString();
            newChunk.transform.SetParent(transform);
            worldChunks[i] = newChunk;
        }
    }

    public void DrawBiomeTexture()
    {
        for(int x = 0; x < biomeMap.width; x++)
        {
            for(int y = 0; y < biomeMap.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * biomeFreq, seed * biomeFreq);
                Color col = biomeGradient.Evaluate(v);
                biomeMap.SetPixel(x, y, col);
            }
        }
        biomeMap.Apply();
    }
    

    public void GenerateTerrain()
    {
        Sprite[] tileSprite;
        for(int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for(int y = 0; y < height; y++)
            {
                if(y < height - dirtLayer)
                {
                    curBiome = GetCurrentBiome(x, y);
                    tileSprite = curBiome.tileAtlas.stone.tileSprite;

                    if(ores[0].spread.GetPixel(x, y).r > 0.05f && height - y > ores[0].maxHeightSpawn)
                        tileSprite = tileAtlas.coal.tileSprite;

                    if(ores[1].spread.GetPixel(x, y).r > 0.05f && height - y > ores[1].maxHeightSpawn)
                        tileSprite = tileAtlas.iron.tileSprite;

                    if(ores[2].spread.GetPixel(x, y).r > 0.05f && height - y > ores[2].maxHeightSpawn)
                        tileSprite = tileAtlas.gold.tileSprite;
                }
                else if(y < height - 1)
                {
                    tileSprite = curBiome.tileAtlas.dirt.tileSprite;
                }
                else
                {
                    tileSprite = curBiome.tileAtlas.grass.tileSprite;
                }

                if (caveGen)
                {
                    if(caveNoiseTexture.GetPixel(x, y).r > 0.05f)
                    {
                        PlaceTile(tileSprite, x , y);
                    }
                }
                else
                {
                    PlaceTile(tileSprite, x , y);
                }
            }
        }


    }

    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for(int x = 0; x < noiseTexture.width; x++)
        {
            for(int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if(v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
                
            }
        }
        noiseTexture.Apply();
    }

    public void PlaceTile(Sprite[] tileSprite, int x, int y)
    {
        if(!worldTiles.Contains(new Vector2Int(x, y)))
        {
            GameObject newTile = new GameObject();

            int chunkCoord = Mathf.RoundToInt(x / chunkSize) * chunkSize;
            chunkCoord /= chunkSize;

            newTile.transform.SetParent(worldChunks[chunkCoord].transform);

            int spriteIndex = Random.Range(0, tileSprite.Length);
            newTile.AddComponent<SpriteRenderer>();
            newTile.GetComponent<SpriteRenderer>().sprite = tileSprite[spriteIndex];

            newTile.name = tileSprite[spriteIndex].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        }
    }
}
