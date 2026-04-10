using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ────────────────────────────────────────────────────────────
//  LevelCard  —  gắn lên LevelCard Prefab
//
//  UILevelSelect sẽ Instantiate prefab này và gọi Setup().
//
//  Cấu trúc prefab gợi ý:
//    Root (LevelCard, Image, Button)
//      ├─ NumberLabel  (TextMeshProUGUI — "1", "2", ...)
//      ├─ NameLabel    (TextMeshProUGUI — tên level)
//      ├─ GridSizeLabel (TextMeshProUGUI — "4 × 4")
//      ├─ StarIcon     (GameObject — hiện khi đã hoàn thành)
//      └─ ColorBadge   (GameObject — hiện khi colorMode)
// ────────────────────────────────────────────────────────────
public class LevelCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI numberLabel;
    [SerializeField] TextMeshProUGUI nameLabel;
    [SerializeField] TextMeshProUGUI gridSizeLabel;
    [SerializeField] GameObject      starIcon;        // active khi level đã hoàn thành
    [SerializeField] GameObject      colorModeBadge;  // active khi colorMode = true
    [SerializeField] Button          cardButton;
    [SerializeField] Image           cardBackground;

    [Header("Màu nền card")]
    [SerializeField] Color normalColor    = new Color(0.16f, 0.20f, 0.32f);
    [SerializeField] Color completedColor = new Color(0.12f, 0.28f, 0.18f);

    public void Setup(int number, LevelDataSO data, bool completed)
    {
        numberLabel.text   = number.ToString();
        nameLabel.text     = string.IsNullOrEmpty(data.displayName)
                             ? $"Level {number}" : data.displayName;
        gridSizeLabel.text = $"{data.rows} × {data.cols}";

        starIcon.SetActive(completed);
        colorModeBadge.SetActive(data.colorMode);
        cardBackground.color = completed ? completedColor : normalColor;

        // Xoá listener cũ trước khi thêm (tránh duplicate khi reuse)
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(() => 
        {
            GameManager.Instance.StartLevel(data);
            FindFirstObjectByType<UIGameplay>().ShowUIGame();
        });
    }
}
