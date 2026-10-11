using UnityEngine;

[System.Serializable]
public class OreClass
{
    public string oreName;

    // Block ID klíč - musí existovat v BlockRegistry
    // Např. "Coal_ore", "Iron_ore", "Gold_ore"
    [Tooltip("Block ID z BlockRegistry")]
    public string blockID = "Coal_ore";

    // Číslo bloku z BlockRegistry - dopočítá se při startu (BiomeClass.ResolveBlockNumbers)
    [System.NonSerialized] public ushort blockNumber;

    // rarity: frekvence šumu - NÍZKÁ hodnota = větší, roztahanější žíly
    //                          VYSOKÁ hodnota = menší, hustěji rozmístěné žíly
    [Range(0.01f, 0.5f)]
    public float rarity = 0.05f;

    // veinSize: práh detekce - NÍZKÁ hodnota = více rudy
    //                          VYSOKÁ hodnota (blíž k 1.0) = méně rudy
    [Range(0.5f, 0.99f)]
    public float veinSize = 0.80f;

    public int maxHeightSpawn = 20;

    public Texture2D spread;

    // preview texturu
    public void GeneratePreview(float seed, int index)
    {
        const int texSize = 128;

        if (spread == null
            || spread.hideFlags != HideFlags.DontSave
            || spread.width  != texSize
            || spread.height != texSize)
        {
            spread = new Texture2D(texSize, texSize);
            spread.filterMode = FilterMode.Point;
            spread.hideFlags = HideFlags.DontSave;
        }

        for (int x = 0; x < texSize; x++)
        {
            for (int y = 0; y < texSize; y++)
            {
                // Každá ruda má vlastní offset aby se nepřekrývaly
                float noise = Mathf.PerlinNoise(
                    (x + seed + index * 1000f) * rarity,
                    (y + seed + index * 1000f) * rarity
                );

                Color color = noise > veinSize ? Color.white : Color.black;
                spread.SetPixel(x, y, color);
            }
        }

        spread.Apply();
    }
}