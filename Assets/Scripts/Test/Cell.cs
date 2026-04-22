using UnityEngine;
using UnityEngine.InputSystem;

public class Cell : MonoBehaviour
{
    bool isDown = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Update()
    {
        Vector3 worldPos = PointerWorldPos();
        if(PointDown())
        {
            OnDown(worldPos);
        }
    }

    bool PointDown()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return Mouse.current?.leftButton.wasPressedThisFrame ?? Input.GetMouseButtonDown(0);
    }

    void OnDown(Vector3 pos)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if(hit.collider != null && hit.collider.gameObject == this.gameObject && !isDown)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            isDown = true;
        }
        else if(hit.collider != null && hit.collider.gameObject == this.gameObject && isDown)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            isDown = false;
        }
    }
    Vector3 PointerWorldPos()
    {
        Vector2 screen;
        if(Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            screen = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            screen = Mouse.current.position.ReadValue();
        else
            screen = (Vector2)Input.mousePosition;
        
        var pos = Camera.main.ScreenToWorldPoint(screen);
        pos.z = 0;
        return pos;
    }
    public Color GetColor()
    {
        return GetComponent<SpriteRenderer>().color;
        
        // if (c == Color.red) return "Red";
        // if (c == Color.white) return "White";
        
        // return c.ToString(); // Trả về RGBA nếu là màu khác
    }
}
