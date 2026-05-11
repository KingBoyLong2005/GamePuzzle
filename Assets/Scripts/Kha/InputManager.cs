using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private Camera mainCamera;

    private IDraggable _selectedPiece;
    private Vector3 _dragOffset;

    private void Awake() => Instance = this;
    private void Update()
    {
        // NẾU ĐANG CÓ MẢNH GHÉP ĐƯỢC CHỌN -> CẬP NHẬT VỊ TRÍ LIÊN TỤC
        if (_selectedPiece != null)
        {
            // Lấy vị trí chuột trực tiếp từ hệ thống (không phụ thuộc Action)
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(mainCamera.transform.position.z)));

            // Di chuyển mảnh ghép (kèm offset để không bị giật tâm)
            _selectedPiece.OnDragging(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0) + _dragOffset);
        }
    }
    public void OnPointerAction(InputAction.CallbackContext context)
    {
        if (mainCamera == null) return; 
        Vector2 screenPos = Pointer.current.position.ReadValue();
        // Chuyển tọa độ màn hình sang thế giới (World Point)
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
        Vector2 mousePos2D = new Vector2(worldPos.x, worldPos.y);

        if (context.started)    
        {
            Debug.Log("Pointer Down at: " + worldPos);
            // Bắn tia 2D kiểm tra vật thể
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, draggableLayer);
            if (hit.collider != null)
            {
                _selectedPiece = hit.collider.GetComponent<IDraggable>();
                if (_selectedPiece != null)
                {
                    _dragOffset = _selectedPiece.GetTransform().position - (Vector3)mousePos2D;
                    _selectedPiece.OnDragStart(worldPos);
                }
            }
        }
        else if (context.performed)
        {
            Debug.Log("Pointer Move at: " + worldPos);
            if (_selectedPiece != null)
            {
                _selectedPiece.OnDragging((Vector3)mousePos2D + _dragOffset);
            }
        }
        else if (context.canceled)
        {
            Debug.Log("Pointer Up at: " + worldPos);
            if (_selectedPiece != null)
            {
                _selectedPiece.OnDragEnd(worldPos); // Gọi hàm này để Snap vào lưới
                _selectedPiece = null; // Xóa tham chiếu để không dính theo chuột nữa
            }
        }
    }
}