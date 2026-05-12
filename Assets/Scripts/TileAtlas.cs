using UnityEngine;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    public TileClass grass;
    public TileClass dirt;
    public TileClass snowDirt;
    public TileClass sand;
    public TileClass stone;

    [Header("Ores")]
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
}
