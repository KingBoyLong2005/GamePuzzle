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

    public void OnDragging(Vector3 worldPos)
    {
        // Di chuyển mảnh ghép theo ngón tay/chuột
        transform.position = worldPos;
        Debug.Log("Đang kéo mảnh ghép: " + gameObject.name);

        // NÂNG CAO: Bạn có thể thêm logic Ghost Preview tại đây 
        // để hiển thị mảnh ghép mờ mờ trên lưới trước khi thả
    }

    public void OnDragEnd(Vector3 worldPos)
    {
        transform.localScale = Vector3.one;
        Board board = Object.FindFirstObjectByType<Board>();
        if (board == null)
        {
            ReturnToQueue();
            return;
        }

        Cell rootCell = board.GetCellFromWorldPos(worldPos);

        if (rootCell != null)
        {
            if (CanPlaceAt(board, rootCell))
            {
                transform.position = new Vector3(rootCell.transform.position.x, rootCell.transform.position.y, -1f);

                foreach (Vector2Int offset in occupiedCells)
                {
                    // Tính tọa độ ô thực tế dựa trên ô gốc (rootCell)
                    int targetX = rootCell.x + offset.x;
                    int targetY = rootCell.y - offset.y; 

                    board.GetCell(targetX, targetY).SetState(true);
                }
                _isPlaced = true;
                Debug.Log("Đã đặt mảnh ghép");
            }
            else
            {
                Debug.Log("Vị trí bị vướng!");
                ReturnToQueue();
            }
        }
        else
        {
            Debug.Log("Thả ngoài bàn cờ!");
            ReturnToQueue();
        }
    }
    private void ReturnToQueue()
    {
        transform.position = _originalPosition;
        _isPlaced = false;
    }
    private bool CanPlaceAt(Board board, Cell root)
    {
        foreach (Vector2Int offset in occupiedCells)
        {
            int targetX = root.x + offset.x;
            int targetY = root.y - offset.y;

            // Lấy ô Cell thực tế từ Board
            Cell targetCell = board.GetCell(targetX, targetY);

            // Nếu targetCell trả về null (nghĩa là ô này nằm ngoài rìa Board)
            // Hoặc ô đó đã được lấp đầy trước đó (isFilled == true)
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