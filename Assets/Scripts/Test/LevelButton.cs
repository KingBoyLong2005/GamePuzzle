using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText;  // Text hiển thị "Level 1", "Level 2" ...

    private LevelBoardData levelData;
    private Action<LevelBoardData> onSelected;

    // ── Được LevelSelectManager gọi sau khi Instantiate ───────────
    public void Setup(int levelIndex, LevelBoardData data, Action<LevelBoardData> callback)
    {
        levelData  = data;
        onSelected = callback;

        if (labelText != null)
            labelText.text = $"Level {levelIndex}";

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        onSelected?.Invoke(levelData);
    }

    private void OnDestroy()
    {
        // Tránh memory leak khi button bị Destroy
        GetComponent<Button>().onClick.RemoveListener(OnClick);
    }
}