using UnityEngine;

// Nastavení jeskyní - zobrazí se v Inspectoru u TerrainGeneration
[System.Serializable]
public class CaveSettings
{
    [Tooltip("Na kolik sloupců světa připadá jeden červ (menší číslo = víc jeskyní)")]
    public int columnsPerWorm = 70;

    [Tooltip("Délka jednoho červa v krocích")]
    public int minLength = 100;
    public int maxLength = 250;

    [Tooltip("Poloměr tunelu - mění se plynule podél červa mezi min a max")]
    public float minRadius = 1.5f;
    public float maxRadius = 3.5f;

    [Tooltip("Jak rychle se mění šířka tunelu (menší = delší úseky stejné šířky)")]
    public float radiusNoiseScale = 0.08f;

    [Tooltip("Maximální změna směru za jeden krok (v radiánech)")]
    public float turnStrength = 0.5f;

    [Tooltip("Jak moc se červ stáčí do vodorovného směru (0 = vůbec)")]
    public float horizontalBias = 0.3f;

    [Tooltip("Šance na rozdvojení tunelu v každém kroku")]
    public float branchChance = 0.012f;
    public int maxBranchDepth = 2;

    [Tooltip("Jeskyně začínají nejméně tolik bloků pod povrchem")]
    public int minDepthBelowSurface = 8;

    [Tooltip("Spodní řady světa, do kterých jeskyně nikdy nezasáhnou")]
    public int bottomMargin = 5;
}

public static class WormCaveGenerator
{
    const int edgeMargin = 5; // Červ se od levého/pravého/horního okraje mapy odrazí

    // Hlavní funkce, která vypustí všechny červy do světa
    public static void GenerateCaves(WorldData worldData, int seed, CaveSettings settings)
    {
        System.Random rng = new System.Random(seed); // Generátor náhody podle seedu
        int totalCleared = 0; // Počítadlo, kolik bloků celkem vykopeme

        // Počet červů roste s šířkou světa, aby byla hustota jeskyní pořád stejná
        int numWorms = Mathf.Max(1, worldData.worldWidth / settings.columnsPerWorm);

        for (int i = 0; i < numWorms; i++)
        {
            // Náhodný start na ose X (kousek od okrajů mapy)
            int startX = rng.Next(10, worldData.worldWidth - 10);

            // Start mezi dnem světa a hloubkou minDepthBelowSurface pod povrchem
            int surfaceY = worldData.heightMap[startX];
            int lowestPossible = settings.bottomMargin;
            int highestPossible = Mathf.Max(lowestPossible + 1, surfaceY - settings.minDepthBelowSurface);
            int startY = rng.Next(lowestPossible, highestPossible);

            int length = rng.Next(settings.minLength, settings.maxLength + 1);

            totalCleared += GenerateWorm(worldData, settings, startX, startY, length, rng, depth: 0);
        }

        Debug.Log($"[Caves] Worms: {numWorms} | Total blocks cleared: {totalCleared}");
    }

    // Logika plazení a kopání jednoho konkrétního červa
    static int GenerateWorm(WorldData worldData, CaveSettings settings, float x, float y, int length, System.Random rng, int depth)
    {
        // Náhodný počáteční úhel (směr), kterým se červ vydá
        float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);

        // Každý červ čte šum šířky z jiného místa, aby se šířka neměnila u všech stejně
        float noiseOffset = (float)(rng.NextDouble() * 10000.0);

        int clearedCount = 0;

        for (int step = 0; step < length; step++)
        {
            // Plynulé zatáčení: k úhlu přičteme náhodné číslo od -turnStrength do +turnStrength
            angle += ((float)rng.NextDouble() - 0.5f) * 2f * settings.turnStrength;

            // Stáčení k vodorovnému směru: sin(2 * úhel) je 0 pro vodorovný a svislý směr
            // a největší pro šikmý, takže šikmé tunely se pomalu narovnávají do vodorovna
            angle -= Mathf.Sin(2f * angle) * 0.5f * settings.horizontalBias;

            // Pozice je float, takže se červ hýbe libovolným směrem (ne jen 8 směry po mřížce)
            x += Mathf.Cos(angle);
            y += Mathf.Sin(angle);

            // Odraz od okrajů mapy (jako míček) místo klouzání podél okraje
            if (x < edgeMargin || x > worldData.worldWidth - edgeMargin)
            {
                angle = Mathf.PI - angle; // otočí vodorovný směr
                x = Mathf.Clamp(x, edgeMargin, worldData.worldWidth - edgeMargin);
            }
            if (y < settings.bottomMargin || y > worldData.worldHeight - edgeMargin)
            {
                angle = -angle; // otočí svislý směr
                y = Mathf.Clamp(y, settings.bottomMargin, worldData.worldHeight - edgeMargin);
            }

            // Šířka tunelu se plynule mění podle Perlin šumu -> úzké chodby a širší "místnosti"
            float noise = Mathf.PerlinNoise(noiseOffset + step * settings.radiusNoiseScale, 0f);
            float radius = Mathf.Lerp(settings.minRadius, settings.maxRadius, noise);

            clearedCount += CarveCircle(worldData, x, y, radius, settings.bottomMargin);

            // VĚTVENÍ: v každém kroku malá šance, že se červ rozdvojí
            if (depth < settings.maxBranchDepth && step > 10 && step < length - 10
                && rng.NextDouble() < settings.branchChance)
            {
                int branchLength = rng.Next(30, Mathf.Max(31, length / 2)); // Větev je nejvýš poloviční
                // Zavoláme tuhle stejnou funkci znovu (rekurze) z aktuální pozice
                clearedCount += GenerateWorm(worldData, settings, x, y, branchLength, rng, depth + 1);
            }
        }

        return clearedCount;
    }

    // Vykope kruh o daném poloměru kolem bodu (cx, cy), vrátí počet vykopaných bloků
    static int CarveCircle(WorldData worldData, float cx, float cy, float radius, int bottomMargin)
    {
        int centerX = Mathf.RoundToInt(cx);
        int centerY = Mathf.RoundToInt(cy);
        int r = Mathf.CeilToInt(radius);
        int cleared = 0;

        for (int dx = -r; dx <= r; dx++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                // Pythagorova věta: bod [dx, dy] leží v kruhu, když dx² + dy² <= r²
                if (dx * dx + dy * dy > radius * radius) continue;

                int blockX = centerX + dx;
                int blockY = centerY + dy;

                // Mimo mapu a spodní řady nekopeme
                if (!worldData.IsInBounds(blockX, blockY) || blockY < bottomMargin) continue;

                if (!worldData.IsAir(blockX, blockY))
                {
                    worldData.SetBlock(blockX, blockY, BlockRegistry.Air);
                    cleared++;
                }
            }
        }

        return cleared;
    }
}
