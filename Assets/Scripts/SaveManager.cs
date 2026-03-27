using System;
using System.Collections.Generic;
using UnityEngine;

// ────────────────────────────────────────────────────────────
//  SaveManager  —  Singleton, tồn tại suốt game
//
//  Lưu trạng thái hoàn thành từng level vào PlayerPrefs dạng JSON.
//  Key: "PuzzleSave"
// ────────────────────────────────────────────────────────────

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    const string SAVE_KEY = "PuzzleSave";

    [System.Serializable]
    class SaveData
    {
        public List<string> completedLevels = new();  // lưu tên asset (name) của LevelDataSO
    }

    SaveData data;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>Đánh dấu level đã hoàn thành và lưu.</summary>
    public void MarkCompleted(LevelDataSO level)
    {
        string key = level.name;
        if (!data.completedLevels.Contains(key))
        {
            data.completedLevels.Add(key);
            Save();
        }
    }

    /// <summary>Kiểm tra level đã hoàn thành chưa.</summary>
    public bool IsCompleted(LevelDataSO level) =>
        data.completedLevels.Contains(level.name);

    /// <summary>Xoá toàn bộ save (dùng cho debug).</summary>
    public void ResetAll()
    {
        data = new SaveData();
        Save();
        Debug.Log("[SaveManager] Save đã được reset.");
    }

    // ── Internal ─────────────────────────────────────────────

    void Save()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    void Load()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        data = string.IsNullOrEmpty(json)
            ? new SaveData()
            : JsonUtility.FromJson<SaveData>(json);

        if (data == null) data = new SaveData();
        Debug.Log($"[SaveManager] Loaded. Completed: {data.completedLevels.Count} levels.");
    }
}
