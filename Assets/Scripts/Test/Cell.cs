using UnityEngine;

public class Cell : MonoBehaviour
{
    // Board sẽ gọi hàm này khi Piece được đặt lên thành công
    public void SetState(bool isActive)
    {
        GetComponent<SpriteRenderer>().color = isActive ? Color.red : Color.white;
    }

    public Color GetColor()
    {
        return GetComponent<SpriteRenderer>().color;
    }
}