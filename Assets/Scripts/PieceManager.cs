using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ────────────────────────────────────────────────────────────
//  PieceManager  —  quản lý toàn bộ mảnh ghép trong level
//
//  Trách nhiệm:
//    - Spawn các mảnh từ PieceCell Prefab
//    - Xử lý kéo / thả / xoay
//    - Giao tiếp với BoardManager để đặt/gỡ mảnh
//    - Báo cáo lại ngoài qua event (không phụ thuộc GameController)
//
//  PieceInstance (class lồng bên dưới) quản lý
//  state + visual của từng mảnh riêng lẻ.
// ────────────────────────────────────────────────────────────
public class PieceManager : MonoBehaviour
{
    [Header("Prefab ô mảnh — 1 ô vuông có SpriteRenderer + BoxCollider2D")]
    [SerializeField] GameObject pieceCellPrefab;

    // ── Events báo ra ngoài (GameController lắng nghe) ───────
    public event Action OnPiecePlaced;
    public event Action OnPieceUnplaced;

    List<PieceInstance> allPieces = new();

    // Trạng thái kéo hiện tại
    PieceInstance heldPiece;
    bool          pendingUnplace;
    bool          isDragging;
    float         pressTime;
    Vector3       holdOffset;   // offset con trỏ → root mảnh khi bắt đầu giữ

    BoardManager board;
    LevelDataSO  level;

    // ── Setup ─────────────────────────────────────────────────

    public void SpawnPieces(LevelDataSO levelData)
    {
        Clear();
        level = levelData;
        board = BoardManager.Instance;

        if (level.pieces == null || level.pieces.Length == 0) return;
        LayoutAndSpawnAllPieces();
    }

    public void Clear()
    {
        foreach (var p in allPieces) p.Destroy();
        allPieces.Clear();
        heldPiece  = null;
        isDragging = false;
        level      = null;
    }

    // Xếp mảnh thành hàng bên dưới bảng
    void LayoutAndSpawnAllPieces()
    {
        float step       = board.CellStep;
        float boardLeft  = board.CellCenter(0, 0).x              - step * 0.5f;
        float boardRight = board.CellCenter(0, level.cols - 1).x + step * 0.5f;
        float startY     = board.CellCenter(level.rows - 1, 0).y - 1.5f;

        float x       = boardLeft;
        float y       = startY;
        float rowMaxH = 0f;

        foreach (var pieceData in level.pieces)
        {
            var   offsets = pieceData.GetCellOffsets();
            float pieceW  = (MaxX(offsets) + 1) * step;
            float pieceH  = (MaxY(offsets) + 1) * step;

            // Xuống hàng nếu không còn chỗ
            if (x + pieceW > boardRight && x > boardLeft)
            {
                x       = boardLeft;
                y      -= rowMaxH + 0.5f;
                rowMaxH = 0f;
            }

            var homePos = new Vector3(x + pieceW * 0.5f, y - pieceH * 0.5f, 0);
            allPieces.Add(new PieceInstance(pieceData, homePos, pieceCellPrefab, transform, step, board.CellSize));

            x      += pieceW + 0.5f;
            rowMaxH = Mathf.Max(rowMaxH, pieceH);
        }
    }

    // ── Update / Input ────────────────────────────────────────

    void Update()
    {
        if (level == null) return;

        Vector3 worldPos = PointerWorldPos();

        if (PointerDown())     OnDown(worldPos);
        if (PointerHeld())     OnHeld(worldPos);
        if (PointerReleased()) OnReleased();
        if (RightClick())      OnRightClick(worldPos);
    }

    void OnDown(Vector3 worldPos)
    {
        var piece = FindPieceAt(worldPos);
        if (piece == null) return;

        heldPiece      = piece;
        holdOffset     = piece.Position - worldPos;
        pressTime      = Time.time;
        isDragging     = false;
        pendingUnplace = false;
        piece.SetSortOrder(10);
    }

    void OnHeld(Vector3 worldPos)
    {
        if (heldPiece == null) return;

        Vector3 targetPos = worldPos + holdOffset;

        if (!isDragging && Vector3.Distance(targetPos, heldPiece.Position) > 0.08f)
        {
            isDragging = true;

            // Chỉ unplace khi bắt đầu drag thật sự
            if (heldPiece.IsPlaced)
            {
                UnplacePiece(heldPiece);
                pendingUnplace = false;
            }
        }

        if (!isDragging) return;

        heldPiece.MoveTo(new Vector3(targetPos.x, targetPos.y, -1f));

        var  snapCell = board.WorldToCell(targetPos);
        bool canDrop  = board.CanPlace(heldPiece.CurrentOffsets, snapCell.y, snapCell.x);
        heldPiece.SetPreviewTint(canDrop);
    }

    void OnReleased()
    {
        if (heldPiece == null) return;

        heldPiece.ResetTint();
        bool wasTap = !isDragging && (Time.time - pressTime) < 0.25f;

        if (wasTap)
        {
            RotatePiece(heldPiece);
        }
        else if (isDragging)
        {
            TryDropPiece(heldPiece);
        }

        heldPiece      = null;
        isDragging     = false;
        pendingUnplace = false;
    }

    void OnRightClick(Vector3 worldPos)
    {
        var piece = FindPieceAt(worldPos);
        if (piece != null) RotatePiece(piece);
    }

    // ── Xoay mảnh (cả khi đang đặt trên board) ───────────────

    void RotatePiece(PieceInstance piece)
    {
        if (piece.IsPlaced)
        {
            // Gỡ khỏi board → xoay → thử đặt lại đúng vị trí cũ
            int savedRow = piece.PlacedRow;
            int savedCol = piece.PlacedCol;

            board.RemovePiece(piece.CurrentOffsets, savedRow, savedCol);
            piece.RotateInPlace();

            if (board.CanPlace(piece.CurrentOffsets, savedRow, savedCol))
            {
                // Đặt lại được → snap về đúng cell cũ
                board.PlacePiece(piece.CurrentOffsets, savedRow, savedCol, piece.Color);
                piece.SnapToBoard(board.CellCenter(savedRow, savedCol), savedRow, savedCol);
                piece.SetSortOrder(3);
            }
            else
            {
                // Không đặt lại được (ví dụ vượt biên sau xoay) → trả về home
                piece.Unplace();
                piece.SetSortOrder(4);
                OnPieceUnplaced?.Invoke();
            }
        }
        else
        {
            piece.Rotate();
            piece.SetSortOrder(4);
        }
    }

    // ── Đặt / gỡ mảnh ────────────────────────────────────────

    void TryDropPiece(PieceInstance piece)
    {
        var snapCell = board.WorldToCell(piece.Position);
        int row = snapCell.y, col = snapCell.x;

        if (board.CanPlace(piece.CurrentOffsets, row, col))
        {
            board.PlacePiece(piece.CurrentOffsets, row, col, piece.Color);
            piece.SnapToBoard(board.CellCenter(row, col), row, col);
            piece.SetSortOrder(3);
            OnPiecePlaced?.Invoke();
        }
        else
        {
            piece.ReturnHome();
            piece.SetSortOrder(4);
        }
    }

    void UnplacePiece(PieceInstance piece)
    {
        board.RemovePiece(piece.CurrentOffsets, piece.PlacedRow, piece.PlacedCol);
        piece.Unplace();
        OnPieceUnplaced?.Invoke();
    }

    // ── Helper ────────────────────────────────────────────────

    // Tìm mảnh tại vị trí world (ưu tiên mảnh chưa đặt)
    PieceInstance FindPieceAt(Vector3 worldPos)
    {
        PieceInstance best     = null;
        int           bestPrio = -1;

        foreach (var p in allPieces)
        {
            if (!p.ContainsPoint(worldPos)) continue;
            int prio = p.IsPlaced ? 3 : 4;
            if (prio > bestPrio) { best = p; bestPrio = prio; }
        }
        return best;
    }

    static int MaxX(Vector2Int[] offsets)
    { int m = 0; foreach (var o in offsets) m = Mathf.Max(m, o.x); return m; }

    static int MaxY(Vector2Int[] offsets)
    { int m = 0; foreach (var o in offsets) m = Mathf.Max(m, o.y); return m; }

    // ── Input helpers ─────────────────────────────────────────

    Vector3 PointerWorldPos()
    {
        Vector2 screen;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            screen = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            screen = Mouse.current.position.ReadValue();
        else
            screen = (Vector2)Input.mousePosition;

        var pos = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
        pos.z = 0;
        return pos;
    }

    bool PointerDown()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return Mouse.current?.leftButton.wasPressedThisFrame ?? Input.GetMouseButtonDown(0);
    }

    bool PointerHeld()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.isPressed;
        return Mouse.current?.leftButton.isPressed ?? Input.GetMouseButton(0);
    }

    bool PointerReleased()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
        return Mouse.current?.leftButton.wasReleasedThisFrame ?? Input.GetMouseButtonUp(0);
    }

    bool RightClick()
    {
        return Mouse.current?.rightButton.wasPressedThisFrame ?? Input.GetMouseButtonDown(1);
    }

    // ══════════════════════════════════════════════════════════
    //  PieceInstance  —  state + visual của 1 mảnh ghép
    //
    //  Class này chỉ dùng nội bộ trong PieceManager.
    //  Tự quản lý: root GameObject, các ô con, collider.
    // ══════════════════════════════════════════════════════════
    class PieceInstance
    {
        public bool         IsPlaced       { get; private set; }
        public int          PlacedRow      { get; private set; } = -1;
        public int          PlacedCol      { get; private set; } = -1;
        public Color        Color          => data.color;
        public Vector3      Position       => root.transform.position;
        public Vector2Int[] CurrentOffsets => currentOffsets;

        readonly PieceData data;
        readonly float     cellStep;
        readonly float     cellSize;

        GameObject cellPrefab;
        GameObject root;

        // Offsets sau xoay (currentOffsets) vs offsets gốc (baseOffsets)
        Vector2Int[] baseOffsets;
        Vector2Int[] currentOffsets;
        int          rotationSteps;   // 0..3, mỗi bước = 90° CW

        Vector3 homePos;

        // Visual + collision
        readonly List<SpriteRenderer> cellRenderers = new();
        readonly List<BoxCollider2D>  cellColliders = new();

        public PieceInstance(PieceData pieceData, Vector3 home,
                             GameObject prefab, Transform parent,
                             float step, float size)
        {
            data       = pieceData;
            homePos    = home;
            cellPrefab = prefab;
            cellStep   = step;
            cellSize   = size;

            baseOffsets    = pieceData.GetCellOffsets();
            currentOffsets = (Vector2Int[])baseOffsets.Clone();

            root = new GameObject($"Piece_{data.pieceName}");
            root.transform.SetParent(parent);
            root.transform.position = home;

            // Rigidbody2D Kinematic để OverlapPoint của collider hoạt động
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType     = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;

            BuildVisuals();
        }

        // Tạo lại toàn bộ ô con (gọi sau mỗi lần xoay)
        void BuildVisuals()
        {
            ClearVisuals();

            foreach (var off in currentOffsets)
            {
                // off.x = phải, off.y = xuống → -Y trong world
                var cell = UnityEngine.Object.Instantiate(cellPrefab, root.transform);
                cell.transform.localPosition = new Vector3(off.x * cellStep, -off.y * cellStep, 0);

                var sr = cell.GetComponent<SpriteRenderer>();
                sr.color        = data.color;
                sr.sortingOrder = 4;
                cellRenderers.Add(sr);

                var col = cell.GetComponent<BoxCollider2D>();
                if (col == null) col = cell.AddComponent<BoxCollider2D>();
                // col.size = Vector2.one;
                cellColliders.Add(col);
            }
        }

        void ClearVisuals()
        {
            foreach (var sr in cellRenderers) if (sr) UnityEngine.Object.Destroy(sr.gameObject);
            cellRenderers.Clear();
            cellColliders.Clear();
        }

        // ── Tính offsets sau khi xoay, normalize về góc trên-trái ──

        void ApplyRotation()
        {
            currentOffsets = new Vector2Int[baseOffsets.Length];
            for (int i = 0; i < baseOffsets.Length; i++)
            {
                var c = baseOffsets[i];
                for (int s = 0; s < rotationSteps; s++)
                    c = new Vector2Int(c.y, -c.x);   // 90° CW trong grid space
                currentOffsets[i] = c;
            }

            // Normalize: dịch offset sao cho minX = 0, minY = 0
            // Tránh piece bị lệch sau khi xoay
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var o in currentOffsets)
            {
                if (o.x < minX) minX = o.x;
                if (o.y < minY) minY = o.y;
            }
            for (int i = 0; i < currentOffsets.Length; i++)
                currentOffsets[i] -= new Vector2Int(minX, minY);
        }

        // ── Xoay 90° CW khi chưa đặt trên board ─────────────────

        public void Rotate()
        {
            rotationSteps = (rotationSteps + 1) % 4;
            ApplyRotation();
            BuildVisuals();
        }

        // ── Xoay 90° CW khi đang đặt trên board (gọi từ PieceManager) ──
        // Không thay đổi IsPlaced — PieceManager tự xử lý re-place.

        public void RotateInPlace()
        {
            rotationSteps = (rotationSteps + 1) % 4;
            ApplyRotation();
            BuildVisuals();
        }

        // ── Di chuyển ─────────────────────────────────────────

        public void MoveTo(Vector3 pos)  => root.transform.position = pos;
        public void ReturnHome()         => root.transform.position = homePos;

        public void SnapToBoard(Vector3 anchorCenter, int row, int col)
        {
            root.transform.position = new Vector3(anchorCenter.x, anchorCenter.y, 0.1f);
            IsPlaced  = true;
            PlacedRow = row;
            PlacedCol = col;
        }

        public void Unplace()
        {
            IsPlaced  = false;
            PlacedRow = -1;
            PlacedCol = -1;
            ReturnHome();
        }

        public void Destroy() { if (root) UnityEngine.Object.Destroy(root); }

        // ── Visual ────────────────────────────────────────────

        public void SetSortOrder(int order)
        {
            foreach (var sr in cellRenderers) if (sr) sr.sortingOrder = order;
        }

        public void SetPreviewTint(bool canPlace)
        {
            Color tint = canPlace
                ? new Color(0.3f, 1f, 0.45f, 0.85f)
                : new Color(1f, 0.25f, 0.25f, 0.75f);
            foreach (var sr in cellRenderers) if (sr) sr.color = tint;
        }

        public void ResetTint()
        {
            foreach (var sr in cellRenderers) if (sr) sr.color = data.color;
        }

        // ── Collision ─────────────────────────────────────────

        public bool ContainsPoint(Vector3 worldPos)
        {
            foreach (var col in cellColliders)
                if (col && col.OverlapPoint(worldPos)) return true;
            return false;
        }
    }
}