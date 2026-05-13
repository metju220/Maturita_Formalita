using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeCol;
    public TileAtlas tileAtlas;

    [Header("Noise Settings")]
    public float terrainFreq;
    public float caveFreq = 0.05f;
    public int maxCaveHeight = 30;
    public int minCaveHeight = 5;
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generateCaves = true;
    public int dirtLayer = 4;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;

    [Header("Ore Settings")]
    public OreClass[] ores;

    public void GenerateCavePreview(float seed)
    {
        if (!generateCaves) return;

        if (caveNoiseTexture == null || caveNoiseTexture.width != 128 || caveNoiseTexture.height != 128)
        {
            caveNoiseTexture = new Texture2D(128, 128);
            caveNoiseTexture.filterMode = FilterMode.Point;
        }

        // Tloušťka nudle (čím menší, tím tenčí chodba)
        float thickness = 0.05f;

        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                float caveNoise = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);

                // KLÍČOVÁ ZMĚNA: Hledáme hodnoty blízko 0.5
                float distanceToCenter = Mathf.Abs(caveNoise - 0.5f);

                Color color = distanceToCenter < thickness ? Color.white : Color.black;
                caveNoiseTexture.SetPixel(x, y, color);
            }
        }
        caveNoiseTexture.Apply();
    }
}
