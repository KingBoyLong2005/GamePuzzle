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
            Vector3 targetPos = mouseWorldPos + _dragOffset;
            targetPos.z = -1f;

            _selectedPiece.OnDragging(targetPos);
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
                _selectedPiece = hit.collider.GetComponentInParent<IDraggable>();
                if (_selectedPiece != null)
                {
                    Debug.Log($"[InputManager] Đã bắt trúng mảnh: {hit.collider.transform.parent.name}");
                    _dragOffset = _selectedPiece.GetTransform().position - (Vector3)mousePos2D;
                    _dragOffset.z = 0f; // Đảm bảo offset chỉ ảnh hưởng đến X và Y
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
                _selectedPiece.OnDragEnd(_selectedPiece.GetTransform().position); // Gọi hàm này để Snap vào lưới
                _selectedPiece = null; // Xóa tham chiếu để không dính theo chuột nữa
            }
        }
    }
}