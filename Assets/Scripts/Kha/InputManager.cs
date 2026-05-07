using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [Header("Settings")]
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private Camera mainCamera;

    private IDraggable _selectedPiece;
    private Vector3 _dragOffset;
    private float _zDistance;

    private void Awake() => Instance = this;

    public void OnPointerAction(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();

        if (context.started) StartDrag(screenPos);
        else if (context.performed) ContinueDrag(screenPos);
        else if (context.canceled) EndDrag();
    }

    private void StartDrag(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableLayer))
        {
            _selectedPiece = hit.collider.GetComponent<IDraggable>();
            if (_selectedPiece != null)
            {
                _zDistance = Vector3.Distance(mainCamera.transform.position, hit.point);
                _dragOffset = _selectedPiece.GetTransform().position - GetWorldPos(screenPos);
                _selectedPiece.OnDragStart(GetWorldPos(screenPos));
            }
        }
    }

    private void ContinueDrag(Vector2 screenPos)
    {
        if (_selectedPiece == null) return;
        _selectedPiece.OnDragging(GetWorldPos(screenPos) + _dragOffset);
    }

    private void EndDrag()
    {
        if (_selectedPiece == null) return;
        _selectedPiece.OnDragEnd(_selectedPiece.GetTransform().position);
        _selectedPiece = null;
    }

    public Vector3 GetWorldPos(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        return ray.GetPoint(_zDistance);
    }
}