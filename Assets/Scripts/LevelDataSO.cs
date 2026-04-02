using UnityEngine;

// ────────────────────────────────────────────────────────────
//  Tạo asset: chuột phải trong Project → Create → Puzzle/Level Data
// ────────────────────────────────────────────────────────────

[CreateAssetMenu(menuName = "Puzzle/Level Data", fileName = "Level_01")]
public class LevelDataSO : ScriptableObject
{
    [Header("Board")]
    public int rows = 4;
    public int cols = 4;

    [Header("Color Mode (level khó)")]
    public bool colorMode = false;

    [Header("Row Clues")]
    public int[]   rowCounts;
    public Color[] rowColors;   // chỉ dùng khi colorMode = true

    [Header("Column Clues")]
    public int[]   colCounts;
    public Color[] colColors;   // chỉ dùng khi colorMode = true

    [Header("Pieces")]
    public PieceShapeData[] pieces;

    [Header("Solution (dùng cho Hint overlay)")]
    [Tooltip("Mảng bool row-major, length = rows * cols")]
    public bool[] solution;

    [Tooltip("Mảng màu index tương ứng solution (1=Red,2=Blue,3=Green,4=Yellow,5=Purple). Chỉ dùng khi colorMode=true")]
    public int[]  solutionColorIdx;

    [Header("Display")]
    [Tooltip("Tên hiển thị trên màn hình chọn level")]
    public string displayName = "Level";
    public Sprite thumbnail;        // tuỳ chọn, hiển thị preview
}

// ────────────────────────────────────────────────────────────
//  Piece Shape — định nghĩa hình dạng một mảnh
// ────────────────────────────────────────────────────────────

[System.Serializable]
public class PieceShapeData
{
    public string      pieceName = "Piece";
    public Color       color     = Color.white;

    [Tooltip("Các offset (col, row) tương đối từ pivot. +X=phải, +Y=xuống")]
    public Vector2Int[] cells;

    [Tooltip("Prefab piece có sẵn sprite/màu. Nếu để trống → tự tạo bằng code dùng Color ở trên.")]
    public GameObject  prefab;
}