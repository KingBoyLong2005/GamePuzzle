using UnityEngine;
using UnityEngine.InputSystem;

public class WorldScrollView : MonoBehaviour
{
    // public Vector3 targetPos = new Vector3(1, 0, 0);
    // Vector3 originalPos;
    // bool isMoved = false;

    public Transform content;

    public float minY;
    public float maxY;

    Vector3 lastPointerPos;
    bool isDragging    = false;
    bool pieceDragging = false;   // true khi PieceManager đang kéo piece

    // void Start()
    // {
    //     originalPos = transform.position;
    // }

    void Update()
    {
        // Khi đang kéo piece → khoá toàn bộ input của scroll
        if (pieceDragging) return;

        if (PointerDown())
        {
            if (IsPointerOver())
            {
                isDragging     = true;
                lastPointerPos = PointerWorldPos();
            }
        }

        if (PointerHeld() && isDragging)
        {
            Vector3 current = PointerWorldPos();
            float deltaY    = current.y - lastPointerPos.y;
            MoveContent(deltaY);
            lastPointerPos  = current;
        }

        if (PointerReleased())
        {
            isDragging = false;
        }

        // if (PointerDown())
        // {
        //     if (!IsPointerOverItem())
        //     {
        //         HandleClick();
        //     }
        // }
    }

    // ── Gọi từ PieceManager ───────────────────────────────────

    // Khi bắt đầu drag piece: scroll MoveBack + khoá input scroll
    public void OnPieceDragStart()
    {
        pieceDragging = true;
        isDragging    = false;

        // if (isMoved)
        //     MoveBack();
    }

    // Khi thả piece: mở lại input scroll
    public void OnPieceDragEnd()
    {
        pieceDragging = false;
    }

    // ── Internal ──────────────────────────────────────────────

    // void HandleClick()
    // {
    //     Vector3 worldPos  = PointerWorldPos();
    //     RaycastHit2D hit  = Physics2D.Raycast(worldPos, Vector2.zero);

    //     if (!isMoved)
    //     {
    //         if (hit.collider != null && hit.collider.gameObject == gameObject)
    //             MoveToTarget();
    //     }
    //     else
    //     {
    //         if (hit.collider == null || hit.collider.gameObject != gameObject)
    //             MoveBack();
    //     }
    // }

    // void MoveToTarget()
    // {
    //     transform.position = targetPos;
    //     isMoved            = true;
    // }

    // void MoveBack()
    // {
    //     transform.position = originalPos;
    //     isMoved            = false;
    // }

    void MoveContent(float deltaY)
    {
        Vector3 pos = content.position;
        pos.y       = Mathf.Clamp(pos.y + deltaY, minY, maxY);
        content.position = pos;
    }

    bool IsPointerOver()
    {
        RaycastHit2D hit = Physics2D.Raycast(PointerWorldPos(), Vector2.zero);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }

    // bool IsPointerOverItem()
    // {
    //     RaycastHit2D hit = Physics2D.Raycast(PointerWorldPos(), Vector2.zero);
    //     return hit.collider != null && hit.collider.CompareTag("Piece");
    // }

    // ── Input helpers ─────────────────────────────────────────

    Vector3 PointerWorldPos()
    {
        Vector2 screen;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            screen = Touchscreen.current.primaryTouch.position.ReadValue();
        else if (Mouse.current != null)
            screen = Mouse.current.position.ReadValue();
        else
            screen = (Vector2)Input.mousePosition;

        var pos = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
        pos.z = 0;
        return pos;
    }

    bool PointerDown()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return Mouse.current?.leftButton.wasPressedThisFrame ?? Input.GetMouseButtonDown(0);
    }

    bool PointerHeld()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.isPressed;
        return Mouse.current?.leftButton.isPressed ?? Input.GetMouseButton(0);
    }

    bool PointerReleased()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
        return Mouse.current?.leftButton.wasReleasedThisFrame ?? Input.GetMouseButtonUp(0);
    }
}