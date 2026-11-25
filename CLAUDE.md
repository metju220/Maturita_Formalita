# Sandboxová 2D survival hra – maturitní projekt

Shrnutí stavu projektu, rozhodnutí a domluvených postupů. Slouží jako kontext pro další práci (i pro Claude Code).

## O projektu

- 2D sandbox survival hra s pohledem z boku ve stylu Terrarie (těžba, stavění, jeskyně, biomy).
- Unity **6000.4.6f1**, C#, Tilemap, URP 2D. Repozitář: `metju220/Maturita_Formalita`.
- Hlavní scéna: `Assets/Scenes/Game.unity`.
- Termín: ideálně **únor 2027**, nejpozději březen 2027.
- Závazný popis rozsahu a UI je v dokumentu *návrh řešení* (`Sandboxova-2D-survival-hra_navrh-reseni_Vrazel.docx`, mimo repozitář). Když se něco rozchází, platí dokument.

## Architektura (aktuální stav)

Data světa jsou oddělená od generování a od vykreslování.

| Soubor | Role |
|---|---|
| `WorldData.cs` | „paměť světa“: `ushort[,] blockMap` (0 = vzduch), `int[] heightMap`, `bedrockLayers`; `GetBlock/SetBlock/IsAir/IsInBounds/IsBreakable` |
| `BlockRegistry.cs` | ScriptableObject se seznamem bloků (textové ID + `TileBase`). Překládá text ↔ číslo: `GetNumber`, `GetName`, `GetTile(ushort)`, `Exists`, konstanta `Air = 0` |
| `TerrainGeneration.cs` | generování: seed → `Initialize` → `ResolveBlockNumbers` u biomů → `GenerateWorld` (výšková mapa, terén podle biomu, jeskyně, rudy) → `DrawWorld` → stěny na okrajích → spawn hráče. Náhledy biomů a rud v `OnValidate` (textury `HideFlags.DontSave`) |
| `BiomeClass.cs`, `OreClass.cs` | nastavení biomů a rud. Textová ID z Inspectoru + `[NonSerialized]` číselná dvojčata (`surfaceBlock`, `blockNumber`…) dopočítaná při startu. `ValidateBlockIDs` hlídá překlepy v ID |
| `WormCaveGenerator.cs` | statická třída, jeskyně metodou „červů“: `CaveSettings` (v Inspectoru u TerrainGeneration), počet červů podle šířky světa, float pozice, proměnlivá šířka podle Perlinova šumu, stáčení do vodorovna, odraz od okrajů, rekurzivní větvení |
| `ChunkRenderer.cs` | kreslí `WorldData` do Tilemapy (`DrawWorld`, `UpdateTile(x, y, ushort)`). Zatím bez skutečných chunků |
| `BlockInteraction.cs` | na hráči: levé tlačítko těží, pravé pokládá `placeBlockID`. Pravidla: dosah, bedrock, soused pro pokládání, kolize s hráčem (`Physics2D.OverlapBox`), **viditelnost** – úsečka z hlavy/středu/nohou nesmí projít jiným blokem |
| `PlayerControlls/PlayerController.cs`, `CamControl.cs` | pohyb, skok (trigger `Ground`), kamera omezená na mapu |

Důležité detaily:
- Seed je `int` (`randomSeed` v Inspectoru zapne náhodný svět, jinak jde svět zopakovat).
- Čísla bloků závisí na **pořadí v BlockRegistry** → při ukládání světa uložit i tabulku číslo → textové ID.
- Spodních `bedrockLayers` řad je nezničitelných; rudy ani jeskyně do nich nezasahují.
- Svět 2056 × 256, terén 160–192 (hodnoty ve scéně), dlaždice 32 × 32 px, Point filter, 32 PPU.
- Vstup: starý Input Manager (`Input.GetMouseButtonDown`…), v projektu zapnuté oba systémy.

## Historie hlavních změn

1. Recenze kódu a opravy: seed z Inspectoru, náhledové textury neuložené do scény, bedrock a stěny na okrajích, neexistující ID bloků, duplicitní `ChunkRenderer`, název souboru `ChunkRenderer.cs`.
2. Smazán mrtvý kód (TileAtlas, TileClass, nepoužité proměnné v BiomeClass a BlockRegistry).
3. Přechod ze `string` ID bloků na `ushort` čísla.
4. Nové jeskyně (`CaveSettings`), pokládání bloků, kontrola viditelnosti při těžbě/stavění.

## Plán (pořadí)

1. **Inventář + hotbar** ← další krok
2. `Health` komponenta (hráč i monstra) + životy a hlad v UI
3. První monstrum (slime) – stavový automat Patrol/Chase/Attack, spawner, útok zbraní
4. Struktury ve světě (textové šablony) + útržky deníku + deník
5. Cestovatel u spawnu + dialogy + řetěz úkolů (události přes `GameEvents`)
6. Ukládání do JSON (seed, změněné chunky, hráč, inventář, úkoly, tabulka ID bloků)
7. Další nepřátelé (hřib, zombie), boss Strážce hlubin (2. fáze pod 50 % HP), konec příběhu
- **Nedodělané biomy** (připomenout, kdykoli padne otázka „co dál“): výběr biomu podle prahů šumu místo barev gradientu, vlastní výška terénu pro biom s plynulým přechodem (Lerp), kontrola četnosti biomů, rozmazané hranice, oddělený offset šumu biomů a terénu, biom spočítat jednou na sloupec.

### Návrh inventáře (domluveno)
- Hotbar **10 slotů vlevo nahoře** (klávesy 1–9, 0 + kolečko), pod ním zdraví a hlad. Inventář **30 slotů** + slot batohu a rozšiřující řada.
- Obrazovka inventáře má 3 sloupce: postava (Brnění/Charmy) | inventář | crafting (suroviny „3 / 5“, tlačítko Vyrobit). Otevírá se `Tab` (`E` je pro mluvení s NPC).
- Stack 99 (bloky, suroviny), nástroje a zbraně 1. Přesouvání: klik vzít / klik položit, sloučit, prohodit.
- Vytěžený blok jde rovnou do inventáře (padající předměty až s monstry). Pokládá se vybraný blok z hotbaru.
- Kód: `ItemData` (ScriptableObject: jméno, ikona, maxStack, typ Block/Tool/Weapon/Material/Lore, blockID), `BlockDefinition.dropItem`, `ItemStack`, `Inventory` (`AddItem` ve dvou průchodech, `OnInventoryChanged`), `InventoryUI` + prefab `SlotUI` (uGUI). Logika oddělená od zobrazení stejně jako WorldData/ChunkRenderer.
- Grafika zatím provizorní; později vlastní pixel art v Aseprite (9-slice rámečky).
- Styl UI: tmavě fialový panel `#1F1A2E`, slot/tlačítko `#2D2643`, zlatý rám `#C9A86B`, okraj slotu `#7D6540`, text `#F3EAD6`, neaktivní `#464252`, srdce `#D94545`, hlad `#E39B3A`.

### Příběh (z návrhu řešení)
Zaniklá hornická osada; horníci v nejhlubší šachtě narazili na krystal Srdce hlubin hlídaný Strážcem hlubin. Kletba oživila mrtvé horníky (zombie), hřiby a slimy. U spawnu stojí cestovatel s károu, učí základy a postupně odhaluje příběh; útržky deníků jsou ve strukturách. Po porážce Strážce hra pokračuje jako volný sandbox.

## Jak se na projektu pracuje

- Autor kód obhajuje u maturity: změny vysvětlovat po částech (co dělá a proč), raději navést a zkontrolovat, než potichu přepsat.
- Bez výslovného pokynu **necommitovat ani nepushovat**; autor si změny kontroluje a nahrává sám.
- Komentáře v kódu česky, věcně; žádné komentáře ve stylu odpovědi z chatu („tady ti…“).
- Neupravovat `.unity` / `.asset` soubory, když běží Unity Editor (přepsal by je).
- Ověření kompilace bez Unity: zkopírovat `Assembly-CSharp.csproj` mimo projekt s absolutními cestami ke skriptům a spustit `dotnet build` (odkazy na DLL Unity jsou v csproj).
- Git: pokud push/fetch hlásí `SSL certificate problem`, pomůže `git config --global http.sslBackend schannel`.
