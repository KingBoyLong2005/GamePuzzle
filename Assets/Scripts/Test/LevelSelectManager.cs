using UnityEngine;


public class LevelSelectManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelSelectPanel;   // Panel bọc toàn bộ màn chọn level
    [SerializeField] private Transform levelButtonContainer; // Content bên trong ScrollView/Grid

    [Header("Prefab")]
    [SerializeField] private LevelButton levelButtonPrefab; // Prefab nút level (có LevelButton component)

    [Header("Data")]
    [SerializeField] private LevelBoardData[] allLevels;    // Kéo thả tất cả LevelBoardData vào đây trong Inspector

    [Header("Dependencies")]
    [SerializeField] private GameManagerTest gameManager;       // Coordinator khởi động game khi level được chọn
    [SerializeField] private MainMenu mainMenu;             // Dùng để Back về MainMenu

    // ── Vòng đời ──────────────────────────────────────────────────
    private void Start()
    {
        // Panel mặc định ẩn; sẽ được bật bởi MainMenu.OnStartButtonClicked
        levelSelectPanel.SetActive(false);
    }

    // ── Hiển thị màn chọn level ────────────────────────────────────
    public void ShowLevelSelect()
    {
        levelSelectPanel.SetActive(true);
        SpawnLevelButtons();
    }

    public void HideLevelSelect()
    {
        levelSelectPanel.SetActive(false);
    }

    // ── Spawn button cho từng level ────────────────────────────────
    private void SpawnLevelButtons()
    {
        // Xóa các button cũ trước khi spawn mới (tránh duplicate khi mở lại panel)
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < allLevels.Length; i++)
        {
            LevelBoardData data = allLevels[i];
            LevelButton btn = Instantiate(levelButtonPrefab, levelButtonContainer);
            btn.Setup(i + 1, data, OnLevelSelected);
        }
    }

    // ── Callback khi người chơi nhấn một level button ─────────────
    private void OnLevelSelected(LevelBoardData selectedLevel)
    {
        HideLevelSelect();
        gameManager.StartLevel(selectedLevel);
    }

    // ── Nút Back (gán vào Inspector nếu cần) ──────────────────────
    public void OnBackButtonClicked()
    {
        HideLevelSelect();
        mainMenu.ShowMainMenu();
    }
}