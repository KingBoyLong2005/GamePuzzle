using UnityEngine;

// ────────────────────────────────────────────────────────────
//  GameManager  —  Singleton điều hướng màn hình
//
//  Flow: MainMenu → LevelSelect → Gameplay → (win) → LevelSelect
//
//  Gán script này lên một Empty GO có DontDestroyOnLoad.
//  Kéo LevelDataSO assets vào allLevels trong Inspector.
// ────────────────────────────────────────────────────────────
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Tất cả các level (theo thứ tự)")]
    public LevelDataSO[] allLevels;

    public enum Screen { MainMenu, LevelSelect, Gameplay }
    public Screen       CurrentScreen { get; private set; }
    public LevelDataSO  ActiveLevel   { get; private set; }

    UIMainMenu     mainMenuUI;
    UILevelSelect  levelSelectUI;
    GameController gameController;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Tìm các UI trong scene (kể cả đang ẩn)
        mainMenuUI    = FindFirstObjectByType<UIMainMenu>(FindObjectsInactive.Include);
        levelSelectUI = FindFirstObjectByType<UILevelSelect>(FindObjectsInactive.Include);
        gameController = FindFirstObjectByType<GameController>(FindObjectsInactive.Include);

        ShowMainMenu();
    }

    // ── Điều hướng ────────────────────────────────────────────

    public void ShowMainMenu()
    {
        CurrentScreen = Screen.MainMenu;
        mainMenuUI?.Show();
        levelSelectUI?.Hide();
        gameController?.HideGame();
    }

    public void ShowLevelSelect()
    {
        CurrentScreen = Screen.LevelSelect;
        mainMenuUI?.Hide();
        levelSelectUI?.Show(allLevels);
        gameController?.HideGame();
    }

    public void StartLevel(LevelDataSO level)
    {
        ActiveLevel   = level;
        CurrentScreen = Screen.Gameplay;
        mainMenuUI?.Hide();
        levelSelectUI?.Hide();
        gameController?.LoadLevel(level);
    }

    public void BackToLevelSelect()
    {
        gameController?.HideGame();
        ShowLevelSelect();
    }

    public void OnLevelComplete(LevelDataSO level)
    {
        SaveManager.Instance.MarkCompleted(level);
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
        return (idx >= 0 && idx + 1 < allLevels.Length) ? allLevels[idx + 1] : null;
    }
}
