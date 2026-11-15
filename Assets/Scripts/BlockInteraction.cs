using System.Collections.Generic;
using UnityEngine;

public class BlockInteraction : MonoBehaviour
{
    [Header("References")]
    public WorldData worldData;
    public ChunkRenderer chunkRenderer;
    public BlockRegistry blockRegistry;

    [Header("Mining Settings")]
    public float miningRadius = 5f; // Jak daleko od hráče můžu těžit a stavět

    [Header("Placing Settings")]
    [Tooltip("ID bloku z BlockRegistry, který se pokládá pravým tlačítkem myši")]
    public string placeBlockID = "Stone_block";

    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 1f, 1f, 0.8f); // Barva čtverečku pod myší

    private Camera cam;
    private Transform highlightTransform;
    private SpriteRenderer highlightRenderer;

    private ushort placeBlock; // Číslo pokládaného bloku (přeloží se z placeBlockID ve Start)
    private readonly List<Collider2D> overlapResults = new List<Collider2D>(); // Pro kontrolu kolize při pokládání

    private Collider2D bodyCollider; // Pevný collider hráče - podle něj víme, kde má hlavu a nohy
    private readonly Vector2[] viewOrigins = new Vector2[3]; // Body, ze kterých hráč "kouká" na bloky

    private void Start()
    {
        cam = Camera.main; // Najde hlavní kameru
        CreateHighlight(); // Vytvoří čtvereček pro zvýraznění bloku

        // Textové ID z Inspectoru převedeme jednou na číslo bloku
        placeBlock = blockRegistry.GetNumber(placeBlockID);

        bodyCollider = FindBodyCollider();
    }

    // Najde na hráči pevný collider (trigger pod nohama pro detekci země přeskočí)
    Collider2D FindBodyCollider()
    {
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            if (!col.isTrigger) return col;
        }
        return null;
    }

    // Vygeneruje za běhu hry čtvereček, který bude zvýrazňovat bloky
    void CreateHighlight()
    {
        GameObject highlightObj = new GameObject("BlockHighlight");
        highlightTransform = highlightObj.transform;

        highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
        highlightRenderer.sprite = CreateOutlineSprite(); // obrys pro tile
        highlightRenderer.color = highlightColor;
        highlightRenderer.sortingOrder = 10; // Aby byl vidět nad bloky

        highlightObj.SetActive(false);
    }

    // Kód pro vygenerování textury obrysu (čtverečku) pixel po pixelu
    Sprite CreateOutlineSprite()
    {
        int size = 32;
        int thickness = 2; // Tloušťka čáry obrysu

        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point; // Ostré pixely

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isEdge = x < thickness || x >= size - thickness ||
                              y < thickness || y >= size - thickness;

                tex.SetPixel(x, y, isEdge ? Color.white : new Color(0, 0, 0, 0));
            }
        }
        tex.Apply(); // Uloží změny do textury

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void Update()
    {
        UpdateHighlight();

        if (Input.GetMouseButtonDown(0))      // Levé tlačítko = těžení
        {
            TryBreakBlock();
        }
        else if (Input.GetMouseButtonDown(1)) // Pravé tlačítko = pokládání
        {
            TryPlaceBlock();
        }
    }

    // Logika pro svícení čtverečku pod myší
    void UpdateHighlight()
    {
        Vector2Int cell = GetMouseCell();

        // Čtvereček ukážeme, když jde blok pod myší vytěžit, nebo když tam jde blok položit
        if (CanBreak(cell) || CanPlace(cell))
        {
            // Posune čtvereček na daný blok a ukáže ho
            highlightTransform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
            highlightTransform.gameObject.SetActive(true);
        }
        else
        {
            // Pokud tu nejde nic udělat (nebo jsem daleko), čtvereček schová
            highlightTransform.gameObject.SetActive(false);
        }
    }

    // Samotné zničení bloku
    void TryBreakBlock()
    {
        Vector2Int cell = GetMouseCell();
        if (!CanBreak(cell)) return;

        worldData.SetBlock(cell.x, cell.y, BlockRegistry.Air);
        chunkRenderer.UpdateTile(cell.x, cell.y, BlockRegistry.Air);
    }

    // Položení bloku na prázdné místo pod myší
    void TryPlaceBlock()
    {
        if (placeBlock == BlockRegistry.Air) return; // Špatné placeBlockID v Inspectoru

        Vector2Int cell = GetMouseCell();
        if (!CanPlace(cell)) return;

        worldData.SetBlock(cell.x, cell.y, placeBlock);
        chunkRenderer.UpdateTile(cell.x, cell.y, placeBlock);
    }

    // Převede pozici myši na obrazovce na souřadnice bloku v mřížce světa
    Vector2Int GetMouseCell()
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Zaokrouhlí pozici dolů na celé bloky
        return new Vector2Int(Mathf.FloorToInt(mouseWorldPos.x), Mathf.FloorToInt(mouseWorldPos.y));
    }

    // Je blok v dosahu hráče? (měří se vzdálenost ke středu bloku)
    bool IsInReach(Vector2Int cell)
    {
        Vector2 cellCenter = new Vector2(cell.x + 0.5f, cell.y + 0.5f);
        return Vector2.Distance(transform.position, cellCenter) <= miningRadius;
    }

    // Jde blok vytěžit? V dosahu, v mapě, není to bedrock, je tam nějaký blok a hráč na něj vidí
    bool CanBreak(Vector2Int cell)
    {
        return IsInReach(cell)
            && worldData.IsBreakable(cell.x, cell.y)
            && !worldData.IsAir(cell.x, cell.y)
            && HasLineOfSight(cell);
    }

    // Jde sem položit blok?
    bool CanPlace(Vector2Int cell)
    {
        if (!IsInReach(cell)) return false;
        if (!worldData.IsInBounds(cell.x, cell.y)) return false;
        if (!worldData.IsAir(cell.x, cell.y)) return false; // Místo musí být volné
        if (!HasSolidNeighbour(cell)) return false;         // Blok nesmí viset ve vzduchu
        if (!HasLineOfSight(cell)) return false;            // Nesmí se stavět skrz jiné bloky
        if (IsOccupiedByCollider(cell)) return false;       // Blok se nesmí položit do hráče
        return true;
    }

    // Vidí hráč na blok? Stačí, když je volná cesta aspoň z jednoho bodu na těle (hlava, střed, nohy)
    bool HasLineOfSight(Vector2Int cell)
    {
        Vector2 target = new Vector2(cell.x + 0.5f, cell.y + 0.5f); // Střed cílového bloku

        foreach (Vector2 origin in GetViewOrigins())
        {
            if (IsPathClear(origin, target, cell)) return true;
        }
        return false;
    }

    // Tři body na těle hráče, ze kterých "kouká": hlava, střed těla a nohy
    Vector2[] GetViewOrigins()
    {
        if (bodyCollider == null)
        {
            // Pojistka, kdyby hráč neměl pevný collider - koukáme jen ze středu objektu
            viewOrigins[0] = viewOrigins[1] = viewOrigins[2] = transform.position;
            return viewOrigins;
        }

        Bounds body = bodyCollider.bounds;
        viewOrigins[0] = new Vector2(body.center.x, body.max.y - 0.1f); // Hlava (kousek pod temenem)
        viewOrigins[1] = body.center;                                   // Střed těla
        viewOrigins[2] = new Vector2(body.center.x, body.min.y + 0.1f); // Nohy (kousek nad chodidly)
        return viewOrigins;
    }

    // Jde po úsečce od origin k target po malých krocích a hlídá, jestli cestou nestojí pevný blok
    bool IsPathClear(Vector2 origin, Vector2 target, Vector2Int targetCell)
    {
        const float stepSize = 0.05f; // Délka jednoho kroku - menší = přesnější, ale víc kontrol
        int steps = Mathf.CeilToInt(Vector2.Distance(origin, target) / stepSize);

        for (int i = 1; i < steps; i++)
        {
            // Bod na úsečce: Lerp(origin, target, 0) = origin, Lerp(..., 1) = target
            Vector2 point = Vector2.Lerp(origin, target, (float)i / steps);
            int x = Mathf.FloorToInt(point.x);
            int y = Mathf.FloorToInt(point.y);

            if (x == targetCell.x && y == targetCell.y) return true; // Došli jsme až k cílovému bloku
            if (!worldData.IsAir(x, y)) return false;                // Cestou stojí jiný blok
        }
        return true;
    }

    // Má místo aspoň jednoho souseda (vlevo, vpravo, nahoře, dole), který je pevný blok?
    bool HasSolidNeighbour(Vector2Int cell)
    {
        return !worldData.IsAir(cell.x - 1, cell.y)
            || !worldData.IsAir(cell.x + 1, cell.y)
            || !worldData.IsAir(cell.x, cell.y - 1)
            || !worldData.IsAir(cell.x, cell.y + 1);
    }

    // Stojí v tomhle políčku něco s fyzickým colliderem (hráč)?
    bool IsOccupiedByCollider(Vector2Int cell)
    {
        Vector2 cellCenter = new Vector2(cell.x + 0.5f, cell.y + 0.5f);

        // Triggery ignorujeme (např. senzor země pod nohama hráče), zajímají nás jen pevné collidery
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;

        // Box 0.9 x 0.9 je o kousek menší než blok, aby se nedotkl sousedních bloků
        return Physics2D.OverlapBox(cellCenter, new Vector2(0.9f, 0.9f), 0f, filter, overlapResults) > 0;
    }
}