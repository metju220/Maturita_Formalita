using UnityEngine;

public class WorldData : MonoBehaviour
{
    public int worldWidth;
    public int worldHeight;

    public int[,] blockMap;

    public void Initialize()
    {

        blockMap = new int[worldWidth, worldHeight];
    }

    public void SetBlock(int x, int y, int blockID)
    {
        if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight)
        {
            blockMap[x, y] = blockID;
        }
    }

    public int GetBlock(int x, int y)
    {
        if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight)
        {
            return blockMap[x, y];
        }

        return 0;
    }
}