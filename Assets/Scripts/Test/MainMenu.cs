using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenu: Quản lý màn hình chính với nút Start và Quit.
/// Khi Start được nhấn → hiển thị LevelSelectPanel.
/// Khi Quit được nhấn → thoát ứng dụng.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;       // Panel chứa nút Start/Quit
    [SerializeField] private LevelSelectManager levelSelectManager; // Quản lý màn chọn level

    public Button buttonStart; // Kéo thả nút Start vào đây
    public Button buttonQuit;  // Kéo thả nút Quit vào đây

    private void Start()
    {
        // Đảm bảo MainMenu hiển thị, Level Select ẩn khi khởi đầu
        ShowMainMenu();
        buttonStart.onClick.AddListener(OnStartButtonClicked);
        buttonQuit.onClick.AddListener(OnQuitButtonClicked);
    }

    // ── Nút Start ──────────────────────────────────────────────────
    public void OnStartButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        levelSelectManager.ShowLevelSelect();
    }

    // ── Nút Quit ───────────────────────────────────────────────────
    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Public: quay lại MainMenu từ màn khác ─────────────────────
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectManager.HideLevelSelect();
    }
}