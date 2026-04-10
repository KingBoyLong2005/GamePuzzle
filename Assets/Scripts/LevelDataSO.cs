using System.Collections.Generic;
using UnityEngine;

// Tạo asset: chuột phải → Create → Puzzle/Level Data
[CreateAssetMenu(menuName = "Puzzle/Level Data", fileName = "Level_01")]
public class LevelDataSO : ScriptableObject
{
    [Header("Kích thước bảng")]
    public int rows = 4;
    public int cols = 4;

    [Header("Chế độ màu (level khó hơn)")]
    public bool colorMode;

    [Header("Gợi ý hàng — số ô cần điền mỗi hàng")]
    public int[]   rowCounts;
    public Color[] rowColors;   // chỉ dùng khi colorMode = true

    [Header("Gợi ý cột — số ô cần điền mỗi cột")]
    public int[]   colCounts;
    public Color[] colColors;   // chỉ dùng khi colorMode = true

    [Header("Các mảnh cần đặt")]
    public PieceData[] pieces;

    [Header("Lời giải (dùng cho tính năng Hint)")]
    [Tooltip("Mảng bool row-major, length = rows × cols")]
    public bool[] solution;
    [Tooltip("Màu ô lời giải: 1=Đỏ 2=Lam 3=Lục 4=Vàng 5=Tím. Chỉ dùng khi colorMode=true")]
    public int[]  solutionColors;

    [Header("Hiển thị")]
    public string displayName = "Level";
    public Sprite thumbnail;
}

// ────────────────────────────────────────────────────────────
//  PieceData  —  hình dạng và màu của một mảnh ghép
//
//  Hình dạng lưu dưới dạng LƯỚI 2D:
//    shapeRows × shapeCols ô, mỗi ô bool (tick = thuộc mảnh)
//    Ví dụ: mảnh L 3×2
//      shapeCells = [ true, false,
//                     true, false,
//                     true, true  ]
// ────────────────────────────────────────────────────────────
[System.Serializable]
public class PieceData
{
    public string pieceName = "Piece";
    public Color  color     = Color.white;

    [Header("Hình dạng (lưới 2D — tick ô thuộc mảnh)")]
    public int    shapeRows = 3;
    public int    shapeCols = 3;
    [Tooltip("Mảng bool row-major, length = shapeRows × shapeCols")]
    public bool[] shapeCells;

    /// <summary>
    /// Trả về danh sách offset (col, row) của các ô được tick.
    /// Gốc (0,0) = góc trên-trái. +col = phải, +row = xuống.
    /// </summary>
    public Vector2Int[] GetCellOffsets()
    {
        var offsets = new List<Vector2Int>();
        if (shapeCells == null) return offsets.ToArray();

        for (int r = 0; r < shapeRows; r++)
        for (int c = 0; c < shapeCols; c++)
        {
            int idx = r * shapeCols + c;
            if (idx < shapeCells.Length && shapeCells[idx])
                offsets.Add(new Vector2Int(c, r));
        }
        return offsets.ToArray();
    }
}
