using UnityEngine;

public class Cell : MonoBehaviour
{
    public int x;
    public int y;
    public bool isFilled = false;
    private Color originalColor;
    // Board sẽ gọi hàm này khi Piece được đặt lên thành công
    private void Awake()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
    }
    public void SetState(bool filled)
    {
        isFilled = filled;

        // (Tùy chọn) Đổi màu sắc để trực quan hóa việc ô đã bị chiếm
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = filled ? Color.red : originalColor;
        }
    }

    public Color GetColor()
    {
        return GetComponent<SpriteRenderer>().color;
    }
}