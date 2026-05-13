using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
[Header("Environment")]
    public TileBase grass; // Změněno z TileClass na TileBase
    public TileBase dirt;
    public TileBase snowDirt;
    public TileBase sand;
    public TileBase stone;

    [Header("Ores")]
    public TileBase coal;
    public TileBase iron;
    public TileBase gold;
}
