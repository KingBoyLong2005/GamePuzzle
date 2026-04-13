using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ────────────────────────────────────────────────────────────
//  UIGameplay  —  toàn bộ UI trong màn chơi
//
//  Gán script này lên Canvas GameObject của màn gameplay.
//  Kéo tất cả references vào Inspector — KHÔNG tạo UI bằng code.
//
//  Cấu trúc Canvas gợi ý:
//    Canvas (UIGameplay)
//      ├─ LevelLabel       (TextMeshProUGUI)
//      ├─ HintButton       (Button)
//      ├─ RestartButton    (Button)
//      ├─ MenuButton       (Button)
//      ├─ WinPanel         (GameObject)
//      │    ├─ NextLevelButton    (Button)
//      │    └─ WinRestartButton   (Button)
//      └─ ToastPanel       (GameObject)
//           └─ ToastText   (TextMeshProUGUI)
// ────────────────────────────────────────────────────────────
public class UIGameplay : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] TextMeshProUGUI levelLabel;

    [Header("Nút điều khiển")]
    [SerializeField] Button hintButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;
    [SerializeField] GameObject UIGame;
    [SerializeField] GameObject PieceSpawn;

    [Header("Win Panel")]
    [SerializeField] GameObject winPanel;
    [SerializeField] Button     nextLevelButton;
    [SerializeField] Button     winRestartButton;

    [Header("Toast thông báo")]
    [SerializeField] GameObject      toastPanel;
    [SerializeField] TextMeshProUGUI toastText;

    [Header("Controller — kéo GameController vào đây")]
    [SerializeField] GameController controller;

    void Awake()
    {
        hintButton.onClick.AddListener(()      => controller.RequestHint());
        restartButton.onClick.AddListener(()   => controller.RestartLevel());
        menuButton.onClick.AddListener(()      => 
        {
            GameManager.Instance.BackToLevelSelect();
            HideUIGame();
        });
        nextLevelButton.onClick.AddListener(() => controller.NextLevel());
        winRestartButton.onClick.AddListener(() => controller.RestartLevel());
        PieceSpawn.SetActive(false);
    }

    // ── Setup (gọi mỗi khi load level) ───────────────────────

    public void Setup(int levelNumber)
    {
        levelLabel.text = $"Level {levelNumber}";
        winPanel.SetActive(false);
        toastPanel.SetActive(false);
    }

    // ── Win ───────────────────────────────────────────────────

    public void ShowWinPanel() => winPanel.SetActive(true);

    // ── Toast ─────────────────────────────────────────────────

    public void ShowToast(string message, float duration = 2.5f)
    {
        StopAllCoroutines();
        StartCoroutine(ToastRoutine(message, duration));
    }

    IEnumerator ToastRoutine(string message, float duration)
    {
        toastText.text = message;
        toastPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        toastPanel.SetActive(false);
    }

    public void ShowUIGame()
    {
        UIGame.SetActive(true);
        PieceSpawn.SetActive(true);
    }
    public void HideUIGame()
    {
        UIGame.SetActive(false);
        PieceSpawn.SetActive(false);
    }
}
