using UnityEngine;

public class BlockInteraction : MonoBehaviour
{
    [Header("References")]
    public WorldData worldData;
    public ChunkRenderer chunkRenderer;

    [Header("Mining Settings")]
    public float miningRadius = 4f; // Jak daleko od hráče můžu těžit

    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 1f, 1f, 0.8f); // Barva čtverečku pod myší

    private Camera cam;
    private Transform highlightTransform;
    private SpriteRenderer highlightRenderer;

    private void Start()
    {
        cam = Camera.main; // Najde hlavní kameru
        CreateHighlight(); // Vytvoří čtvereček pro zvýraznění bloku
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

        if (Input.GetMouseButtonDown(0))
        {
            TryBreakBlock();
        }
    }

    // Logika pro svícení čtverečku pod myší
    void UpdateHighlight()
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Zaokrouhlí pozici na celé bloky
        int blockX = Mathf.FloorToInt(mouseWorldPos.x);
        int blockY = Mathf.FloorToInt(mouseWorldPos.y);

        // Spočítá vzdálenost od hráče ke středu bloku
        float distance = Vector2.Distance(transform.position, new Vector2(blockX + 0.5f, blockY + 0.5f));

        // Zkontroluje, jestli jsem v dosahu a v mapě
        bool inRange = distance <= miningRadius && worldData.IsInBounds(blockX, blockY);
        // Zkontroluje, jestli na tom místě vůbec nějaký blok je
        bool hasBlock = inRange && !string.IsNullOrEmpty(worldData.GetBlock(blockX, blockY));

        if (hasBlock)
        {
            // Posune čtvereček na daný blok a ukáže ho
            highlightTransform.position = new Vector3(blockX + 0.5f, blockY + 0.5f, 0f);
            highlightTransform.gameObject.SetActive(true);
        }
        else
        {
            // Pokud tam nic není nebo jsem daleko, čtvereček schová
            highlightTransform.gameObject.SetActive(false);
        }
    }

    // Samotné zničení bloku
    void TryBreakBlock()
    {
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        int blockX = Mathf.FloorToInt(mouseWorldPos.x);
        int blockY = Mathf.FloorToInt(mouseWorldPos.y);

        // Kontroly: dosah, hranice mapy a jestli tam je co ničit
        float distance = Vector2.Distance(transform.position, new Vector2(blockX + 0.5f, blockY + 0.5f));
        if (distance > miningRadius) return;
        if (!worldData.IsInBounds(blockX, blockY)) return;

        string currentBlock = worldData.GetBlock(blockX, blockY);
        if (string.IsNullOrEmpty(currentBlock)) return;

        worldData.SetBlock(blockX, blockY, "");
        chunkRenderer.UpdateTile(blockX, blockY, "");
    }
}