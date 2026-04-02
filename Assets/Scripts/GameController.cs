/*  ============================================================
    BLOCK PUZZLE GAME  —  GameController.cs  (fixed)
    ============================================================ */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // ── Runtime state ────────────────────────────────────────────

    LevelDataSO currentLevelData;
    LevelDataSO Level => currentLevelData;

    // Alias để dùng chung với code cũ (PieceInstance dùng PieceShapeData)
    // PieceShapeData thay cho PieceShape nội bộ

    Color[,]    gridColor;
    bool[,]     gridOccupied;
    GameObject[,] cellObjs;
    SpriteRenderer[,] cellFill;

    TextMeshPro[]    rowClueLabels, colClueLabels;
    SpriteRenderer[] rowClueBGs,    colClueBGs;

    class PieceInstance
    {
        public GameObject  root;
        public PieceShapeData shape;
        public Vector2Int[] currentCells;
        public int         rotState;
        public Color       color;
        public bool        usePrefab;          // true = sprite từ prefab, false = tạo bằng code
        public List<SpriteRenderer> visuals   = new();
        public List<BoxCollider2D>  colliders = new();
        public bool  isPlaced;
        public int   placedRow, placedCol;
        public Vector3 homePos;
        public int   pressedCellIdx = 0;      // ô đang bấm vào → pivot khi xoay
    }
    List<PieceInstance> pieces = new();

    PieceInstance dragging;
    Vector3       dragOffset;
    Vector3       pressWorld;      // vị trí world lúc bấm xuống
    float         dragStartTime;
    bool          isDragging;

    List<GameObject> hintObjs = new();
    bool hintActive;

    // ── Layout constants ─────────────────────────────────────────
    // CELL = world-unit size of one grid square
    // GAP  = space between squares
    // step = CELL + GAP  (distance between cell centres)
    const float CELL         = 1.0f;
    const float GAP          = 0.08f;
    const float CLUE_OFFSET  = 1.3f;
    float Step => CELL + GAP;

    Vector3 boardOrigin; // world position of the TOP-LEFT corner of cell[0,0]
                         // i.e. NOT the centre — the corner.

    Canvas          uiCanvas;
    TextMeshProUGUI levelLabel;
    Button          hintBtn, restartBtn;
    GameObject      winPanel, toastObj;
    TextMeshProUGUI toastText;

    // ═══════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    void Awake()
    {
        // GameManager sẽ gọi LoadLevel() sau — không làm gì ở đây
    }

    void Start()
    {
        SetupCamera();
        BuildUI();
        gameObject.SetActive(false); // ẩn cho đến khi GameManager gọi LoadLevel()
    }

    void Update() => HandleInput();

    // ═══════════════════════════════════════════════════════════
    //  CAMERA
    // ═══════════════════════════════════════════════════════════

    void SetupCamera()
    {
        var cam = Camera.main;
        cam.orthographic    = true;
        cam.backgroundColor = new Color(0.13f, 0.18f, 0.28f);
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(0, 0, -10);
    }

    // ═══════════════════════════════════════════════════════════
    //  LEVEL LOAD
    // ═══════════════════════════════════════════════════════════

    /// <summary>Được gọi từ GameManager.StartLevel()</summary>
    public void LoadLevel(LevelDataSO levelData)
    {
        gameObject.SetActive(true);
        currentLevelData = levelData;

        ClearScene();
        var L = Level;

        float totalW = L.cols * Step - GAP;
        float totalH = L.rows * Step - GAP;

        boardOrigin = new Vector3(-totalW / 2f, totalH / 2f + 1.5f, 0);

        gridOccupied = new bool[L.rows, L.cols];
        gridColor    = new Color[L.rows, L.cols];

        BuildBoard();
        BuildClues();
        BuildPieces();
        RefreshClues();

        int idx = GameManagerTest.Instance ? GameManagerTest.Instance.GetLevelIndex(levelData) : 0;
        levelLabel.text = $"Level {idx + 1}";
        winPanel.SetActive(false);
        hintActive = false;
    }

    public void HideGame()
    {
        ClearScene();
        gameObject.SetActive(false);
    }

    // Legacy alias kept so existing calls compile
    void LoadCurrentLevel() => LoadLevel(currentLevelData);

    void ClearScene()
    {
        if (cellObjs != null)
            foreach (var o in cellObjs) if (o) Destroy(o);

        var toDelete = new List<GameObject>();
        if (rowClueLabels != null)
            foreach (var t in rowClueLabels) if (t) toDelete.Add(t.transform.parent.gameObject);
        if (colClueLabels != null)
            foreach (var t in colClueLabels) if (t) toDelete.Add(t.transform.parent.gameObject);
        foreach (var g in toDelete) Destroy(g);

        foreach (var p in pieces) if (p.root) Destroy(p.root);
        pieces.Clear();

        foreach (var h in hintObjs) if (h) Destroy(h);
        hintObjs.Clear();
    }

    // ═══════════════════════════════════════════════════════════
    //  BOARD
    // ═══════════════════════════════════════════════════════════

    void BuildBoard()
    {
        var L = Level;
        cellObjs = new GameObject[L.rows, L.cols];
        cellFill = new SpriteRenderer[L.rows, L.cols];

        for (int r = 0; r < L.rows; r++)
        for (int c = 0; c < L.cols; c++)
        {
            var bg = new GameObject($"Cell_{r}_{c}");
            bg.transform.position   = CellCenter(r, c);
            bg.transform.localScale = Vector3.one * CELL;

            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite       = MakeRoundedSprite();
            bgSr.color        = new Color(0.22f, 0.28f, 0.42f);
            bgSr.sortingOrder = 0;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform, false);
            fill.transform.localPosition = new Vector3(0, 0, -0.01f);
            fill.transform.localScale    = Vector3.one * 0.88f;

            var fillSr = fill.AddComponent<SpriteRenderer>();
            fillSr.sprite       = MakeRoundedSprite();
            fillSr.color        = Color.clear;
            fillSr.sortingOrder = 1;

            cellObjs[r, c] = bg;
            cellFill[r, c] = fillSr;
        }
    }

    // ─── Coordinate helpers ───────────────────────────────────────
    // All piece cell offsets use: +X = right, +Y = DOWN (row-major)
    // World space:                +X = right, +Y = UP
    // So row offset maps to -Y in world space.

    /// <summary>World position of the CENTRE of grid cell [row,col].</summary>
    Vector3 CellCenter(int row, int col)
    {
        // boardOrigin is top-left corner of cell[0,0]
        // centre of cell[0,0] is boardOrigin + (CELL/2, -CELL/2)
        return boardOrigin + new Vector3(col * Step + CELL * 0.5f,
                                        -(row * Step + CELL * 0.5f), 0);
    }

    /// <summary>
    /// Convert a world position to the nearest grid [col, row] (x=col, y=row).
    /// </summary>
    Vector2Int WorldToCell(Vector3 world)
    {
        // Offset from boardOrigin top-left corner
        float lx = world.x - boardOrigin.x;
        float ly = boardOrigin.y - world.y;   // flip Y: down = positive row

        int col = Mathf.RoundToInt((lx - CELL * 0.5f) / Step);
        int row = Mathf.RoundToInt((ly - CELL * 0.5f) / Step);
        return new Vector2Int(col, row);
    }

    bool InBounds(int r, int c) => r >= 0 && r < Level.rows && c >= 0 && c < Level.cols;

    // ═══════════════════════════════════════════════════════════
    //  CLUES
    // ═══════════════════════════════════════════════════════════

    void BuildClues()
    {
        var L = Level;
        rowClueLabels = new TextMeshPro[L.rows];
        rowClueBGs    = new SpriteRenderer[L.rows];
        colClueLabels = new TextMeshPro[L.cols];
        colClueBGs    = new SpriteRenderer[L.cols];

        for (int r = 0; r < L.rows; r++)
        {
            var pos = CellCenter(r, 0) + Vector3.left * CLUE_OFFSET;
            var go  = MakeClueBadge(pos, L.rowCounts[r],
                L.colorMode && L.rowColors != null ? L.rowColors[r] : Color.white);
            rowClueBGs[r]    = go.GetComponent<SpriteRenderer>();
            rowClueLabels[r] = go.GetComponentInChildren<TextMeshPro>();
        }

        for (int c = 0; c < L.cols; c++)
        {
            var pos = CellCenter(0, c) + Vector3.up * CLUE_OFFSET;
            var go  = MakeClueBadge(pos, L.colCounts[c],
                L.colorMode && L.colColors != null ? L.colColors[c] : Color.white);
            colClueBGs[c]    = go.GetComponent<SpriteRenderer>();
            colClueLabels[c] = go.GetComponentInChildren<TextMeshPro>();
        }
    }

    GameObject MakeClueBadge(Vector3 pos, int count, Color clueColor)
    {
        var root = new GameObject("Clue");
        root.transform.position   = pos;
        root.transform.localScale = Vector3.one * CELL * 0.85f;

        bool colored = clueColor != Color.white;

        var sr = root.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeRoundedSprite();
        sr.color        = colored
            ? new Color(clueColor.r, clueColor.g, clueColor.b, 0.25f)
            : new Color(0.85f, 0.88f, 0.95f, 1f);
        sr.sortingOrder = 2;

        if (colored)
        {
            var border = new GameObject("Border");
            border.transform.SetParent(root.transform, false);
            border.transform.localPosition = new Vector3(0, 0, 0.01f);
            border.transform.localScale    = Vector3.one * 1.06f;
            var bsr = border.AddComponent<SpriteRenderer>();
            bsr.sprite       = MakeRoundedSprite();
            bsr.color        = new Color(clueColor.r, clueColor.g, clueColor.b, 0.55f);
            bsr.sortingOrder = 2;
        }

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(root.transform, false);
        labelGO.transform.localPosition = new Vector3(0, 0, -0.1f);
        labelGO.transform.localScale    = Vector3.one * 0.9f;

        var tmp = labelGO.AddComponent<TextMeshPro>();
        tmp.text      = count.ToString();
        tmp.fontSize  = 3f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = colored ? clueColor : new Color(0.1f, 0.1f, 0.15f);
        tmp.sortingOrder = 3;
        tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 1f);

        return root;
    }

    void RefreshClues()
    {
        var L = Level;
        for (int r = 0; r < L.rows; r++)
            SetClueState(rowClueBGs[r], rowClueLabels[r],
                CountRow(r), L.rowCounts[r],
                L.colorMode && L.rowColors != null ? L.rowColors[r] : Color.white);

        for (int c = 0; c < L.cols; c++)
            SetClueState(colClueBGs[c], colClueLabels[c],
                CountCol(c), L.colCounts[c],
                L.colorMode && L.colColors != null ? L.colColors[c] : Color.white);
    }

    int CountRow(int r)
    {
        var L = Level; int n = 0;
        for (int c = 0; c < L.cols; c++)
        {
            if (!gridOccupied[r, c]) continue;
            if (!L.colorMode) { n++; continue; }
            if (L.rowColors != null && ColorMatch(gridColor[r, c], L.rowColors[r])) n++;
        }
        return n;
    }

    int CountCol(int c)
    {
        var L = Level; int n = 0;
        for (int r = 0; r < L.rows; r++)
        {
            if (!gridOccupied[r, c]) continue;
            if (!L.colorMode) { n++; continue; }
            if (L.colColors != null && ColorMatch(gridColor[r, c], L.colColors[c])) n++;
        }
        return n;
    }

    void SetClueState(SpriteRenderer bg, TextMeshPro lbl, int current, int target, Color clueColor)
    {
        bool colored = clueColor != Color.white;
        if (current == target)
        {
            bg.color  = new Color(0.25f, 0.85f, 0.40f, 0.9f);
            lbl.color = new Color(0.05f, 0.35f, 0.10f);
        }
        else if (current > target)
        {
            bg.color  = new Color(0.95f, 0.25f, 0.25f, 0.9f);
            lbl.color = Color.white;
        }
        else
        {
            bg.color  = colored
                ? new Color(clueColor.r, clueColor.g, clueColor.b, 0.25f)
                : new Color(0.85f, 0.88f, 0.95f, 1f);
            lbl.color = colored ? clueColor : new Color(0.1f, 0.1f, 0.15f);
        }
    }

    bool ColorMatch(Color a, Color b) =>
        Mathf.Abs(a.r - b.r) < 0.15f &&
        Mathf.Abs(a.g - b.g) < 0.15f &&
        Mathf.Abs(a.b - b.b) < 0.15f;

    // ═══════════════════════════════════════════════════════════
    //  PIECES
    // ═══════════════════════════════════════════════════════════

    void BuildPieces()
    {
        var L = Level;
        float boardW = L.cols * Step - GAP;

        float startY = boardOrigin.y - L.rows * Step - 0.8f;
        float x      = -boardW / 2f;
        float y      = startY;
        float rowH   = 0;

        if (L.pieces == null || L.pieces.Length == 0) return;
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
                y   -= rowH + 0.4f;
                rowH = 0;
            }

            // Root is placed so that cell(0,0) offset lands at the correct world position.
            // We want cell offset (minCX, minCY) to appear at (x, y).
            // root.pos + offset*Step = cell_world
            // => root.pos = (x - minCX*Step, y + minCY*Step)  [Y flipped: +row = -world.y]
            float homeX = x + (pw * 0.5f) - (minCX + maxCX) * 0.5f * Step;
            float homeY = y - (ph * 0.5f) + (minCY + maxCY) * 0.5f * Step;
            var   home  = new Vector3(homeX, homeY, 0);

            CreatePiece(shape, home);
            x    += pw + 0.4f;
            rowH  = Mathf.Max(rowH, ph);
        }
    }

    void CreatePiece(PieceShapeData shape, Vector3 worldPos)
    {
        var pi = new PieceInstance
        {
            shape        = shape,
            color        = shape.color,
            usePrefab    = shape.prefab != null,
            rotState     = 0,
            isPlaced     = false,
            placedRow    = -1,
            placedCol    = -1,
            currentCells = (Vector2Int[])shape.cells.Clone(),
            homePos      = worldPos
        };

        if (shape.prefab != null)
        {
            // ── Dùng Prefab (sprite/màu đã có sẵn) ──────────────────────────
            pi.root = UnityEngine.Object.Instantiate(shape.prefab, worldPos, Quaternion.identity);
            pi.root.name = $"Piece_{shape.pieceName}";
        }
        else
        {
            // ── Tạo bằng code (fallback) ─────────────────────────────────────
            pi.root = new GameObject($"Piece_{shape.pieceName}");
            pi.root.transform.position = worldPos;
        }

        // Đảm bảo có Rigidbody2D kinematic (cần cho OnMouse* events)
        var rb = pi.root.GetComponent<Rigidbody2D>();
        if (rb == null) rb = pi.root.AddComponent<Rigidbody2D>();
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        RebuildPieceVisuals(pi);
        pieces.Add(pi);
    }

    void RebuildPieceVisuals(PieceInstance pi)
    {
        // Xoá visuals cũ (chỉ xoá GO con tạo bằng code, không xoá GO của prefab)
        foreach (var v in pi.visuals)   if (v) Destroy(v.gameObject);
        pi.visuals.Clear();
        foreach (var c in pi.colliders) if (c) Destroy(c);
        pi.colliders.Clear();

        foreach (var offset in pi.currentCells)
        {
            if (!pi.usePrefab)
            {
                // ── Code mode: tạo SpriteRenderer con ───────────────────────
                var vis = new GameObject("V");
                vis.transform.SetParent(pi.root.transform, false);
                vis.transform.localPosition = new Vector3(offset.x * Step, -offset.y * Step, 0);
                vis.transform.localScale    = Vector3.one * CELL * 0.92f;

                var sr = vis.AddComponent<SpriteRenderer>();
                sr.sprite       = MakeRoundedSprite();
                sr.color        = pi.color;
                sr.sortingOrder = 4;
                pi.visuals.Add(sr);

                var inner = new GameObject("I");
                inner.transform.SetParent(vis.transform, false);
                inner.transform.localPosition = new Vector3(0, 0, -0.01f);
                inner.transform.localScale    = Vector3.one * 0.7f;
                var isr = inner.AddComponent<SpriteRenderer>();
                isr.sprite       = MakeRoundedSprite();
                isr.color        = new Color(1, 1, 1, 0.15f);
                isr.sortingOrder = 5;
            }

            // Collider luôn tạo lại (cả code mode lẫn prefab mode)
            var box    = pi.root.AddComponent<BoxCollider2D>();
            box.offset = new Vector2(offset.x * Step, -offset.y * Step);
            box.size   = new Vector2(CELL * 0.92f, CELL * 0.92f);
            pi.colliders.Add(box);
        }

        // Prefab mode: thu thập SpriteRenderer từ prefab để dùng preview/sort
        if (pi.usePrefab)
        {
            pi.visuals.Clear();
            foreach (var sr in pi.root.GetComponentsInChildren<SpriteRenderer>())
                pi.visuals.Add(sr);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  INPUT
    // ═══════════════════════════════════════════════════════════

    void HandleInput()
    {
        bool pressed    = Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame  : Input.GetMouseButtonDown(0);
        bool held       = Mouse.current != null ? Mouse.current.leftButton.isPressed             : Input.GetMouseButton(0);
        bool released   = Mouse.current != null ? Mouse.current.leftButton.wasReleasedThisFrame  : Input.GetMouseButtonUp(0);
        bool rightClick = Mouse.current != null ? Mouse.current.rightButton.wasPressedThisFrame  : Input.GetMouseButtonDown(1);

        Vector2 screen  = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : (Vector2)Input.mousePosition;
        Vector3 world   = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10));
        world.z = 0;

        if (pressed)
        {
            var hit = PickPiece(world);
            if (hit != null)
            {
                if (hit.isPlaced) UnplacePiece(hit);
                dragging              = hit;
                dragOffset            = hit.root.transform.position - world;
                pressWorld            = world;
                dragStartTime         = Time.time;
                isDragging            = false;
                hit.pressedCellIdx    = FindClosestCell(hit, world);  // ô đang bấm
                SetPieceSortOrder(hit, 10);
            }
        }

        if (held && dragging != null)
        {
            Vector3 newPos = world + dragOffset;
            if (!isDragging && Vector3.Distance(newPos, dragging.root.transform.position) > 0.08f)
                isDragging = true;

            if (isDragging)
            {
                dragging.root.transform.position = new Vector3(newPos.x, newPos.y, -1f);

                // ✅ FIX: preview uses root position (not cursor) so it matches snap target
                var previewCell = WorldToCell(new Vector3(newPos.x, newPos.y, 0));
                ShowDropPreview(dragging, previewCell.y, previewCell.x);
            }
        }

        if (released && dragging != null)
        {
            ClearDropPreview();
            float held_t = Time.time - dragStartTime;

            if (!isDragging && held_t < 0.25f)
            {
                // Tap = xoay quanh ô đang bấm
                RotatePiece(dragging, dragging.pressedCellIdx);
            }
            else if (isDragging)
            {
                Vector3 rootPos   = dragging.root.transform.position;
                var     cell      = WorldToCell(new Vector3(rootPos.x, rootPos.y, 0));

                if (TryPlace(dragging, cell.y, cell.x))
                {
                    SetPieceSortOrder(dragging, 3);
                    CheckWin();
                }
                else
                {
                    dragging.root.transform.position = dragging.homePos;
                    SetPieceSortOrder(dragging, 4);
                }
            }

            dragging   = null;
            isDragging = false;
        }

        if (rightClick)
        {
            var hit = PickPiece(world);
            if (hit != null && !hit.isPlaced)
            {
                int pivotIdx = FindClosestCell(hit, world);
                RotatePiece(hit, pivotIdx);
            }
        }
    }

    PieceInstance PickPiece(Vector3 world)
    {
        PieceInstance best = null;
        int bestOrd = -999;
        foreach (var p in pieces)
        {
            foreach (var col in p.colliders)
            {
                if (col == null) continue;
                if (!col.OverlapPoint(world)) continue;
                int ord = p.isPlaced ? 3 : 4;
                if (ord > bestOrd) { best = p; bestOrd = ord; }
                break;
            }
        }
        return best;
    }

    void ShowDropPreview(PieceInstance pi, int startRow, int startCol)
    {
        bool can = CanPlace(pi, startRow, startCol);
        Color tint = can
            ? new Color(0.3f, 1f, 0.45f, 0.85f)
            : new Color(1f, 0.25f, 0.25f, 0.75f);
        foreach (var sr in pi.visuals) sr.color = tint;
    }

    void ClearDropPreview()
    {
        if (dragging == null) return;
        // Prefab: reset về trắng (sprite tự có màu gốc). Code: dùng pieceColor.
        Color resetColor = dragging.usePrefab ? Color.white : dragging.color;
        foreach (var sr in dragging.visuals) sr.color = resetColor;
    }

    void SetPieceSortOrder(PieceInstance pi, int order)
    {
        foreach (var sr in pi.visuals) sr.sortingOrder = order;
    }

    // ═══════════════════════════════════════════════════════════
    //  ROTATION
    // ═══════════════════════════════════════════════════════════

    void RotatePiece(PieceInstance pi, int pivotCellIdx = -1)
    {
        if (pi.isPlaced) return;

        // Offset của ô pivot TRƯỚC khi xoay
        Vector2Int pivotBefore = (pivotCellIdx >= 0 && pivotCellIdx < pi.currentCells.Length)
            ? pi.currentCells[pivotCellIdx] : Vector2Int.zero;

        pi.rotState = (pi.rotState + 1) % 4;
        pi.currentCells = new Vector2Int[pi.shape.cells.Length];
        for (int i = 0; i < pi.shape.cells.Length; i++)
        {
            var c = pi.shape.cells[i];
            for (int r = 0; r < pi.rotState; r++)
                c = new Vector2Int(c.y, -c.x);  // 90° CW in grid space
            pi.currentCells[i] = c;
        }

        // Offset của ô pivot SAU khi xoay
        Vector2Int pivotAfter = (pivotCellIdx >= 0 && pivotCellIdx < pi.currentCells.Length)
            ? pi.currentCells[pivotCellIdx] : Vector2Int.zero;

        // Dịch root để ô pivot giữ nguyên vị trí world
        Vector3 delta = new Vector3(
            (pivotBefore.x - pivotAfter.x) * Step,
            -(pivotBefore.y - pivotAfter.y) * Step,  // -Y vì grid Y xuống = world -Y
            0);
        pi.root.transform.position += delta;
        pi.homePos                 += delta;

        RebuildPieceVisuals(pi);
    }

    // Tìm index của ô piece gần vị trí world nhất
    int FindClosestCell(PieceInstance pi, Vector3 worldPos)
    {
        int   best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < pi.currentCells.Length; i++)
        {
            var off = pi.currentCells[i];
            Vector3 cellW = pi.root.transform.position +
                new Vector3(off.x * Step, -off.y * Step, 0);
            float dist = Vector3.Distance(worldPos, cellW);
            if (dist < bestDist) { bestDist = dist; best = i; }
        }
        return best;
    }

    // ═══════════════════════════════════════════════════════════
    //  PLACEMENT
    // ═══════════════════════════════════════════════════════════

    bool CanPlace(PieceInstance pi, int startRow, int startCol)
    {
        foreach (var off in pi.currentCells)
        {
            int r = startRow + off.y, c = startCol + off.x;
            if (!InBounds(r, c)) return false;
            if (gridOccupied[r, c]) return false;
        }
        return true;
    }

    bool TryPlace(PieceInstance pi, int startRow, int startCol)
    {
        if (!CanPlace(pi, startRow, startCol)) return false;

        // Mark grid
        foreach (var off in pi.currentCells)
        {
            int r = startRow + off.y, c = startCol + off.x;
            gridOccupied[r, c]    = true;
            gridColor[r, c]       = pi.color;
            cellFill[r, c].color  = pi.color;
        }

        pi.isPlaced  = true;
        pi.placedRow = startRow;
        pi.placedCol = startCol;

        // ✅ FIX: snap root to exact world position of anchor cell
        // Then each visual child is at localPos = (off.x*Step, -off.y*Step)
        // => worldPos of cell visual = root + (off.x*Step, -off.y*Step)
        //                            = CellCenter(startRow,startCol) + (off.x*Step, -off.y*Step)
        //                            = CellCenter(startRow+off.y, startCol+off.x)  ✓
        Vector3 snap = CellCenter(startRow, startCol);
        // pi.root.transform.position = new Vector3(snap.x, snap.y, 0);

        // Ensure visuals are at correct local positions (in case of float drift)
        // for (int i = 0; i < pi.currentCells.Length; i++)
        // {
        //     var off = pi.currentCells[i];
        //     pi.visuals[i].transform.parent.localPosition =
        //         new Vector3(off.x * Step, -off.y * Step, 0);
        // }
        pi.root.transform.position = new Vector3(snap.x, snap.y, 0.1f);
        RefreshClues();
        return true;
    }

    void UnplacePiece(PieceInstance pi)
    {
        foreach (var off in pi.currentCells)
        {
            int r = pi.placedRow + off.y, c = pi.placedCol + off.x;
            if (!InBounds(r, c)) continue;
            gridOccupied[r, c]   = false;
            gridColor[r, c]      = Color.clear;
            cellFill[r, c].color = Color.clear;
        }
        pi.isPlaced  = false;
        pi.placedRow = -1;
        pi.placedCol = -1;
        RefreshClues();
    }

    // ═══════════════════════════════════════════════════════════
    //  WIN CHECK
    // ═══════════════════════════════════════════════════════════

    void CheckWin()
    {
        var L = Level;
        for (int r = 0; r < L.rows; r++)
            if (CountRow(r) != L.rowCounts[r]) return;
        for (int c = 0; c < L.cols; c++)
            if (CountCol(c) != L.colCounts[c]) return;
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        yield return new WaitForSeconds(0.4f);
        foreach (var sr in cellFill)
            StartCoroutine(FlashColor(sr, new Color(0.3f,0.9f,0.4f), 0.4f));
        yield return new WaitForSeconds(0.6f);
        winPanel.SetActive(true);

        // Lưu progress qua GameManager + SaveManager
        if (GameManagerTest.Instance) GameManagerTest.Instance.OnLevelComplete(currentLevelData);
    }

    IEnumerator FlashColor(SpriteRenderer sr, Color to, float dur)
    {
        if (sr == null || sr.color == Color.clear) yield break;
        Color from = sr.color; float t = 0;
        while (t < dur) { t += Time.deltaTime; sr.color = Color.Lerp(from, to, t/dur); yield return null; }
    }

    // ═══════════════════════════════════════════════════════════
    //  HINT
    // ═══════════════════════════════════════════════════════════

    public void RequestHint()
    {
        // Replace ShowHint() with: AdManager.Instance.ShowRewardedAd(() => ShowHint());
        ShowHint();
        ShowToast("Gợi ý hiện trong 6 giây!");
    }

    void ShowHint()
    {
        if (hintActive) return;
        hintActive = true;
        var L = Level;
        if (L.solution == null) return;

        for (int r = 0; r < L.rows; r++)
        for (int c = 0; c < L.cols; c++)
        {
            int idx = r * L.cols + c;
            if (idx >= L.solution.Length || !L.solution[idx]) continue;

            var h = new GameObject("Hint");
            h.transform.position   = CellCenter(r, c);
            h.transform.localScale = Vector3.one * CELL * 0.85f;

            var sr = h.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeRoundedSprite();
            sr.sortingOrder = 6;

            Color hc = Color.white;
            if (L.colorMode && L.solutionColorIdx != null && idx < L.solutionColorIdx.Length)
                hc = IndexToColor(L.solutionColorIdx[idx]);
            sr.color = new Color(hc.r, hc.g, hc.b, 0.35f);
            hintObjs.Add(h);
        }
        StartCoroutine(FadeHint(6f));
    }

    IEnumerator FadeHint(float delay)
    {
        yield return new WaitForSeconds(delay);
        float dur = 1f, t = 0;
        var srs = new List<SpriteRenderer>();
        foreach (var h in hintObjs) { var sr = h?.GetComponent<SpriteRenderer>(); if (sr) srs.Add(sr); }
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0.35f, 0f, t / dur);
            foreach (var sr in srs) { var col = sr.color; col.a = a; sr.color = col; }
            yield return null;
        }
        foreach (var h in hintObjs) if (h) Destroy(h);
        hintObjs.Clear();
        hintActive = false;
    }

    Color IndexToColor(int idx) => idx switch
    {
        1 => Color.red, 2 => Color.blue, 3 => Color.green,
        4 => Color.yellow, 5 => Color.magenta, _ => Color.white
    };

    // ═══════════════════════════════════════════════════════════
    //  UI
    // ═══════════════════════════════════════════════════════════

    void BuildUI()
    {
        var canvasGO = new GameObject("Canvas");
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(540, 960);
        canvasGO.AddComponent<GraphicRaycaster>();

        levelLabel = MakeTMPUI(canvasGO, "LevelLabel", new Vector2(-180, 120), new Vector2(300, 50));
        levelLabel.text      = "Level 1";
        levelLabel.fontSize  = 28;
        levelLabel.fontStyle = FontStyles.Bold;
        levelLabel.alignment = TextAlignmentOptions.Center;
        levelLabel.color     = Color.white;

                MakeButton(canvasGO, "< Menu", new Vector2(0, -360),
            new Vector2(160, 46), new Color(0.25f, 0.28f, 0.40f),
            () => { if (GameManagerTest.Instance) GameManagerTest.Instance.BackToLevelSelect(); });

        hintBtn = MakeButton(canvasGO, "Hint  (Ad)", new Vector2(-180, 70),
            new Vector2(110, 60), new Color(0.15f, 0.55f, 0.95f), RequestHint);

        restartBtn = MakeButton(canvasGO, "Restart", new Vector2(150, 80),
            new Vector2(160, 60), new Color(0.35f, 0.35f, 0.45f), RestartLevel);

        winPanel = MakePanel(canvasGO, "WinPanel", Vector2.zero,
            new Vector2(380, 320), new Color(0.1f, 0.15f, 0.25f, 0.97f));
        winPanel.SetActive(false);

        var wt = MakeTMPUI(winPanel, "WinTitle", new Vector2(0, 80), new Vector2(340, 70));
        wt.text = "Level Complete!"; wt.fontSize = 30; wt.fontStyle = FontStyles.Bold;
        wt.alignment = TextAlignmentOptions.Center; wt.color = new Color(1f, 0.85f, 0.2f);

        MakeButton(winPanel, "Next Level", new Vector2(0, -20),
            new Vector2(220, 58), new Color(0.2f, 0.75f, 0.4f), NextLevel);
        MakeButton(winPanel, "Restart", new Vector2(0, -90),
            new Vector2(220, 48), new Color(0.35f, 0.35f, 0.45f), RestartLevel);

        toastObj  = MakePanel(canvasGO, "Toast", new Vector2(0, -330),
            new Vector2(380, 54), new Color(0.08f, 0.08f, 0.12f, 0.92f));
        toastText = MakeTMPUI(toastObj, "ToastText", Vector2.zero, new Vector2(360, 50));
        toastText.alignment = TextAlignmentOptions.Center;
        toastText.fontSize  = 17; toastText.color = Color.white;
        toastObj.SetActive(false);
    }

    TextMeshProUGUI MakeTMPUI(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go.AddComponent<TextMeshProUGUI>();
    }

    Button MakeButton(GameObject parent, string label, Vector2 pos, Vector2 size,
                      Color bg, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var txt = MakeTMPUI(go, "Txt", Vector2.zero, size);
        txt.text = label; txt.fontSize = 18; txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center; txt.color = Color.white;
        return btn;
    }

    GameObject MakePanel(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        return go;
    }

    public void ShowToast(string msg, float dur = 2.5f)
    {
        StopCoroutine(nameof(ToastCo));
        StartCoroutine(ToastCo(msg, dur));
    }

    IEnumerator ToastCo(string msg, float dur)
    {
        toastText.text = msg; toastObj.SetActive(true);
        yield return new WaitForSeconds(dur);
        toastObj.SetActive(false);
    }

    void RestartLevel()
    {
        winPanel.SetActive(false);
        foreach (var h in hintObjs) if (h) Destroy(h);
        hintObjs.Clear(); hintActive = false;
        LoadLevel(currentLevelData);
    }

    void NextLevel()
    {
        winPanel.SetActive(false);
        if (GameManagerTest.Instance)
        {
            var next = GameManagerTest.Instance.GetNextLevel(currentLevelData);
            if (next != null)
                GameManagerTest.Instance.StartLevel(next);
            else
                GameManagerTest.Instance.BackToLevelSelect();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SPRITE UTILITY
    // ═══════════════════════════════════════════════════════════

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