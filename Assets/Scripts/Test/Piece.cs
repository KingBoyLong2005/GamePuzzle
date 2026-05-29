using UnityEngine;
using System.Collections.Generic;

public class Piece : MonoBehaviour
{
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject ScrollViewContent;

    // Public để GameManager truyền data vào
    [HideInInspector] public LevelBoardData levelData;

    private GameObject contentParent;
    private bool _scrollViewBuilt = false;


    private void Start() { }

    public void RebuildPieces()
    {
        if (levelData == null)
        {
            Debug.LogError("[Piece] RebuildPieces: levelData chưa được gán!");
            return;
        }

        EnsureScrollView();
        ClearPieces();
        BuildPiece();
    }

    private void EnsureScrollView()
    {
        if (_scrollViewBuilt) return;

        if (ScrollViewContent == null)
        {
            Debug.LogError("[Piece] ScrollViewContent chưa được gán trong Inspector!");
            return;
        }

        Vector3 posScrollView = new Vector3(-7f, 0, 0);
        var scrollViewObject = Instantiate(ScrollViewContent, posScrollView, Quaternion.identity);
        contentParent = scrollViewObject.transform.Find("Content").gameObject;
        _scrollViewBuilt = true;
    }

    private void ClearPieces()
    {
        if (contentParent == null) return;
        foreach (Transform child in contentParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildPiece()
    {
        float spawnOffsetX = 0f;
        float spawnOffsetY = 0f;

        foreach (var piece in levelData.pieces)
        {
            GameObject pieceObject = new GameObject("Piece_Generated_Shape");
            pieceObject.transform.SetParent(contentParent.transform);
            pieceObject.transform.position = new Vector3(
                transform.position.x + spawnOffsetX,
                transform.position.y - spawnOffsetY,
                5
            );

            DraggablePiece dragScript = pieceObject.AddComponent<DraggablePiece>();
            dragScript.occupiedCells = new List<Vector2Int>();

            foreach (var pos in piece.positions)
            {
                Vector2Int safePos = new Vector2Int(
                    Mathf.RoundToInt(pos.x),
                    Mathf.RoundToInt(pos.y)
                );
                dragScript.occupiedCells.Add(safePos);

                if (piecePrefab != null)
                {
                    GameObject newCell = Instantiate(piecePrefab, gameObject.transform);
                    newCell.transform.position = new Vector3(
                        transform.position.x + pos.x,
                        pieceObject.transform.position.y + pos.y,
                        0
                    );
                    newCell.transform.SetParent(pieceObject.transform);
                }
                else
                {
                    Debug.LogWarning("[Piece] piecePrefab chưa được gán trong Inspector!");
                }
            }

            dragScript.SaveOriginalPosition();

            spawnOffsetX += 3f;
            spawnOffsetY += 3f;
        }
    }
}