using UnityEngine;

[System.Serializable]
public class OreClass
{
    public string oreName;
    public float rarity;
    public float veinSize;
    public int maxHeightSpawn;
    public Texture2D spread;

    // Tato funkce vygeneruje náhled šumu do textury
    public void GeneratePreview(float seed, int index)
    {
        // Tady změníš 100, 100 na 128, 128
        if (spread == null || spread.width != 128 || spread.height != 128)
        {
            spread = new Texture2D(128, 128);
            // Volitelné: Nastavíme ostrost textury (Point je pro pixel-art nejlepší)
            spread.filterMode = FilterMode.Point;
        }

        for (int x = 0; x < spread.width; x++)
        {
            for (int y = 0; y < spread.height; y++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + seed + index * 1000) * rarity,
                    (y + seed + index * 1000) * rarity
                );

                Color color = noise > veinSize ? Color.white : Color.black;
                spread.SetPixel(x, y, color);
            }
        }
        spread.Apply();
    }
}