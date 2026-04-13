using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleInput : MonoBehaviour
{
    // ── Events ────────────────────────────────────────────────
    public event Action OnPiecePlaced;
    public event Action OnPieceUnplaced;
    public PieceInstance heldPiece;
    public bool          pendingUnplace;
    public bool          isDragging;
    public float         pressTime;
    public Vector3       holdOffset;
    void Update()
    {
        if (PieceManager.Instance.level == null) return;

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

            // Tách khỏi content → piece không còn bị kéo theo scroll nữa
            heldPiece.DetachFromContent();

            // Báo cho WorldScrollView biết đang drag piece → tự MoveBack + khoá scroll
            PieceManager.Instance.worldScrollView?.OnPieceDragStart();

            if (heldPiece.IsPlaced)
            {
                UnplacePiece(heldPiece);
                pendingUnplace = false;
            }
        }

        if (!isDragging) return;

        heldPiece.MoveTo(new Vector3(targetPos.x, targetPos.y, -1f));

        var  snapCell = PieceManager.Instance.board.WorldToCell(targetPos);
        bool canDrop  = PieceManager.Instance.board.CanPlace(heldPiece.CurrentOffsets, snapCell.y, snapCell.x);
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
            // Mở lại scroll sau khi thả
            PieceManager.Instance.worldScrollView?.OnPieceDragEnd();
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

    // ── Xoay mảnh ────────────────────────────────────────────

    public void RotatePiece(PieceInstance piece)
    {
        if (piece.IsPlaced)
        {
            int savedRow = piece.PlacedRow;
            int savedCol = piece.PlacedCol;

            PieceManager.Instance.board.RemovePiece(piece.CurrentOffsets, savedRow, savedCol);
            piece.RotateInPlace();

            if (PieceManager.Instance.board.CanPlace(piece.CurrentOffsets, savedRow, savedCol))
            {
                PieceManager.Instance.board.PlacePiece(piece.CurrentOffsets, savedRow, savedCol, piece.Color);
                piece.SnapToBoard(PieceManager.Instance.board.CellCenter(savedRow, savedCol), savedRow, savedCol);
                piece.SetSortOrder(3);
            }
            else
            {
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
        var snapCell = PieceManager.Instance.board.WorldToCell(piece.Position);
        int row = snapCell.y, col = snapCell.x;

        if (PieceManager.Instance.board.CanPlace(piece.CurrentOffsets, row, col))
        {
            // Đặt thành công → giữ nguyên tách khỏi content (đã đặt lên board)
            PieceManager.Instance.board.PlacePiece(piece.CurrentOffsets, row, col, piece.Color);
            piece.SnapToBoard(PieceManager.Instance.board.CellCenter(row, col), row, col);
            piece.SetSortOrder(3);
            OnPiecePlaced?.Invoke();
        }
        else
        {
            // Thả sai chỗ → trả về home, reattach vào content
            piece.ReattachToContent();
            piece.ReturnHome();
            piece.SetSortOrder(4);
        }
    }

    void UnplacePiece(PieceInstance piece)
    {
        PieceManager.Instance.board.RemovePiece(piece.CurrentOffsets, piece.PlacedRow, piece.PlacedCol);
        piece.Unplace();
        OnPieceUnplaced?.Invoke();
    }

    // ── Helper ────────────────────────────────────────────────

    PieceInstance FindPieceAt(Vector3 worldPos)
    {
        PieceInstance best     = null;
        int           bestPrio = -1;

        foreach (var p in PieceManager.Instance.allPieces)
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
}