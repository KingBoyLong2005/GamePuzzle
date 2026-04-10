using UnityEngine;
using UnityEngine.UI;

// ────────────────────────────────────────────────────────────
//  UIMainMenu  —  màn hình chính
//
//  Gán script này lên Root Canvas của Main Menu.
//  Toàn bộ visual dựng trong Prefab/Scene — script chỉ
//  wire sự kiện nút và xử lý Show/Hide.
// ────────────────────────────────────────────────────────────
public class UIMainMenu : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button ResetBtn;

    void Awake()
    {
        playButton.onClick.AddListener(() => GameManager.Instance.ShowLevelSelect());
        ResetBtn.onClick.AddListener(() => SaveManager.Instance.ResetAll());
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
