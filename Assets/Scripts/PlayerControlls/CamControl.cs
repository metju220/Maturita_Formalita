using UnityEngine;

public class CamControl : MonoBehaviour
{
    [Range(0, 1)]
    [Tooltip("Rychlost dotahování kamery za hráčem. 0 = pomalé/plynulé, 1 = okamžité přilepení.")]
    public float smoothTime;

    [Tooltip("Sem v Unity přetáhni objekt hráče, kterého má kamera sledovat.")]
    public Transform playerTransform;

    [Header("Map Bounds")]
    [Tooltip("Odkaz na datový soubor/skript, kde je uložena šířka a výška mapy.")]
    public WorldData worldData;
    
    [Tooltip("Offset pro případ, že mapa nezačíná na souřadnicích (0,0)")]
    public Vector2 mapOriginOffset = Vector2.zero;

    // Privátní proměnná pro uložení komponenty kamery tohoto objektu
    private Camera cam;

    private void Start()
    {
        // Při spuštění hry si automaticky načteme komponentu Camera
        cam = GetComponent<Camera>();
    }

    public void FixedUpdate()
    {
        // 1. KROK: Vezmeme aktuální pozici kamery
        Vector3 pos = transform.position;

        // 2. KROK: Přiblížení a plynulý pohyb kamery za hráčem
        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, playerTransform.position.y, smoothTime);

        // 3. KROK: Omezení pohybu kamery, aby nepřejížděla okraje mapy
        if (worldData != null && cam != null)
        {
            // Zjistíme polovinu výšky a šířky výhledu kamery
            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = camHalfHeight * cam.aspect;

            // Vypočet minimální a maximální možné pozice kamery pro osu X
            float minX = mapOriginOffset.x + camHalfWidth;
            float maxX = mapOriginOffset.x + worldData.worldWidth - camHalfWidth;

            // Vypočet minimální a maximální možné pozice kamery pro osu Y
            float minY = mapOriginOffset.y + camHalfHeight;
            float maxY = mapOriginOffset.y + worldData.worldHeight - camHalfHeight;

            // --- OŠETŘENÍ OSY X ---
            // Pokud je mapa užší než samotný výhled kamery
            if (minX > maxX)
            {
                // zamkneme kameru přesně na střed šířky mapy
                float centerX = mapOriginOffset.x + worldData.worldWidth / 2f;
                pos.x = centerX;
            }
            else
            {
                // jinak ořízneme pozici kamery, aby nepřekročila min/max hranice
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
            }

            // --- OŠETŘENÍ OSY Y ---
            // Pokud je mapa nižší než samotný výhled kamery
            if (minY > maxY)
            {
                // zamkneme kameru přesně na střed výšky mapy
                float centerY = mapOriginOffset.y + worldData.worldHeight / 2f;
                pos.y = centerY;
            }
            else
            {
                // jinak ořízneme pozici kamery, aby nepřekročila min/max hranice
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
            }
        }

        // 4. KROK: Použití nově vypočítané (a oříznutou) pozice na kameru
        transform.position = pos;
    }
}