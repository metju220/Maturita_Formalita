using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkRenderer : MonoBehaviour
{
    // Odkazy na komponenty v Unity
    public Tilemap tilemap;
    public BlockRegistry blockRegistry; 

    // Vykreslení světa podle dat z WorldData
    public void DrawWorld(WorldData worldData)
    {
        // Nejdřív smaže všechno, co na mapě bylo
        tilemap.ClearAllTiles();

        // Projede celou mapu sloupec po sloupci, řádek po řádku
        for (int x = 0; x < worldData.worldWidth; x++)
        {
            for (int y = 0; y < worldData.worldHeight; y++)
            {
                // Zjistí textové ID bloku na těchto souřadnicích (např. "stone" nebo "")
                string blockID = worldData.GetBlock(x, y);

                // Pokud je tam prázdno (vzduch), přeskočí to a jde na další blok
                if (string.IsNullOrEmpty(blockID))
                    continue;

                // Podle textového ID najde v registrech skutečný Tile s grafikou
                TileBase tile = blockRegistry.GetTile(blockID);
                
                // Pokud grafika pro tohle ID neexistuje, vyhodí varování a přeskočí
                if (tile == null)
                {
                    Debug.LogWarning($"ChunkRenderer: Block '{blockID}' nemá tile v BlockRegistry!");
                    continue;
                }

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    // Přepíše jen jeden konkrétní blok (volá se za běhu hry při těžbě nebo stavění)
    public void UpdateTile(int x, int y, string blockID)
    {
        // Připraví si souřadnice v mřížce
        Vector3Int cellPos = new Vector3Int(x, y, 0);

        // Pokud je nové ID prázdné (blok byl vytěžen)
        if (string.IsNullOrEmpty(blockID))
        {
            // .smaže z této pozice Tile (přepíše ho na null = vzduch)
            tilemap.SetTile(cellPos, null);
            return;
        }

        // Jinak najde grafiku pro nový blok (třeba když hráč položí kostku)
        TileBase tile = blockRegistry.GetTile(blockID);
        if (tile == null)
        {
            Debug.LogWarning($"ChunkRenderer: Block '{blockID}' nemá tile v BlockRegistry!");
            return;
        }

        // Vykreslí nový blok na dané místo
        tilemap.SetTile(cellPos, tile);
    }
}