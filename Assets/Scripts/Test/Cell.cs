using UnityEngine;

public class Cell : MonoBehaviour
{
    public int x;
    public int y;
    public bool isFilled = false;
    // Board sẽ gọi hàm này khi Piece được đặt lên thành công
    public void SetState(bool filled)
    {
        isFilled = filled;

        // (Tùy chọn) Đổi màu sắc để trực quan hóa việc ô đã bị chiếm
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = filled ? Color.red : Color.white;
        }
    }

    public Color GetColor()
    {
        return GetComponent<SpriteRenderer>().color;
    }
}