using UnityEngine;

// ────────────────────────────────────────────────────────────
//  BoardCell  —  gắn lên Cell Prefab của bảng
//
//  Cấu trúc prefab:
//    Root (BoardCell, SpriteRenderer — nền ô)
//      └─ Fill (SpriteRenderer — màu khi mảnh đặt vào)
// ────────────────────────────────────────────────────────────
public class BoardCell : MonoBehaviour
{
    [SerializeField] SpriteRenderer fillRenderer;

    public Color CurrentColor => fillRenderer.color;

    Color defaultColor;   // màu gốc của Fill trong prefab

    void Awake()
    {
        defaultColor = fillRenderer.color;
    }

    public void SetColor(Color color) => fillRenderer.color = color;

    // Khôi phục màu gốc của prefab thay vì set Color.clear
    public void ClearColor() => fillRenderer.color = defaultColor;
}