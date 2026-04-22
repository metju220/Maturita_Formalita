using UnityEngine;

[CreateAssetMenu(fileName = "TileClass", menuName = "TileClass")]

public class TileClass : ScriptableObject
{
    public string tileName;
    public Sprite[] tileSprite;
}
