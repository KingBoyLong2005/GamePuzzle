using UnityEngine;

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
        foreach (var piece in levelData.pieces)
        {
            GameObject PieceObject = new GameObject("Piece L");
            PieceObject.transform.SetParent(transform); // Đặt PieceObject làm con của GameObject hiện tại
            foreach (var pos in piece.positions)
            {
                GameObject newPiece = Instantiate(piecePrefab, gameObject.transform);
                newPiece.transform.position = new Vector3(transform.position.x + pos.x, transform.position.y + pos.y, 0);
                newPiece.transform.SetParent(PieceObject.transform); 
            }
        }
    }
}
