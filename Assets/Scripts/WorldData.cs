using UnityEngine;

public class WorldData : MonoBehaviour
{
    [Header("World Size")]
    public int worldWidth  = 200;
    public int worldHeight = 80;

    [Tooltip("Počet řad světa tvořících nezničitelný bedrock")]
    public int bedrockLayers = 1;

    [HideInInspector]
    public ushort[,] blockMap; // 2D mřížka (X, Y) naplněná textovými ID bloků

    [HideInInspector]
    public int[] heightMap; // 1D pole, které si pamatuje nejvyšší bod povrchu pro každý sloupec X

    // Vytvoří prázdný svět
    public void Initialize()
    {
        blockMap = new ushort[worldWidth, worldHeight];
        heightMap = new int[worldWidth];
    }

    // Bezpečně zapíše blok na zadané souřadnice
    public void SetBlock(int x, int y, ushort block)
    {
        if (IsInBounds(x, y))
            blockMap[x, y] = block; // Pokud by někdo poslal null, uloží se ""
    }

    // Bezpečně vrátí ID bloku na zadaných souřadnicích
    public ushort GetBlock(int x, int y)
    {
        if (IsInBounds(x, y))
            return blockMap[x, y];
        return BlockRegistry.Air;
    }

    // Rychlá kontrola, jestli je na dané pozici vzduch
    public bool IsAir(int x, int y)
    {
        return GetBlock(x, y) == BlockRegistry.Air;
    }

    // Hlídač, který kontroluje, jestli zadané X a Y vůbec leží uvnitř mapy
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
    }

    // Jde blok na této pozici vytěžit? (bedrock na dně světa nejde)
    public bool IsBreakable(int x, int y)
    {
        return IsInBounds(x, y) && y >= bedrockLayers;
    }
}