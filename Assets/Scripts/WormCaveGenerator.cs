using System.Collections.Generic;
using UnityEngine;

public class WormCaveGenerator
{
    // Hlavní funkce, která vypustí všechny červy do světa
    public static void GenerateCaves(WorldData worldData, float seed, int numWorms = 10, int wormLength = 120)
    {
        System.Random rng = new System.Random((int)seed); // Generátor náhody podle seedu
        int totalCleared = 0; // Počítadlo, kolik bloků celkem vykopeme

        // Smyčka, která vytvoří zadaný počet červů (základních tunelů)
        for (int i = 0; i < numWorms; i++)
        {
            // Náhodný start na ose X (kousek od okrajů mapy)
            int startX = rng.Next(10, worldData.worldWidth - 10);

            // Výpočet bezpečné hloubky, aby jeskyně nezačínaly hned na trávě nebo nepadaly pod dno
            int surfaceY = worldData.heightMap[startX];
            int lowestPossible = 5;
            int highestPossible = Mathf.Max(lowestPossible + 1, surfaceY - 8); // Max 8 bloků pod povrchem

            int startY = rng.Next(lowestPossible, highestPossible);

            // Vypustíme červa a přičteme počet vykopaných bloků
            int cleared = GenerateWorm(worldData, startX, startY, wormLength, rng, depth: 0);
            totalCleared += cleared;
        }

        Debug.Log($"[Caves] Total worms (incl. branches): started {numWorms} | Total blocks cleared: {totalCleared}");
    }

    // Logika plazení a kopání jednoho konkrétního červa
    static int GenerateWorm(WorldData worldData, int startX, int startY, int length, System.Random rng, int depth)
    {
        const int maxBranchDepth = 2; // Stopka pro větvení, aby se z větví nedělaly další stovky větví

        int x = startX;
        int y = startY;

        int tunnelRadius = rng.Next(2, 4); // Náhodná tloušťka tunelu (šířka jeskyně)

        // Náhodný počáteční úhel (směr), kterým se červ vydá
        float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
        int clearedCount = 0;

        // Smyčka pro každý krok délky červa
        for (int step = 0; step < length; step++)
        {
            // Plynulé zatáčení: k aktuálnímu úhlu přičteme/odečteme malé náhodné číslo
            angle += ((float)rng.NextDouble() - 0.5f) * 1.0f;

            // Přepočet úhlu na posun (osa X a Y)
            int dirX = Mathf.RoundToInt(Mathf.Cos(angle) * 1.5f);
            int dirY = Mathf.RoundToInt(Mathf.Sin(angle) * 1.5f);

            x += dirX;
            y += dirY;

            // Nepustí červa mimo mapu
            x = Mathf.Clamp(x, 5, worldData.worldWidth - 5);
            y = Mathf.Clamp(y, 5, worldData.worldHeight - 5);

            // VYKOPOVÁNÍ KRUHU: Červ kolem sebe vymaže bloky v zadaném radiusu
            for (int dx = -tunnelRadius; dx <= tunnelRadius; dx++)
            {
                for (int dy = -tunnelRadius; dy <= tunnelRadius; dy++)
                {
                    // Matematický trik: kontrola, jestli bod [dx, dy] leží uvnitř kruhu
                    if (dx * dx + dy * dy <= tunnelRadius * tunnelRadius)
                    {
                        int checkX = x + dx;
                        int checkY = y + dy;

                        // Pokud jsme v mapě a nekopeme úplné dno
                        if (worldData.IsInBounds(checkX, checkY) && checkY >= 5)
                        {
                            // Pokud tam byl pevný blok, přičteme ho do statistiky a smažeme ho
                            if (!worldData.IsAir(checkX, checkY))
                                clearedCount++;
                            
                            worldData.SetBlock(checkX, checkY, "");
                        }
                    }
                }
            }

            // VĚTVENÍ: cca 1.2% šance v každém kroku, že se červ rozdvojí
            if (depth < maxBranchDepth && step > 10 && step < length - 10 && rng.NextDouble() < 0.012)
            {
                int branchLength = rng.Next(40, 90); // Nová chodba bude o něco kratší
                // Zavoláme tuhle stejnou funkci znovu (rekurze) z aktuální pozice
                clearedCount += GenerateWorm(worldData, x, y, branchLength, rng, depth + 1);
            }
        }

        return clearedCount;
    }
}