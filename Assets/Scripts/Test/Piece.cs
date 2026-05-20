using UnityEngine;
using System.Collections.Generic;

public class Piece : MonoBehaviour
{
    [SerializeField] GameObject piecePrefab;
    public LevelBoardData levelData; // Tham chiếu đến ScriptableObject chứa dữ liệu Level
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildPiece();
    }

    void BuildPiece()
    {
        float spawnOffsetX = 0f;

        foreach (var piece in levelData.pieces)
        {
            GameObject pieceObject = new GameObject("Piece_Generated_Shape");
            pieceObject.transform.SetParent(transform); // Đặt PieceObject làm con của GameObject hiện tại
            
            pieceObject.transform.position = new Vector3(transform.position.x + spawnOffsetX, transform.position.y, 0);

            DraggablePiece dragScript = pieceObject.AddComponent<DraggablePiece>();

            //dragScript.occupiedCells = new List<Vector2Int>((IEnumerable<Vector2Int>)piece.positions);
            dragScript.occupiedCells = new List<Vector2Int>();

            foreach (var pos in piece.positions)
            {
                Vector2Int safePos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
                dragScript.occupiedCells.Add(safePos);
                if (piecePrefab != null) 
                {
                    GameObject newPiece = Instantiate(piecePrefab, gameObject.transform);
                    newPiece.transform.position = new Vector3(transform.position.x + pos.x, transform.position.y + pos.y, 0);
                    newPiece.transform.SetParent(pieceObject.transform);
                }
                else
                {                     
                    Debug.LogWarning("Piece Prefab chưa được gán trong Inspector!");
                }
            }
            
            dragScript.SaveOriginalPosition();

            spawnOffsetX += 3f; // Điều chỉnh khoảng cách giữa các mảnh ghép khi sinh ra
        }

    }
}
