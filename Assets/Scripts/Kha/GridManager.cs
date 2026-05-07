using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public float cellSize = 1f;
    public Vector2Int boardSize = new Vector2Int(5, 5);

    // Lưu trữ trạng thái ô: 0 = Trống, 1 = Đen, 2 = Chặn
    private int[,] _grid;

    private void Awake()
    {
        Instance = this;
        _grid = new int[boardSize.x, boardSize.y];
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - transform.position.x) / cellSize);
        int y = Mathf.RoundToInt((worldPos.y - transform.position.y) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0) + transform.position;
    }

    public bool CanPlacePiece(List<Vector2Int> cells, Vector2Int startPos)
    {
        foreach (var cell in cells)
        {
            Vector2Int target = startPos + cell;
            if (target.x < 0 || target.x >= boardSize.x || target.y < 0 || target.y >= boardSize.y) return false;
            if (_grid[target.x, target.y] != 0) return false; // Đã có block hoặc bị chặn
        }
        return true;
    }
}