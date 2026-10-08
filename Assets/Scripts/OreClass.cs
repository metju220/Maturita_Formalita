using UnityEngine;

[System.Serializable]
public class OreClass
{
    public string oreName;

    // Block ID klíč - musí existovat v BlockRegistry
    // Např. "Coal_ore", "Iron_ore", "Gold_ore"
    [Tooltip("Block ID z BlockRegistry")]
    public string blockID = "Coal_ore";

    // rarity: frekvence šumu - NÍZKÁ hodnota = větší, roztahanější žíly
    //                          VYSOKÁ hodnota = menší, hustěji rozmístěné žíly
    [Range(0.01f, 0.5f)]
    public float rarity = 0.05f;

    // veinSize: práh detekce - NÍZKÁ hodnota = více rudy
    //                          VYSOKÁ hodnota (blíž k 1.0) = méně rudy
    [Range(0.5f, 0.99f)]
    public float veinSize = 0.80f;

    // Ruda se negeneruje nad touto výškou (y >= maxHeightSpawn = žádná ruda)
    public int maxHeightSpawn = 20;

    public Texture2D spread;

    // Vygeneruje preview texturu do Inspectoru
    // Bílá pixely = kde se bude ruda generovat
    public void GeneratePreview(float seed, int index)
    {
        const int texSize = 128;

        // Textura je jen náhled - DontSave zajistí, že se neuloží do scény
        // (při každém otevření se vygeneruje znovu v OnValidate)
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
                // Každá ruda má vlastní offset (index * 1000) aby se nepřekrývaly
                float noise = Mathf.PerlinNoise(
                    (x + seed + index * 1000f) * rarity,
                    (y + seed + index * 1000f) * rarity
                );

                // Bílá = ruda přítomna (noise > veinSize)
                Color color = noise > veinSize ? Color.white : Color.black;
                spread.SetPixel(x, y, color);
            }
        }

        spread.Apply();
    }
}