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
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generateCaves = true;
    public int dirtLayer = 4;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;

    [Header("Ore Settings")]
    public OreClass[] ores;
}
