using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ────────────────────────────────────────────────────────────
//  GameController  —  điều phối gameplay
//
//  Trách nhiệm:
//    - Load / unload level (gọi các manager)
//    - Lắng nghe event từ PieceManager (đặt/gỡ mảnh)
//    - Kiểm tra thắng và chạy win sequence
//    - Xử lý Hint
//    - Điều khiển camera
//
//  KHÔNG chứa logic bảng, mảnh, hay UI trực tiếp.
//  Tất cả giao tiếp qua các manager và UIGameplay.
//
//  Gán script này lên Gameplay Root GO trong scene.
//  Kéo các child manager vào Inspector.
// ────────────────────────────────────────────────────────────
public class GameController : MonoBehaviour
{
    [Header("Các manager con — kéo từ child objects")]
    [SerializeField] BoardManager board;
    [SerializeField] ClueManager  clues;
    [SerializeField] PieceManager pieces;
    [SerializeField] UIGameplay   ui;

    [Header("Prefab overlay hint (SpriteRenderer vuông)")]
    [SerializeField] GameObject hintCellPrefab;

    LevelDataSO currentLevel;

    // ── Load / Unload ─────────────────────────────────────────

    public void LoadLevel(LevelDataSO level)
    {
        currentLevel = level;
        gameObject.SetActive(true);
        SetupCamera();

        board.Build(level);
        clues.Build(level, board);
        pieces.SpawnPieces(level);

        // Đăng ký event mỗi lần load (Clear() trước đó đã huỷ subcription cũ)
        pieces.OnPiecePlaced   += HandlePiecePlaced;
        pieces.OnPieceUnplaced += HandlePieceUnplaced;

        int levelNumber = GameManager.Instance
            ? GameManager.Instance.GetLevelIndex(level) + 1 : 1;
        ui.Setup(levelNumber);
    }

    public void HideGame()
    {
        // Huỷ đăng ký trước khi clear để tránh event rò
        pieces.OnPiecePlaced   -= HandlePiecePlaced;
        pieces.OnPieceUnplaced -= HandlePieceUnplaced;

        board.Clear();
        clues.Clear();
        pieces.Clear();
        gameObject.SetActive(false);
    }

    // ── Handlers event từ PieceManager ───────────────────────

    void HandlePiecePlaced()
    {
        clues.Refresh();
        if (clues.AllCluesSatisfied())
            StartCoroutine(WinSequence());
    }

    void HandlePieceUnplaced() => clues.Refresh();

    // ── Actions từ UIGameplay ─────────────────────────────────

    public void RestartLevel()
    {
        StopAllCoroutines();
        LoadLevel(currentLevel);
    }

    public void NextLevel()
    {
        var next = GameManager.Instance?.GetNextLevel(currentLevel);
        if (next != null) GameManager.Instance.StartLevel(next);
        else              GameManager.Instance?.BackToLevelSelect();
    }

    public void RequestHint()
    {
        // TODO: thay dòng dưới bằng AdManager.Instance.ShowRewardedAd(ShowHint)
        //       khi tích hợp quảng cáo
        ShowHint();
        ui.ShowToast("Gợi ý hiện trong 6 giây!");
    }

    // ── Win sequence ──────────────────────────────────────────

    IEnumerator WinSequence()
    {
        yield return new WaitForSeconds(0.4f);
        board.PlayWinAnimation();

        yield return new WaitForSeconds(0.6f);
        GameManager.Instance?.OnLevelComplete(currentLevel);
        ui.ShowWinPanel();
    }

    // ── Hint ──────────────────────────────────────────────────

    readonly List<Vector2Int> hintCells = new();
    bool hintActive;

    void ShowHint()
    {
        if (hintActive || currentLevel.solution == null) return;
        hintActive = true;

        Color hintGray = new Color(0.55f, 0.55f, 0.55f, 1f);

        for (int r = 0; r < currentLevel.rows; r++)
        for (int c = 0; c < currentLevel.cols; c++)
        {
            int idx = r * currentLevel.cols + c;
            if (idx >= currentLevel.solution.Length || !currentLevel.solution[idx]) continue;

            board.SetCellHintColor(r, c, hintGray);
            hintCells.Add(new Vector2Int(c, r));
        }

        StartCoroutine(ClearHintAfterDelay(6f));
    }

    IEnumerator ClearHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var cell in hintCells)
            board.ClearCellHintColor(cell.y, cell.x);

        hintCells.Clear();
        hintActive = false;
    }

    // ── Camera ────────────────────────────────────────────────

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.orthographic       = true;
        cam.orthographicSize   = 6f;
        cam.backgroundColor    = new Color(0.13f, 0.18f, 0.28f);
        cam.transform.position = new Vector3(0, 0, -10);
    }
}
