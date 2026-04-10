using UnityEngine;

// ────────────────────────────────────────────────────────────
//  ClueManager  —  spawn và cập nhật badge gợi ý hàng/cột
//
//  Trách nhiệm:
//    - Tạo ClueBadge từ prefab theo vị trí board
//    - Refresh trạng thái badge sau mỗi lần đặt/gỡ mảnh
//    - Kiểm tra tất cả clue đã đúng chưa (để xác định win)
//
//  KHÔNG xử lý input hay biết về mảnh ghép.
// ────────────────────────────────────────────────────────────
public class ClueManager : MonoBehaviour
{
    [Header("Prefab badge — gán ClueBadge prefab vào đây")]
    [SerializeField] ClueBadge badgePrefab;

    [Header("Khoảng cách badge so với mép bảng")]
    [SerializeField] float offsetFromBoard = 1.3f;

    LevelDataSO  level;
    BoardManager board;

    ClueBadge[] rowBadges;
    ClueBadge[] colBadges;

    // ── Build / Clear ─────────────────────────────────────────

    public void Build(LevelDataSO levelData, BoardManager boardManager)
    {
        Clear();
        level = levelData;
        board = boardManager;

        rowBadges = new ClueBadge[level.rows];
        colBadges = new ClueBadge[level.cols];

        for (int r = 0; r < level.rows; r++)
        {
            var pos = board.CellCenter(r, 0) + Vector3.left * offsetFromBoard;
            rowBadges[r] = Instantiate(badgePrefab, pos, Quaternion.identity, transform);
            rowBadges[r].Setup(level.rowCounts[r], GetRowColor(r));
        }

        for (int c = 0; c < level.cols; c++)
        {
            var pos = board.CellCenter(0, c) + Vector3.up * offsetFromBoard;
            colBadges[c] = Instantiate(badgePrefab, pos, Quaternion.identity, transform);
            colBadges[c].Setup(level.colCounts[c], GetColColor(c));
        }
    }

    public void Clear()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        rowBadges = null;
        colBadges = null;
        level     = null;
    }

    // ── Refresh sau mỗi lần đặt/gỡ mảnh ─────────────────────

    public void Refresh()
    {
        if (level == null) return;

        for (int r = 0; r < level.rows; r++)
        {
            int filled = board.CountFilledInRow(r, level.colorMode, GetRowColor(r));
            rowBadges[r].UpdateState(filled, level.rowCounts[r]);
        }

        for (int c = 0; c < level.cols; c++)
        {
            int filled = board.CountFilledInCol(c, level.colorMode, GetColColor(c));
            colBadges[c].UpdateState(filled, level.colCounts[c]);
        }
    }

    // ── Kiểm tra thắng ────────────────────────────────────────

    public bool AllCluesSatisfied()
    {
        for (int r = 0; r < level.rows; r++)
            if (board.CountFilledInRow(r, level.colorMode, GetRowColor(r)) != level.rowCounts[r])
                return false;

        for (int c = 0; c < level.cols; c++)
            if (board.CountFilledInCol(c, level.colorMode, GetColColor(c)) != level.colCounts[c])
                return false;

        return true;
    }

    // ── Helper ────────────────────────────────────────────────

    Color GetRowColor(int r) =>
        level.colorMode && level.rowColors != null && r < level.rowColors.Length
            ? level.rowColors[r] : Color.white;

    Color GetColColor(int c) =>
        level.colorMode && level.colColors != null && c < level.colColors.Length
            ? level.colColors[c] : Color.white;
}
