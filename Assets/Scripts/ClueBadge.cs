using TMPro;
using UnityEngine;

// ────────────────────────────────────────────────────────────
//  ClueBadge  —  gắn lên Clue Prefab
//
//  Cấu trúc prefab:
//    Root (ClueBadge, SpriteRenderer — nền badge)
//      └─ Label (TextMeshPro — số đếm)
//
//  Badge đổi màu theo trạng thái:
//    Đúng    → xanh lá
//    Vượt    → đỏ
//    Chưa đủ → màu gợi ý ban đầu
// ────────────────────────────────────────────────────────────
public class ClueBadge : MonoBehaviour
{
    [SerializeField] SpriteRenderer background;
    [SerializeField] TextMeshPro    countLabel;

    static readonly Color Satisfied = new(0.25f, 0.85f, 0.40f, 0.9f);
    static readonly Color Overfill  = new(0.95f, 0.25f, 0.25f, 0.9f);
    static readonly Color Default   = new(0.85f, 0.88f, 0.95f, 1f);

    Color clueColor;  // màu gợi ý (Color.white = không có màu)

    public void Setup(int targetCount, Color color)
    {
        clueColor       = color;
        countLabel.text = targetCount.ToString();
        ApplyIdleColor();
    }

    public void UpdateState(int current, int target)
    {
        if (current == target)
        {
            background.color = Satisfied;
            countLabel.color = new Color(0.05f, 0.35f, 0.10f);
        }
        else if (current > target)
        {
            background.color = Overfill;
            countLabel.color = Color.white;
        }
        else
        {
            ApplyIdleColor();
        }
    }

    void ApplyIdleColor()
    {
        bool hasColor = clueColor != Color.white;
        background.color = hasColor
            ? new Color(clueColor.r, clueColor.g, clueColor.b, 0.25f)
            : Default;
        countLabel.color = hasColor ? clueColor : new Color(0.1f, 0.1f, 0.15f);
    }
}
