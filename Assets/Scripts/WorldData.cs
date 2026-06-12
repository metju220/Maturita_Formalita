using UnityEngine;

// WorldData jako MonoBehaviour je funkční, ale musíš ho mít na GameObject ve scéně
// a přiřadit referenci v TerrainGeneration Inspectoru.
public class WorldData : MonoBehaviour
{
    [Header("World Size")]
    public int worldWidth  = 200;
    public int worldHeight = 80;

    // Blokový grid - [0,0] je levý dolní roh
    // Hodnota 0 = vzduch/prázdno
    [HideInInspector]
    public string[,] blockMap;

    public void Initialize()
    {
        blockMap = new string [worldWidth, worldHeight];

        // Ujistíme se, že vše začíná jako vzduch (int default je 0, ale explicitně pro jistotu)
        for (int x = 0; x < worldWidth; x++)
            for (int y = 0; y < worldHeight; y++)
                blockMap[x, y] = "";
    }

    public void SetBlock(int x, int y, string blockID)
    {
        if (IsInBounds(x, y))
            blockMap[x, y] = blockID ?? ""; // Pokud blockID je null, nastavíme jako prázdný řetězec (vzduch)
    }

    public string GetBlock(int x, int y)
    {
        if (IsInBounds(x, y))
            return blockMap[x, y];

        return ""; // Mimo svět = vzduch
    }

    public bool IsAir(int x, int y)
    {
        return string.IsNullOrEmpty(GetBlock(x, y));
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < worldWidth && y >= 0 && y < worldHeight;
    }
}