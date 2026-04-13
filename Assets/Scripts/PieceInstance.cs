using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceInstance
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

    Vector2Int[] baseOffsets;
    Vector2Int[] currentOffsets;
    int          rotationSteps;

    Vector3    homePos;
    Transform  contentParent;    // parent gốc = content của scroll
    Transform  detachedParent;   // parent khi đang drag = PieceManager

    readonly List<SpriteRenderer> cellRenderers = new();
    readonly List<BoxCollider2D>  cellColliders = new();

    public PieceInstance(PieceData pieceData, Vector3 home,
                            GameObject prefab,
                            Transform contentParent,
                            Transform detachedParent,
                            float step, float size)
    {
        data               = pieceData;
        homePos            = home;
        cellPrefab         = prefab;
        cellStep           = step;
        cellSize           = size;
        this.contentParent  = contentParent;
        this.detachedParent = detachedParent;

        baseOffsets    = pieceData.GetCellOffsets();
        currentOffsets = (Vector2Int[])baseOffsets.Clone();

        root = new GameObject($"Piece_{data.pieceName}");
        root.transform.SetParent(contentParent);   // bắt đầu nằm trong content
        root.transform.position = home;

        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        BuildVisuals();
    }

    // Tách khỏi content → piece không đi theo scroll
    public void DetachFromContent()
    {
        root.transform.SetParent(detachedParent, worldPositionStays: true);
    }

    // Gắn lại vào content → piece đi theo scroll
    public void ReattachToContent()
    {
        root.transform.SetParent(contentParent, worldPositionStays: true);
    }

    void BuildVisuals()
    {
        ClearVisuals();

        foreach (var off in currentOffsets)
        {
            var cell = UnityEngine.Object.Instantiate(cellPrefab, root.transform);
            cell.transform.localPosition = new Vector3(off.x * cellStep, -off.y * cellStep, 0);

            var sr = cell.GetComponent<SpriteRenderer>();
            sr.color        = data.color;
            sr.sortingOrder = 4;
            cellRenderers.Add(sr);

            var col = cell.GetComponent<BoxCollider2D>();
            if (col == null) col = cell.AddComponent<BoxCollider2D>();
            cellColliders.Add(col);
        }
    }

    void ClearVisuals()
    {
        foreach (var sr in cellRenderers) if (sr) UnityEngine.Object.Destroy(sr.gameObject);
        cellRenderers.Clear();
        cellColliders.Clear();
    }

    void ApplyRotation()
    {
        currentOffsets = new Vector2Int[baseOffsets.Length];
        for (int i = 0; i < baseOffsets.Length; i++)
        {
            var c = baseOffsets[i];
            for (int s = 0; s < rotationSteps; s++)
                c = new Vector2Int(c.y, -c.x);
            currentOffsets[i] = c;
        }

        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var o in currentOffsets)
        {
            if (o.x < minX) minX = o.x;
            if (o.y < minY) minY = o.y;
        }
        for (int i = 0; i < currentOffsets.Length; i++)
            currentOffsets[i] -= new Vector2Int(minX, minY);
    }

    public void Rotate()
    {
        rotationSteps = (rotationSteps + 1) % 4;
        ApplyRotation();
        BuildVisuals();
    }

    public void RotateInPlace()
    {
        rotationSteps = (rotationSteps + 1) % 4;
        ApplyRotation();
        BuildVisuals();
    }

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
        // Reattach trước khi về home để homePos đúng trong local space
        ReattachToContent();
        ReturnHome();
    }

    public void Destroy() { if (root) UnityEngine.Object.Destroy(root); }

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

    public bool ContainsPoint(Vector3 worldPos)
    {
        foreach (var col in cellColliders)
            if (col && col.OverlapPoint(worldPos)) return true;
        return false;
    }
}