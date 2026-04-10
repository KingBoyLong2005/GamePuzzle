using System.Collections.Generic;
using UnityEngine;

// Lưu/đọc tiến trình qua PlayerPrefs (JSON).
// Singleton tồn tại suốt game.
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    const string SAVE_KEY = "PuzzleSave";

    [System.Serializable]
    class SaveData
    {
        // Lưu tên asset (ScriptableObject.name) của level đã hoàn thành
        public List<string> completedLevelNames = new();
    }

    SaveData saveData;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SaveManager] Save path: " + Application.persistentDataPath);
        LoadFromDisk();
    }

    // ── Public API ────────────────────────────────────────────

    public void MarkCompleted(LevelDataSO level)
    {
        if (saveData.completedLevelNames.Contains(level.name)) return;
        saveData.completedLevelNames.Add(level.name);
        SaveToDisk();
    }

    public bool IsCompleted(LevelDataSO level) =>
        saveData.completedLevelNames.Contains(level.name);

    /// Xoá toàn bộ save (dùng cho debug).
    public void ResetAll()
    {
        saveData = new SaveData();
        SaveToDisk();
        Debug.Log("[SaveManager] Đã reset save.");
    }

    // ── Internal ──────────────────────────────────────────────

    void SaveToDisk()
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    void LoadFromDisk()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        saveData = string.IsNullOrEmpty(json)
            ? new SaveData()
            : JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
    }
}
