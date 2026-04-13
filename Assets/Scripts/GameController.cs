using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ────────────────────────────────────────────────────────────
//  GameController  —  điều phối gameplay
// ────────────────────────────────────────────────────────────
public class GameController : MonoBehaviour
{
    [Header("Các manager con — kéo từ child objects")]
    [SerializeField] BoardManager board;
    [SerializeField] ClueManager  clues;
    [SerializeField] PieceManager pieces;
    [SerializeField] UIGameplay   ui;
    [SerializeField] HandleInput input;
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
        input = FindFirstObjectByType<HandleInput>();

        input.OnPiecePlaced   += HandlePiecePlaced;
        input.OnPieceUnplaced += HandlePieceUnplaced;

        int levelNumber = GameManager.Instance
            ? GameManager.Instance.GetLevelIndex(level) + 1 : 1;
        ui.Setup(levelNumber);
        FindFirstObjectByType<UIGameplay>().ShowUIGame();
    }

    public void HideGame()
    {
        input.OnPiecePlaced   -= HandlePiecePlaced;
        input.OnPieceUnplaced -= HandlePieceUnplaced;

        board.Clear();
        clues.Clear();
        pieces.Clear();
        ClearHint();
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
        ClearHint();
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
        // TODO: thay bằng AdManager.Instance.ShowRewardedAd(ShowHint)
        ShowHint();
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

    // ── Hint — hiện vĩnh viễn cho đến khi Restart / HideGame ─

    readonly List<Vector2Int> hintCells = new();
    bool hintActive;

    void ShowHint()
    {
        // Hint chỉ hiển thị một lần mỗi level
        if (hintActive || currentLevel.solution == null) return;
        hintActive = true;

        Color hintGray = new Color(0.55f, 0.55f, 0.55f, 1f);

        for (int r = 0; r < currentLevel.rows; r++)
        for (int c = 0; c < currentLevel.cols; c++)
        {
            int idx = r * currentLevel.cols + c;
            if (idx >= currentLevel.solution.Length || !currentLevel.solution[idx]) continue;

            board.SetCellHintColor(r, c, hintGray);
            hintCells.Add(new Vector2Int(c, r));  // x=col, y=row
        }
    }

    // Gọi khi Restart hoặc HideGame để xoá hint cũ
    void ClearHint()
    {
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