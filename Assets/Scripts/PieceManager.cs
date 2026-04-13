using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ────────────────────────────────────────────────────────────
//  PieceManager  —  quản lý toàn bộ mảnh ghép trong level
// ────────────────────────────────────────────────────────────
public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance { get; private set; }
    [Header("Prefab ô mảnh — SpriteRenderer + BoxCollider2D")]
    [SerializeField] public GameObject pieceCellPrefab;

    [Header("Content của WorldScrollView — piece spawn vào đây")]
    [SerializeField] public Transform piecesSpawn;

    [Header("WorldScrollView — để báo khi đang drag piece")]
    [SerializeField] public WorldScrollView worldScrollView;



    public List<PieceInstance> allPieces = new();


    public BoardManager board;
    public LevelDataSO  level;
    private HandleInput handleInput;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        handleInput = FindFirstObjectByType<HandleInput>();
    }


    public void SpawnPieces(LevelDataSO levelData)
    {
        Clear();
        level = levelData;
        board = BoardManager.Instance;

        if (level.pieces == null || level.pieces.Length == 0) return;
        LayoutAndSpawnAllPieces();
    }

    public void Clear()
    {
        foreach (var p in allPieces) p.Destroy();
        allPieces.Clear();
        handleInput.heldPiece  = null;
        handleInput.isDragging = false;
        level      = null;
    }

    void LayoutAndSpawnAllPieces()
    {
        float step   = board.CellStep;
        Bounds bounds = piecesSpawn.GetComponent<SpriteRenderer>().bounds;

        float left    = bounds.min.x;
        float right   = bounds.max.x;
        float startY  = bounds.max.y - 0.5f;

        float x       = left;
        float y       = startY;
        float rowMaxH = 0f;

        foreach (var pieceData in level.pieces)
        {
            var   offsets = pieceData.GetCellOffsets();
            float pieceW  = (MaxX(offsets) + 1) * step;
            float pieceH  = (MaxY(offsets) + 1) * step;

            if (x + pieceW > right && x > left)
            {
                x       = left;
                y      -= rowMaxH + 0.5f;
                rowMaxH = 0f;
            }

            var homePos = new Vector3(x + pieceW * 0.5f, y - pieceH * 0.5f, 0);

            // Spawn piece là con của piecesSpawn (content) → di chuyển cùng scroll
            allPieces.Add(new PieceInstance(
                pieceData, homePos, pieceCellPrefab,
                contentParent: piecesSpawn,
                detachedParent: transform,
                step, board.CellSize));

            x      += pieceW + 0.5f;
            rowMaxH = Mathf.Max(rowMaxH, pieceH);
        }
    }


    static int MaxX(Vector2Int[] offsets)
    { int m = 0; foreach (var o in offsets) m = Mathf.Max(m, o.x); return m; }

    static int MaxY(Vector2Int[] offsets)
    { int m = 0; foreach (var o in offsets) m = Mathf.Max(m, o.y); return m; }

    
}