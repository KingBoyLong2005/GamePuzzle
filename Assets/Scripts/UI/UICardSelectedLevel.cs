using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICardSelectedLevel : MonoBehaviour
{
    public UnityEngine.UI.Image Card;
    public TMP_Text LevelNumberText;
    public TMP_Text NameLevelText;
    public TMP_Text GridSizeText;
    public TMP_Text CompletedText;
    public Button SelectBtn;
    public static Sprite _sprite;

    // public void BuildCard(GameObject parent, LevelDataSO level, int number, bool completed)
    // {
    //     // transform.SetParent(parent.transform, false);
    //     LevelNumberText.text = $"{number}";
    //     NameLevelText.text = string.IsNullOrEmpty(level.displayName) ? $"Level {number}" : level.displayName;
    //     GridSizeText.text = $"{level.rows} × {level.cols}";
    //     if (completed)
    //     {
    //         CompletedText.text = "Completed";
    //     }
    //     SelectBtn.onClick.AddListener(() => GameManagerTest.Instance.StartLevel(level));

    // }
    public void BuildCard(GameObject parent, LevelDataSO level, int number)
    {
        // transform.SetParent(parent.transform, false);
        LevelNumberText.text = $"{number}";
        NameLevelText.text = string.IsNullOrEmpty(level.displayName) ? $"Level {number}" : level.displayName;
        GridSizeText.text = $"{level.rows} × {level.cols}";
        // if (completed)
        // {
        //     CompletedText.text = "Completed";
        // }
        SelectBtn.onClick.AddListener(() => GameManager.Instance.StartLevel(level));
        Debug.Log($"LevelNumberText: '{LevelNumberText.text}', font: {LevelNumberText.font}, color: {LevelNumberText.color}");

    }
}