using UnityEngine;

/// <summary>
/// GameManager: Coordinator trung tâm khởi động một level cụ thể.
/// - Nhận LevelBoardData từ LevelSelectManager khi người chơi chọn level
/// - Truyền data xuống Board và Piece để build board + spawn pieces
/// - Quản lý panel game (hiển thị / ẩn)
///
/// QUAN TRỌNG: GameManager KHÔNG tự gọi CreateBoard() hay BuildPiece() trong Start().
/// Hai hàm đó chỉ được gọi khi StartLevel(data) được invoke từ LevelSelectManager.
/// </summary>
public class GameManagerTest : MonoBehaviour
{
    [Header("UI Panels")]
    // [SerializeField] private GameObject gamePanel;      // Panel bọc toàn bộ màn chơi

    [Header("Game Components")]
    [SerializeField] private Board board;               // Board.cs — chịu trách nhiệm tạo lưới ô
    [SerializeField] private Piece pieceSpawner;        // Piece.cs — chịu trách nhiệm spawn pieces

    [Header("Dependencies")]
    [SerializeField] private MainMenu mainMenu;         // Để quay về MainMenu khi cần

    // ── Vòng đời ──────────────────────────────────────────────────
    private void Start()
    {
        // Panel game ẩn mặc định; chỉ hiện khi StartLevel được gọi
        // if (gamePanel != null)
        //     gamePanel.SetActive(false);
    }

    // ── Được LevelSelectManager gọi ──────────────────────────────
    /// <summary>
    /// Nhận LevelBoardData từ màn chọn level và khởi động trận.
    /// Board và Piece đều được truyền data trước khi gọi hàm build.
    /// </summary>
    public void StartLevel(LevelBoardData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("[GameManager] StartLevel: levelData là null!");
            return;
        }

        // 1. Hiển thị panel game
        // if (gamePanel != null)
        //     gamePanel.SetActive(true);

        // 2. Truyền data vào Board rồi build lưới
        board.levelBoardData = levelData; // levelBoardData phải được đổi thành [SerializeField] public hoặc setter
        board.width  = levelData.width;
        board.height = levelData.height;
        board.CreateBoard(levelData.width, levelData.height);
        board.CreateClueBadge();

        // 3. Truyền data vào Piece rồi build các mảnh ghép
        pieceSpawner.levelData = levelData;
        pieceSpawner.RebuildPieces();
    }

    // ── Nút Back/Thoát game ───────────────────────────────────────
    public void OnExitGameButtonClicked()
    {
        // if (gamePanel != null)
        //     gamePanel.SetActive(false);

        mainMenu.ShowMainMenu();
    }
}