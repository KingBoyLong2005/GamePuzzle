using UnityEngine;
using System.Collections.Generic;

public class DraggablePiece : MonoBehaviour, IDraggable
{
    [Header("Piece Settings")]
    [SerializeField] private int pieceID; // Dùng để check đúng ô (Type 1.1A)
    [SerializeField] private List<Vector2Int> occupiedCells; // Hình dạng mảnh ghép

    private Vector3 _originalPosition;
    private bool _isPlaced = false;

    private void Start()
    {
        // Lưu lại vị trí ban đầu để quay về nếu đặt sai
        _originalPosition = transform.position;
    }

    // --- Thực thi Interface IDraggable ---

    public void OnDragStart(Vector3 worldPos)
    {
        // Hiệu ứng nhấc mảnh ghép lên (Juice)
        transform.localScale = Vector3.one * 1.1f;
        _isPlaced = false;

        // Đưa lên trên cùng để không bị các mảnh khác che
        transform.position += Vector3.back * 0.5f;
    }

    public void OnDragging(Vector3 worldPos)
    {
        // Di chuyển mảnh ghép theo ngón tay/chuột
        transform.position = worldPos;

        // NÂNG CAO: Bạn có thể thêm logic Ghost Preview tại đây 
        // để hiển thị mảnh ghép mờ mờ trên lưới trước khi thả
    }

    public void OnDragEnd(Vector3 worldPos)
    {
        transform.localScale = Vector3.one;

        // Lấy tọa độ lưới gần nhất từ GridManager
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);

        // Kiểm tra xem vị trí này có hợp lệ không (có trống không, có bị chặn không)
        if (GridManager.Instance.CanPlacePiece(occupiedCells, gridPos))
        {
            // Hút vào tâm ô lưới (Snap)
            transform.position = GridManager.Instance.GridToWorld(gridPos);
            _isPlaced = true;

            // Thông báo cho hệ thống kiểm tra logic thắng (Nonogram Check)
            // GridManager.Instance.PlacePieceOnGrid(occupiedCells, gridPos);
        }
        else
        {
            // Nếu không hợp lệ, bay về vị trí cũ
            transform.position = _originalPosition;
        }
    }

    public Transform GetTransform() => transform;

    public List<Vector2Int> GetOccupiedCells(Vector2Int centerGridPos)
    {
        return occupiedCells;
    }

    // Hàm xoay mảnh ghép (Dùng cho Level Type 1.2, 2.2)
    public void RotatePiece()
    {
        for (int i = 0; i < occupiedCells.Count; i++)
        {
            int x = occupiedCells[i].x;
            int y = occupiedCells[i].y;
            occupiedCells[i] = new Vector2Int(y, -x);
        }
        // Cập nhật lại hình ảnh hiển thị (Visuals) ở đây
    }
}