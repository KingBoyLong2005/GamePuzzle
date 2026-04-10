using System.Collections;
using UnityEngine;

// ────────────────────────────────────────────────────────────
//  BoardManager  —  Singleton quản lý lưới bảng
//
//  Trách nhiệm:
//    - Spawn/xoá các ô từ Cell Prefab
//    - Lưu trạng thái ô (bị chiếm / màu)
//    - Chuyển đổi toạ độ world ↔ grid
//    - Đếm ô cho ClueManager
//    - Phát hiệu ứng thắng
//
//  KHÔNG xử lý input hay biết về mảnh ghép.
// ────────────────────────────────────────────────────────────
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("Prefab ô — gán BoardCell prefab vào đây")]
    [SerializeField] BoardCell cellPrefab;

    [Header("Layout")]
    [SerializeField] float cellSize = 1.0f;   // kích thước 1 ô (world units)
    [SerializeField] float cellGap  = 0.08f;  // khoảng trống giữa các ô

    // Khoảng cách giữa hai tâm ô liên tiếp
    public float CellStep => cellSize + cellGap;
    public float CellSize => cellSize;

    // ── State lưới ────────────────────────────────────────────
    bool[,]     cellOccupied;   // ô có bị chiếm không
    Color[,]    cellColor;      // màu của mảnh đang chiếm ô
    BoardCell[,] cells;         // reference đến visual

    LevelDataSO level;
    Vector3     boardOrigin;    // góc trên-trái của ô [0,0]

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Build / Clear ─────────────────────────────────────────

    public void Build(LevelDataSO levelData)
    {
        Clear();
        level = levelData;

        float totalW = level.cols * CellStep - cellGap;
        float totalH = level.rows * CellStep - cellGap;
        boardOrigin  = new Vector3(-totalW / 2f, totalH / 2f + 1.5f, 0);

        cellOccupied = new bool[level.rows, level.cols];
        cellColor    = new Color[level.rows, level.cols];
        cells        = new BoardCell[level.rows, level.cols];

        for (int r = 0; r < level.rows; r++)
        for (int c = 0; c < level.cols; c++)
            cells[r, c] = Instantiate(cellPrefab, CellCenter(r, c), Quaternion.identity, transform);
    }

    public void Clear()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        cellOccupied = null;
        cellColor    = null;
        cells        = null;
    }

    // ── Toạ độ ────────────────────────────────────────────────

    /// <summary>Vị trí world của tâm ô [row, col].</summary>
    public Vector3 CellCenter(int row, int col) =>
        boardOrigin + new Vector3(
            col * CellStep + cellSize * 0.5f,
           -(row * CellStep + cellSize * 0.5f),
            0);

    /// <summary>
    /// Chuyển world position → chỉ số ô.
    /// Trả về Vector2Int(col, row).
    /// </summary>
    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        float localX = worldPos.x - boardOrigin.x;
        float localY = boardOrigin.y - worldPos.y;  // flip Y: xuống = row dương
        int   col    = Mathf.RoundToInt((localX - cellSize * 0.5f) / CellStep);
        int   row    = Mathf.RoundToInt((localY - cellSize * 0.5f) / CellStep);
        return new Vector2Int(col, row);
    }

    public bool IsInBounds(int row, int col) =>
        level != null && row >= 0 && row < level.rows && col >= 0 && col < level.cols;

    // ── Thao tác lưới ─────────────────────────────────────────

    /// <summary>Kiểm tra tất cả offset có thể đặt xuống không.</summary>
    public bool CanPlace(Vector2Int[] offsets, int startRow, int startCol)
    {
        foreach (var off in offsets)
        {
            int r = startRow + off.y, c = startCol + off.x;
            if (!IsInBounds(r, c) || cellOccupied[r, c]) return false;
        }
        return true;
    }

    /// <summary>Đặt mảnh lên bảng — cập nhật state và visual.</summary>
    public void PlacePiece(Vector2Int[] offsets, int startRow, int startCol, Color color)
    {
        foreach (var off in offsets)
        {
            int r = startRow + off.y, c = startCol + off.x;
            cellOccupied[r, c] = true;
            cellColor[r, c]    = color;
            cells[r, c].SetColor(color);
        }
    }

    /// <summary>Gỡ mảnh khỏi bảng — xoá state và visual.</summary>
    public void RemovePiece(Vector2Int[] offsets, int startRow, int startCol)
    {
        foreach (var off in offsets)
        {
            int r = startRow + off.y, c = startCol + off.x;
            if (!IsInBounds(r, c)) continue;
            cellOccupied[r, c] = false;
            cellColor[r, c]    = Color.clear;
            cells[r, c].ClearColor();
        }
    }

    // ── Đếm ô (dùng bởi ClueManager) ─────────────────────────

    public int CountFilledInRow(int row, bool colorMode, Color targetColor)
    {
        int count = 0;
        for (int c = 0; c < level.cols; c++)
        {
            if (!cellOccupied[row, c]) continue;
            if (!colorMode || ColorsMatch(cellColor[row, c], targetColor)) count++;
        }
        return count;
    }

    public int CountFilledInCol(int col, bool colorMode, Color targetColor)
    {
        int count = 0;
        for (int r = 0; r < level.rows; r++)
        {
            if (!cellOccupied[r, col]) continue;
            if (!colorMode || ColorsMatch(cellColor[r, col], targetColor)) count++;
        }
        return count;
    }

    // ── Hiệu ứng thắng ───────────────────────────────────────

    public void PlayWinAnimation() => StartCoroutine(WinFlashRoutine());

    IEnumerator WinFlashRoutine()
    {
        Color winColor = new Color(0.3f, 0.9f, 0.4f);
        for (int r = 0; r < level.rows; r++)
        for (int c = 0; c < level.cols; c++)
            if (cellOccupied[r, c])
                StartCoroutine(FlashCell(cells[r, c], winColor, 0.4f));
        yield return null;
    }

    IEnumerator FlashCell(BoardCell cell, Color targetColor, float duration)
    {
        Color startColor = cell.CurrentColor;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            cell.SetColor(Color.Lerp(startColor, targetColor, t / duration));
            yield return null;
        }
    }

    // ── Helper ────────────────────────────────────────────────

    // So sánh màu với ngưỡng bao dung nhỏ (tránh lỗi float)
    static bool ColorsMatch(Color a, Color b) =>
        Mathf.Abs(a.r - b.r) < 0.15f &&
        Mathf.Abs(a.g - b.g) < 0.15f &&
        Mathf.Abs(a.b - b.b) < 0.15f;

    public void SetCellHintColor(int row, int col, Color color)
    {
        if (!IsInBounds(row, col)) return;
        cells[row, col].SetColor(color);
    }

    public void ClearCellHintColor(int row, int col)
    {
        if (!IsInBounds(row, col)) return;
        if (cellOccupied[row, col])
            cells[row, col].SetColor(cellColor[row, col]);
        else
            cells[row, col].ClearColor();
    }
}
