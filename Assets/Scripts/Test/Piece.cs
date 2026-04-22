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
            foreach (var pos in piece.positions)
            {
                GameObject newPiece = Instantiate(piecePrefab, transform);
                newPiece.transform.position = new Vector3(pos.x, pos.y, 0);
            }
        }
    }
}
