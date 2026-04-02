using UnityEditor;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject CellPrefab;
    public GameObject PiecePrefab;
    private LevelDataSO currentLevelData;
    private LevelDataSO Level => currentLevelData;
    private GameObject[,] cellObjs;
    private SpriteRenderer[,] cellFill;
    private Vector3 boardOrigin;

    private const float CELL = 1.0f;
    private const float GAP = 0.01f;
    private const float CLUE_OFFSET = 1.3f;

    private float Step => CELL + GAP;

    private GameObject cellBG;
    private void Awake()
    {
        // CellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/CellPrefab.prefab");

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadLevel(LevelDataSO levelData)
    {
        currentLevelData = levelData;
        boardOrigin = new Vector3(-Step * Level.cols * 0.5f, Step * Level.rows * 0.5f, 0);
        BuildBoard();
        BuildPiece();
    }
    private void BuildBoard()
    {
        var L = Level;
        cellObjs = new GameObject[L.rows, L.cols];
        cellFill = new SpriteRenderer[L.rows, L.cols];
        for (int r = 0; r < L.rows; r++)
        {
            for (int c = 0; c < L.cols; c++)
            {
                // var bg = new GameObject($"Cell_{r}_{c}");
                // bg.transform.position   = CellCenter(r, c);
                // bg.transform.localScale = Vector3.one * CELL;

                cellBG = Instantiate(CellPrefab, this.transform);
                cellBG.transform.position   = CellCenter(r, c);
                // cellBg.transform.localScale = Vector3.one * CELL;

                // var bgSr = bg.AddComponent<SpriteRenderer>();
                // bgSr.sprite       = MakeRoundedSprite();
                // bgSr.color        = new Color(0.22f, 0.28f, 0.42f);
                // bgSr.sortingOrder = 0;

                // var fill = new GameObject("Fill");
                // fill.transform.SetParent(bg.transform, false);
                // fill.transform.localPosition = new Vector3(0, 0, -0.01f);
                // fill.transform.localScale    = Vector3.one * 0.88f;

                // var fillSr = fill.AddComponent<SpriteRenderer>();
                // fillSr.sprite       = MakeRoundedSprite();
                // fillSr.color        = Color.clear;
                // fillSr.sortingOrder = 1;

                cellObjs[r, c] = cellBG;
                // cellFill[r, c] = fillSr;
            }
        }
    }
    private void BuildPiece()
    {
        var L = Level;
        if (L.pieces == null || L.pieces.Length == 0) return;
 
        float boardW = L.cols * Step - GAP;
        float x      = -boardW / 2f;
        float y      = boardOrigin.y - L.rows * Step - 0.8f;
        float rowH   = 0;
 
        for (int i = 0; i < L.pieces.Length; i++)
        {
            var shape = L.pieces[i];
 
            int minCX = int.MaxValue, maxCX = int.MinValue;
            int minCY = int.MaxValue, maxCY = int.MinValue;
            foreach (var cell in shape.cells)
            {
                minCX = Mathf.Min(minCX, cell.x); maxCX = Mathf.Max(maxCX, cell.x);
                minCY = Mathf.Min(minCY, cell.y); maxCY = Mathf.Max(maxCY, cell.y);
            }
 
            float pw = (maxCX - minCX + 1) * Step;
            float ph = (maxCY - minCY + 1) * Step;
 
            if (i > 0 && x + pw > boardW / 2f + Step)
            {
                x    = -boardW / 2f;
                y   -= rowH + 0.15f;
                rowH = 0;
            }
 
            float homeX = x + (pw * 0.5f) - (minCX + maxCX) * 0.5f * Step;
            float homeY = y - (ph * 0.5f) + (minCY + maxCY) * 0.5f * Step;
 
            CreatePiece(shape, new Vector3(homeX, homeY, 0));
 
            x    += pw + 0.15f;
            rowH  = Mathf.Max(rowH, ph);
        }
    }
 
    private void CreatePiece(PieceShapeData shape, Vector3 worldPos)
    {
        var sourcePrefab = shape.prefab != null ? shape.prefab : PiecePrefab;
 
        var root = Instantiate(sourcePrefab, worldPos, Quaternion.identity);
        root.name = $"Piece_{shape.pieceName}";
 
        var rb = root.GetComponent<Rigidbody2D>();
        if (rb == null) rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
 
        var pi = new PieceInstance
        {
            shape        = shape,
            color        = shape.color,
            root         = root,
            rotState     = 0,
            isPlaced     = false,
            placedRow    = -1,
            placedCol    = -1,
            currentCells = (Vector2Int[])shape.cells.Clone(),
            homePos      = worldPos
        };
 
        RebuildPieceVisuals(pi);
    }
    private void RebuildPieceVisuals(PieceInstance pi)
    {
        // Xoá các ô con đã spawn trước đó (chỉ xoá GO tạo bằng code, không xoá prefab gốc)
        foreach (var v in pi.visuals)   if (v) Destroy(v.gameObject);
        pi.visuals.Clear();
        foreach (var c in pi.colliders) if (c) Destroy(c);
        pi.colliders.Clear();
        float spacingMultiplier = 0.45f;
        foreach (var offset in pi.currentCells)
        {
            if (!pi.usePrefab)
            {
                // ── Spawn PiecePrefab (1 ô) cho từng cell của shape ──────────
                var cell = Instantiate(PiecePrefab, pi.root.transform);
                cell.transform.localPosition = new Vector3(
                    offset.x * Step * spacingMultiplier, 
            -offset.y * Step * spacingMultiplier,0);
            // cell.transform.localPosition = new Vector3(0.001f, 0.001f, 0);
                // Thu thập SR và Collider có sẵn trong prefab
                var sr = cell.GetComponent<SpriteRenderer>();
                if (sr != null) pi.visuals.Add(sr);

                var box = cell.GetComponent<BoxCollider2D>();
                if (box != null) pi.colliders.Add(box);
            }
            else
            {
                // ── shape.prefab mode: collider add trên root ────────────────
                var box    = pi.root.AddComponent<BoxCollider2D>();
                box.offset = new Vector2(offset.x * Step, -offset.y * Step);
                box.size   = new Vector2(CELL * 0.92f, CELL * 0.92f);
                pi.colliders.Add(box);
            }
        }

        // shape.prefab mode: thu thập SR từ prefab để dùng preview/sort
        if (pi.usePrefab)
        {
            foreach (var sr in pi.root.GetComponentsInChildren<SpriteRenderer>())
                pi.visuals.Add(sr);
        }
    }
 
    // private void BuildClue()
    // {
    //     var L = Level;
    //     for (int r = 0; r < L.rows; r++)
    //     {
    //         var pos = CellCenter(r, 0) + Vector3.left * CLUE_OFFSET;
    //         var go  = MakeClueBadge(pos, L.rowCounts[r],
    //             L.colorMode && L.rowColors != null ? L.rowColors[r] : Color.white);
    //         // rowClueBGs[r]    = go.GetComponent<SpriteRenderer>();
    //         // rowClueLabels[r] = go.GetComponentInChildren<TextMeshPro>();
    //     }

    //     for (int c = 0; c < L.cols; c++)
    //     {
    //         var pos = CellCenter(0, c) + Vector3.up * CLUE_OFFSET;
    //         var go  = MakeClueBadge(pos, L.colCounts[c],
    //             L.colorMode && L.colColors != null ? L.colColors[c] : Color.white);
    //         // colClueBGs[c]    = go.GetComponent<SpriteRenderer>();
    //         // colClueLabels[c] = go.GetComponentInChildren<TextMeshPro>();
    //     }
    // }

    // private GameObject MakeClueBadge(Vector3 pos, int count, Color color)
    // {
    //     var go = new GameObject("Clue");
    //     go.transform.position = pos;

    //     // cellBG.sprite       = MakeRoundedSprite();
    //     var ClueBg = cellBG.GetComponent<SpriteRenderer>();
    //     ClueBg.color        = color * new Color(1f, 1f, 1f, 0.75f);
    //     ClueBg.sortingOrder = 2;
    
    //     // var bgPrefab = go.AddComponent<SpriteRenderer>();
    //     // bg.sprite       = MakeRoundedSprite();
    //     // bg.color        = color * new Color(1f, 1f, 1f, 0.75f);
    //     // bg.sortingOrder = 2;

    //     // var label = MakeTMP(go, "Label", Vector2.zero, Vector2.one * 0.8f);
    //     // label.text      = count.ToString();
    //     // label.fontSize  = 48;
    //     // label.alignment = TextAlignmentOptions.Center;
    //     // label.color     = Color.white;

    //     return go;
    // }
    private Vector3 CellCenter(int r, int c)
    {
        return boardOrigin + new Vector3(c * Step + CELL * 0.5f, 
                                            -(r * Step + CELL * 0.5f));
    }
    static Sprite _roundedSprite;
    static Sprite MakeRoundedSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;
        var tex    = new Texture2D(64, 64);
        var pixels = new Color[64 * 64];
        const float R = 10f;
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            float cx = Mathf.Min(x, 63 - x), cy = Mathf.Min(y, 63 - y);
            bool inside = cx >= R || cy >= R || (cx-R)*(cx-R)+(cy-R)*(cy-R) <= R*R;
            pixels[y * 64 + x] = inside ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels); tex.Apply(); tex.filterMode = FilterMode.Bilinear;
        _roundedSprite = Sprite.Create(tex, new Rect(0,0,64,64), new Vector2(0.5f,0.5f), 64f);
        return _roundedSprite;
    }
}
