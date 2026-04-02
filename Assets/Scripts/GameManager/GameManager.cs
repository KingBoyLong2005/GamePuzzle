using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public LevelDataSO[] allLevels;

    [Header("UI và GameController")]
    private UIMainMenu    uiMain;
    private UILevelSelect uiLevelSelect;
    private GameController gameCtrl;
    private BoardManager boardManager;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiMain        = FindFirstObjectByType<UIMainMenu>(FindObjectsInactive.Include);
        uiLevelSelect = FindFirstObjectByType<UILevelSelect>(FindObjectsInactive.Include);
        // gameCtrl      = FindFirstObjectByType<GameController>(FindObjectsInactive.Include);
    
        boardManager  = FindFirstObjectByType<BoardManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMainMenu()
    {
        uiMain?.Show();
        uiLevelSelect?.Hide();
        // gameCtrl?.HideGame();
    }
    public void ShowLevelSelect()
    {
        // CurrentScreen = Screen.LevelSelect;
        uiMain?.Hide();
        uiLevelSelect.Show(allLevels);
        // gameCtrl?.HideGame();
    }
    public void StartLevel(LevelDataSO levelData)
    {
        // CurrentScreen = Screen.Game;
        uiMain?.Hide();
        uiLevelSelect?.Hide();
        boardManager.LoadLevel(levelData);
    }
}
