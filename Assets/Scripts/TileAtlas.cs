using UnityEngine;
using UnityEngine.Tilemaps;

// Pořadí tiles v ChunkRenderer.tiles[] musí být:
//   [0] = null (prázdno/vzduch - NEZAPLŇUJ)
//   [1] = grass
//   [2] = dirt
//   [3] = stone
//   [4] = první ore (coal)
//   [5] = druhý ore (iron)
//   atd.

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    public TileBase grass;
    public TileBase dirt;
    public TileBase snowDirt;
    public TileBase sand;
    public TileBase stone;

    [Header("Ores")]
    public TileBase coal;
    public TileBase iron;
    public TileBase gold;
}