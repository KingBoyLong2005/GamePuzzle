using UnityEngine;
using UnityEngine.UI;

// ────────────────────────────────────────────────────────────
//  UILevelSelect  —  màn hình chọn level
//
//  Gán script này lên Root Canvas của màn hình chọn level.
//  Kéo tất cả references vào Inspector.
//
//  Cấu trúc Canvas gợi ý:
//    Canvas (UILevelSelect)
//      ├─ Header
//      │    ├─ Title (Text)
//      │    └─ BackButton (Button)
//      └─ ScrollView
//           └─ Viewport
//                └─ Content (kéo vào cardContainer — có GridLayoutGroup)
// ────────────────────────────────────────────────────────────
public class UILevelSelect : MonoBehaviour
{
    [SerializeField] Button    backButton;
    [SerializeField] Transform cardContainer;  // Content của ScrollView, có GridLayoutGroup
    [SerializeField] LevelCard cardPrefab;

    void Awake()
    {
        backButton.onClick.AddListener(() => GameManager.Instance.ShowMainMenu());
    }

    public void Show(LevelDataSO[] levels)
    {
        gameObject.SetActive(true);
        SpawnCards(levels);
    }

    public void Hide() => gameObject.SetActive(false);

    void SpawnCards(LevelDataSO[] levels)
    {
        // Xoá card cũ
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < levels.Length; i++)
        {
            bool completed = SaveManager.Instance.IsCompleted(levels[i]);
            var  card      = Instantiate(cardPrefab, cardContainer);
            card.Setup(i + 1, levels[i], completed);
        }
    }
}
