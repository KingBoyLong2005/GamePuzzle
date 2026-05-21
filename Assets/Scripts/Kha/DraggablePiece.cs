using UnityEngine;
using System.Collections.Generic;

public class DraggablePiece : MonoBehaviour, IDraggable
{
    [Header("Piece Settings")]
    [SerializeField] private int pieceID; // Dùng để check đúng ô (Type 1.1A)
    [SerializeField] public List<Vector2Int> occupiedCells; // Hình dạng mảnh ghép

    private Vector3 _originalPosition;
    private bool _isPlaced = false;

    private void Awake()
    {
        // Cấp lệnh cho Hệ thống vật lý của Unity cho phép cụm Object này di chuyển tự do qua Code
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
    }

    // --- Thực thi Interface IDraggable ---

    public void OnDragStart(Vector3 worldPos)
    {
        Debug.Log("Đã gắp mảnh ghép: " + gameObject.name);

        _isPlaced = false;

        // 1. Hiệu ứng phóng lớn một chút để người chơi thấy rõ mình đang cầm nó
        transform.localScale = Vector3.one * 1.1f;

        // 2. Thay đổi trục Z để mảnh ghép luôn đè lên trên các ô lưới (Board)
        // Giả sử Board của bạn ở Z = 0, thì Piece khi kéo nên ở Z = -1
        Vector3 newPos = transform.position;
        newPos.z = -1f;
        transform.position = newPos;

        // 3. (Tùy chọn) Đổi màu mờ đi một chút nếu muốn
        // GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0.8f);
    }

    public void OnDragging(Vector3 newPosition)
    {
        // Di chuyển mảnh ghép theo ngón tay/chuột
        //transform.position = worldPos;
        if (!_isPlaced)
        {
            // Di chuyển cả khối liền (Cấp 2) theo chuột, trục Z khóa ở -1
            transform.position = new Vector3(newPosition.x, newPosition.y, -1f);
        }
        Debug.Log("Đang kéo mảnh ghép: " + gameObject.name);

        // NÂNG CAO: Bạn có thể thêm logic Ghost Preview tại đây 
        // để hiển thị mảnh ghép mờ mờ trên lưới trước khi thả
    }
    
    public void OnDragEnd(Vector3 dummy)
    {
        transform.localScale = Vector3.one;
        if (_isPlaced)
        {
            Debug.Log("Mảnh ghép đã được đặt trước đó, không cần kiểm tra lại.");
            return;
        }
        Board board = Object.FindFirstObjectByType<Board>();
        if (board == null || transform.childCount == 0)
        {
            ReturnToQueue();
            return;
        }

        Transform anchorChild = transform.GetChild(0);
        Cell anchorCell = board.GetCellFromWorldPos(anchorChild.position);
        if (anchorCell == null)
        {
            Debug.Log("Khối Neo nằm ngoài bàn cờ!");
            ReturnToQueue();
            return;
        }
        Vector2Int anchorDataPos = occupiedCells[0];

        if (CanPlaceWholeBody(board, anchorCell, anchorDataPos))
        {
            Vector3 offsetToAnchor = transform.position - anchorChild.position;
            transform.position = new Vector3(anchorCell.transform.position.x + offsetToAnchor.x, anchorCell.transform.position.y + offsetToAnchor.y, -1f);

            foreach (Vector2Int posData in occupiedCells)
            {
                // Tính toán độ lệch tương đối (Delta) so với Khối Neo
                int deltaX = posData.x - anchorDataPos.x;
                int deltaY = posData.y - anchorDataPos.y;

                // Chiếu độ lệch đó lên ô lưới bàn cờ (Trục Y dùng dấu trừ do ma trận ngược của bàn cờ)
                int targetX = anchorCell.x + deltaX;
                int targetY = anchorCell.y - deltaY;

                Cell targetCell = board.GetCell(targetX, targetY);
                if (targetCell != null)
                {
                    targetCell.SetState(true); // Đổi trạng thái ô thành ĐÃ ĐẦY
                }
            }

            _isPlaced = true;
            Debug.Log("Đã lấp đầy các ô trên bàn cờ cho khối liền thành công!");
        }
        else
        {
            Debug.Log("Vị trí của khối liền bị vướng hoặc lọt ra ngoài bảng!");
            ReturnToQueue();
        }
    }
    private void ReturnToQueue()
    {
        transform.position = _originalPosition;
        _isPlaced = false;
    }
    
    private bool CanPlaceWholeBody(Board board, Cell anchorCell, Vector2Int anchorDataPos)
    {
        foreach (Vector2Int posData in occupiedCells)
        {
            // Tính khoảng cách lệch tương đối của ô này so với ô Neo
            int deltaX = posData.x - anchorDataPos.x;
            int deltaY = posData.y - anchorDataPos.y;

            // Tính vị trí ô lưới mục tiêu trên bàn cờ
            int targetX = anchorCell.x + deltaX;
            int targetY = anchorCell.y - deltaY;

            Cell targetCell = board.GetCell(targetX, targetY);

            // Chỉ cần 1 ô vuông con bị lọt ra ngoài bảng hoặc đè lên ô đã có mảnh khác -> Thất bại
            if (targetCell == null || targetCell.isFilled)
            {
                return false;
            }
        }
        return true;
    }
    public void SaveOriginalPosition()
    {
        _originalPosition = this.transform.position;
        Debug.Log($"Đã lưu vị trí ban đầu của khối tại: {_originalPosition}");
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