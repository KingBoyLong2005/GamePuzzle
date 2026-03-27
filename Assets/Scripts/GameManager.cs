using UnityEngine;

// ────────────────────────────────────────────────────────────
//  GameManager  —  Singleton điều phối toàn bộ flow màn hình
//
//  Flow:
//    MainMenu → LevelSelect → Gameplay → (win) → LevelSelect
//
//  Gán trên một Empty GO có DontDestroyOnLoad.
//  Tham chiếu đến UIMainMenu, UILevelSelect, GameController
//  qua Inspector hoặc FindObjectOfType khi cần.
// ────────────────────────────────────────────────────────────

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("All Levels (kéo LevelDataSO assets vào đây theo thứ tự)")]
    public LevelDataSO[] allLevels;

    // ── State ─────────────────────────────────────────────────
    public enum Screen { MainMenu, LevelSelect, Gameplay }
    public Screen CurrentScreen { get; private set; } = Screen.MainMenu;

    LevelDataSO activeLevel;
    public LevelDataSO ActiveLevel => activeLevel;

    // ── Refs (tự tìm hoặc gán) ───────────────────────────────
    UIMainMenu    uiMain;
    UILevelSelect uiLevelSelect;
    GameController gameCtrl;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        uiMain        = FindFirstObjectByType<UIMainMenu>(FindObjectsInactive.Include);
        uiLevelSelect = FindFirstObjectByType<UILevelSelect>(FindObjectsInactive.Include);
        gameCtrl      = FindFirstObjectByType<GameController>(FindObjectsInactive.Include);

        ShowMainMenu();
    }

    // ── Navigation ────────────────────────────────────────────

    public void ShowMainMenu()
    {
        CurrentScreen = Screen.MainMenu;
        uiMain?.Show();
        uiLevelSelect?.Hide();
        gameCtrl?.HideGame();
    }

    public void ShowLevelSelect()
    {
        CurrentScreen = Screen.LevelSelect;
        uiMain?.Hide();
        uiLevelSelect?.Show(allLevels);
        gameCtrl?.HideGame();
    }

    public void StartLevel(LevelDataSO level)
    {
        activeLevel   = level;
        CurrentScreen = Screen.Gameplay;
        uiMain?.Hide();
        uiLevelSelect?.Hide();
        gameCtrl?.LoadLevel(level);
    }

    public void OnLevelComplete(LevelDataSO level)
    {
        SaveManager.Instance.MarkCompleted(level);
    }

    public void BackToLevelSelect()
    {
        gameCtrl?.HideGame();
        ShowLevelSelect();
    }

    // ── Helper ────────────────────────────────────────────────

    public int GetLevelIndex(LevelDataSO level)
    {
        for (int i = 0; i < allLevels.Length; i++)
            if (allLevels[i] == level) return i;
        return -1;
    }

    public LevelDataSO GetNextLevel(LevelDataSO current)
    {
        int idx = GetLevelIndex(current);
        if (idx < 0 || idx + 1 >= allLevels.Length) return null;
        return allLevels[idx + 1];
    }
}
