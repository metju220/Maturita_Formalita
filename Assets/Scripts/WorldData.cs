using UnityEngine;
using UnityEngine.Serialization;

public class WorldData : MonoBehaviour
{
    [Header("World Size")]
    public int worldWidth  = 200; // Šířka světa
    public int worldHeight = 80;  // Výška světa

    [HideInInspector]
    public string[,] blockMap; // 2D mřížka (X, Y) naplněná textovými ID bloků

    [HideInInspector]
    public int[] heightMap; // 1D pole, které si pamatuje nejvyšší bod povrchu pro každý sloupec X

    // Vytvoří prázdný svět (připraví pole v paměti a zaplní je "vzduchem")
    public void Initialize()
    {
        blockMap = new string[worldWidth, worldHeight];
        heightMap = new int[worldWidth];

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
                blockMap[x, y] = ""; // "" znamená vzduch
            
            heightMap[x] = 0;
        }
    }

    // Bezpečně zapíše blok na zadané souřadnice
    public void SetBlock(int x, int y, string blockID)
    {
        if (IsInBounds(x, y))
            blockMap[x, y] = blockID ?? ""; // Pokud by někdo poslal null, uloží se ""
    }

    // Bezpečně vrátí ID bloku na zadaných souřadnicích
    public string GetBlock(int x, int y)
    {
        if (IsInBounds(x, y))
            return blockMap[x, y];
        return ""; // Pokud je mimo mapu, považujeme to za vzduch
    }

    // Rychlá kontrola, jestli je na dané pozici vzduch
    public bool IsAir(int x, int y)
    {
        return string.IsNullOrEmpty(GetBlock(x, y));
    }

    // Hlídač, který kontroluje, jestli zadané X a Y vůbec leží uvnitř mapy
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
    }
}