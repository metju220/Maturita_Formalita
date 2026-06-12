using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkRenderer : MonoBehaviour
{
    public Tilemap tilemap;
    public BlockRegistry blockRegistry;

    public void DrawWorld(WorldData worldData)
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < worldData.worldWidth; x++)
        {
            for (int y = 0; y < worldData.worldHeight; y++)
            {
                string blockID = worldData.GetBlock(x, y);

                if (string.IsNullOrEmpty(blockID))
                    continue;

                TileBase tile = blockRegistry.GetTile(blockID);
                if (tile == null)
                {
                    Debug.LogWarning($"ChunkRenderer: Block '{blockID}' nemá tile v BlockRegistry!");
                    continue;
                }

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}