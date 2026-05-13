using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkRenderer : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase[] tiles;

    public void DrawWorld(WorldData worldData)
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < worldData.worldWidth; x++)
        {
            for (int y = 0; y < worldData.worldHeight; y++)
            {
                int id = worldData.GetBlock(x, y);

                if (id > 0 && id < tiles.Length)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[id]);
                }
            }
        }
    }
}